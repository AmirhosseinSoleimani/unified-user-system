using Microsoft.EntityFrameworkCore;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Application.Interfaces.Security;
using UnifiedUserSystem.src.Application.Interfaces.Services;
using UnifiedUserSystem.src.Business.Interfaces;
using UnifiedUserSystem.src.Contracts.DTOs.Auth;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Identity.Entities;
using UnifiedUserSystem.src.Infrastructure.Time;
using UnifiedUserSystem.src.UnifiedUserSystem.Application.Interfaces;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Security;

namespace UnifiedUserSystem.src.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _uow;
        private readonly IPasswordHasher _hasher;
        private readonly IUserBusiness _business;
        private readonly IJwtTokenService _jwt;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IClock _clock;
        private readonly ICurrentUser _currentUser;
        private readonly IClientContext _clientContext;
        private readonly IAuthProtectionService _authProtectionService;

        public AuthService(
            IUnitOfWork uow,
            IPasswordHasher hasher,
            IUserBusiness business,
            IJwtTokenService jwt,
            IRefreshTokenService refreshTokens,
            IClock clock,
            ICurrentUser currentUser,
            IClientContext clientContext,
            IAuthProtectionService authProtectionService)
        {
            _uow = uow;
            _hasher = hasher;
            _business = business;
            _jwt = jwt;
            _refreshTokenService = refreshTokens;
            _clock = clock;
            _currentUser = currentUser;
            _clientContext = clientContext;
            _authProtectionService = authProtectionService;
        }

        public AuthService(
            IUnitOfWork uow,
            IPasswordHasher hasher,
            IUserBusiness business,
            IJwtTokenService jwt,
            IRefreshTokenService refreshTokens,
            IClock clock,
            ICurrentUser currentUser,
            IClientContext clientContext)
            : this(
                uow,
                hasher,
                business,
                jwt,
                refreshTokens,
                clock,
                currentUser,
                clientContext,
                NullAuthProtectionService.Instance)
        {
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest req, CancellationToken ct = default)
        {
            _business.ValidateRegister(req);

            var email = User.NormalizeEmail(req.Email);
            var username = User.NormalizeUsername(req.Username);
            var fullName = User.NormalizeFullname(req.FullName);

            if (await _uow.Users.EmailExistsAsync(email))
                throw new InvalidOperationException("Email already exists.");

            if (await _uow.Users.UsernameExistsAsync(username))
                throw new InvalidOperationException("Username already exists.");

            var defaultRoleId = (int)AppRole.User;
            var role = await _uow.Roles.FindByIdAsync(defaultRoleId, ct)
                ?? throw new InvalidOperationException("Default role not found. Seed roles first.");

            var now = _clock.Utcnow;
            var passwordHash = _hasher.Hash(req.Password);

            var user = User.CreateNew(email, username, fullName, passwordHash, now, actorUserId: null);
            user.AssignRole(roleId: role.Id, now, actorUserId: user.Id);

            _uow.Users.Add(user);

            var refreshToken = _refreshTokenService.GenerateToken();
            var refreshSession = CreateRefreshTokenSession(
                user.Id,
                refreshToken,
                now,
                _clientContext.DeviceName,
                _clientContext.UserAgent,
                _clientContext.IpAddress,
                _clientContext.ClientId);

            _uow.RefreshTokenSessions.Add(refreshSession);

            await _uow.SaveChangesAsync(ct);

            var accessToken = _jwt.CreateAccessToken(user);
            return BuildAuthResponse(user, accessToken, refreshToken, refreshSession.ExpiresAtUtc);
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest req, CancellationToken ct = default)
        {
            _business.ValidateLogin(req);

            var normalizedLoginIdentifier = NormalizeLoginIdentifier(req.EmailOrUsername);

            var protection = await _authProtectionService.CheckAsync(
                normalizedLoginIdentifier,
                _clientContext,
                ct);

            if (!protection.IsAllowed)
                return null;

            var user = await _uow.Users.FindEmailOrUsernameAsync(normalizedLoginIdentifier);

            if (user is null || !user.IsActive)
            {
                await _authProtectionService.RecordFailureAsync(
                    normalizedLoginIdentifier,
                    _clientContext,
                    ct);

                return null;
            }

            if (!_hasher.Verify(req.Password, user.PasswordHash))
            {
                await _authProtectionService.RecordFailureAsync(
                    normalizedLoginIdentifier,
                    _clientContext,
                    ct);

                return null;
            }

            var now = _clock.Utcnow;
            var refreshToken = _refreshTokenService.GenerateToken();

            var refreshSession = CreateRefreshTokenSession(
                user.Id,
                refreshToken,
                now,
                _clientContext.DeviceName,
                _clientContext.UserAgent,
                _clientContext.IpAddress,
                _clientContext.ClientId);

            _uow.RefreshTokenSessions.Add(refreshSession);

            await _authProtectionService.ResetAsync(
                normalizedLoginIdentifier,
                _clientContext,
                ct);

            await _uow.SaveChangesAsync(ct);

            var accessToken = _jwt.CreateAccessToken(user);
            return BuildAuthResponse(user, accessToken, refreshToken, refreshSession.ExpiresAtUtc);
        }

        public async Task<AuthResponse?> RefreshAsync(RefreshTokenRequest req, CancellationToken ct = default)
        {
            if (req is null)
                throw new DomainException("Request is null.");

            var now = _clock.Utcnow;
            var refreshTokenHash = _refreshTokenService.HashToken(req.RefreshToken);

            var currentSession = await _uow.RefreshTokenSessions.FindByHashAsync(refreshTokenHash, ct);
            if (currentSession is null)
                return null;

            if (currentSession.IsRevoked)
            {
                currentSession.MarkReuseDetected(now, currentSession.UserId);
                await RevokeAffectedSessionChainAsync(currentSession, now, ct);
                await _uow.SaveChangesAsync(ct);
                return null;
            }

            if (currentSession.IsExpired(now))
                return null;

            var user = await _uow.Users.FindByIdWithRolesAsync(currentSession.UserId, ct);
            if (user is null || !user.IsActive)
                return null;

            var newRefreshToken = _refreshTokenService.GenerateToken();

            var newSession = CreateRefreshTokenSession(
                user.Id,
                newRefreshToken,
                now,
                _clientContext.DeviceName ?? currentSession.DeviceName,
                _clientContext.UserAgent ?? currentSession.UserAgent,
                _clientContext.IpAddress ?? currentSession.IpAddress,
                _clientContext.ClientId ?? currentSession.ClientId);

            _uow.RefreshTokenSessions.Add(newSession);

            currentSession.Rotate(newSession.Id, now, user.Id);

            await _uow.SaveChangesAsync(ct);

            var accessToken = _jwt.CreateAccessToken(user);
            return BuildAuthResponse(user, accessToken, newRefreshToken, newSession.ExpiresAtUtc);
        }

        public async Task<bool> LogoutAsync(Guid currentUserId, LogoutRequest req, CancellationToken ct = default)
        {
            if (req is null)
                throw new DomainException("Request is null.");

            if (currentUserId == Guid.Empty)
                return false;

            var refreshTokenHash = _refreshTokenService.HashToken(req.RefreshToken);
            var session = await _uow.RefreshTokenSessions.FindByHashAsync(refreshTokenHash, ct);

            if (session is null || session.UserId != currentUserId)
                return false;

            var now = _clock.Utcnow;

            if (session.IsExpired(now) || session.IsRevoked)
                return false;

            session.Revoke(now, currentUserId);

            await _uow.SaveChangesAsync(ct);
            return true;
        }

        public async Task RevokeAllSessionsAsync(Guid userId, CancellationToken ct = default)
        {
            if (userId == Guid.Empty)
                return;

            await RevokeAllActiveSessionsAsync(userId, _clock.Utcnow, ct);
            await _uow.SaveChangesAsync(ct);
        }

        private AuthResponse BuildAuthResponse(
            User user,
            string accessToken,
            string refreshToken,
            DateTimeOffset refreshTokenExpiresAtUtc)
        {
            var roles = user.UserRoles
                .Select(x => x.Role?.Name)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .Cast<string>()
                .ToArray();

            if (roles.Length == 0)
                roles = new[] { "user" };

            return new AuthResponse(
                user.Id,
                user.Email,
                user.Username,
                user.Fullname,
                roles,
                accessToken,
                refreshToken,
                refreshTokenExpiresAtUtc);
        }

        private RefreshTokenSession CreateRefreshTokenSession(
            Guid userId,
            string refreshToken,
            DateTimeOffset nowUtc,
            string? deviceName,
            string? userAgent,
            string? ipAddress,
            string? clientId)
        {
            var refreshTokenHash = _refreshTokenService.HashToken(refreshToken);
            var expiresAtUtc = _refreshTokenService.GetExpiresAtUtc(nowUtc);

            return RefreshTokenSession.Create(
                userId,
                refreshTokenHash,
                nowUtc,
                expiresAtUtc,
                deviceName,
                userAgent,
                ipAddress,
                clientId,
                actorUserId: userId);
        }

        private async Task RevokeAllActiveSessionsAsync(
            Guid userId,
            DateTimeOffset nowUtc,
            CancellationToken ct)
        {
            var activeSessions = await _uow.RefreshTokenSessions.ListActiveByUserIdAsync(userId, nowUtc, ct);

            foreach (var session in activeSessions)
            {
                session.Revoke(nowUtc, userId);
            }
        }

        private async Task RevokeAffectedSessionChainAsync(
            RefreshTokenSession reusedSession,
            DateTimeOffset nowUtc,
            CancellationToken ct)
        {
            var userSessions = await _uow.RefreshTokenSessions.ListByUserIdAsync(reusedSession.UserId, ct)
                ?? Array.Empty<RefreshTokenSession>();

            var sessionsById = userSessions
                .GroupBy(x => x.Id)
                .ToDictionary(x => x.Key, x => x.First());

            if (!sessionsById.ContainsKey(reusedSession.Id))
                sessionsById.Add(reusedSession.Id, reusedSession);

            var visitedSessionIds = new HashSet<Guid>();
            var stack = new Stack<RefreshTokenSession>();
            stack.Push(reusedSession);

            while (stack.Count > 0)
            {
                var session = stack.Pop();

                if (!visitedSessionIds.Add(session.Id))
                    continue;

                if (session.IsActive(nowUtc))
                    session.Revoke(nowUtc, reusedSession.UserId);

                if (session.ReplacedBySessionId is not Guid replacementSessionId)
                    continue;

                if (sessionsById.TryGetValue(replacementSessionId, out var replacementSession))
                    stack.Push(replacementSession);
            }
        }

        private static string NormalizeLoginIdentifier(string emailOrUsername)
            => (emailOrUsername ?? string.Empty).Trim().ToLowerInvariant();

        private sealed class NullAuthProtectionService : IAuthProtectionService
        {
            public static readonly NullAuthProtectionService Instance = new();

            private NullAuthProtectionService()
            {
            }

            public Task<AuthProtectionCheckResult> CheckAsync(
                string normalizedLoginIdentifier,
                IClientContext clientContext,
                CancellationToken ct = default)
            {
                return Task.FromResult(AuthProtectionCheckResult.Allow());
            }

            public Task RecordFailureAsync(
                string normalizedLoginIdentifier,
                IClientContext clientContext,
                CancellationToken ct = default)
            {
                return Task.CompletedTask;
            }

            public Task ResetAsync(
                string normalizedLoginIdentifier,
                IClientContext clientContext,
                CancellationToken ct = default)
            {
                return Task.CompletedTask;
            }
        }
    }
}