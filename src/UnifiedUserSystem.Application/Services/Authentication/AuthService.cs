using UnifiedUserSystem.src.Application.Abstractions.Persistence;
using UnifiedUserSystem.src.Application.Abstractions.Security;
using UnifiedUserSystem.src.Application.Abstractions.Services;
using UnifiedUserSystem.src.Application.Abstractions.Time;
using UnifiedUserSystem.src.Application.Abstractions.Web;
using UnifiedUserSystem.src.Application.Options;
using UnifiedUserSystem.src.Application.Validation;
using UnifiedUserSystem.src.Contracts.DTOs.Auth;
using UnifiedUserSystem.src.Contracts.DTOs.Security;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Identity.Entities;
using UnifiedUserSystem.src.Domain.Security.Entities;
using UnifiedUserSystem.src.Domain.Security.Enums;
using Microsoft.Extensions.Options;


namespace UnifiedUserSystem.Application.Services.Authentication;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;
    private readonly IRegistrationRequestValidator _registrationValidator;
    private readonly ILoginRequestValidator _loginValidator;
    private readonly IJwtTokenService _jwt;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly IClientContext _clientContext;
    private readonly IAuthProtectionService _authProtectionService;
    private readonly ISecuritySettingsService _securitySettingsService;
    private readonly IOtpGenerator _otpGenerator;
    private readonly IOtpHasher _otpHasher;
    private readonly IEmailOtpSender _emailOtpSender;
    private readonly ISmsOtpSender _smsOtpSender;
    private readonly MfaOptions _mfaOptions;


    public AuthService(
        IUnitOfWork uow,
        IPasswordHasher hasher,
        IRegistrationRequestValidator registrationValidator,
        ILoginRequestValidator loginValidator,
        IJwtTokenService jwt,
        IRefreshTokenService refreshTokens,
        IClock clock,
        ICurrentUser currentUser,
        IClientContext clientContext,
        IAuthProtectionService authProtectionService,
        ISecuritySettingsService securitySettingsService,
        IOtpGenerator otpGenerator,
        IOtpHasher otpHasher,
        IEmailOtpSender emailOtpSender,
        ISmsOtpSender smsOtpSender,
        IOptions<MfaOptions> mfaOptions)
        {
        _uow = uow;
        _hasher = hasher;
        _registrationValidator = registrationValidator;
        _loginValidator = loginValidator;
        _jwt = jwt;
        _refreshTokenService = refreshTokens;
        _clock = clock;
        _currentUser = currentUser;
        _clientContext = clientContext;
        _authProtectionService = authProtectionService;
        _securitySettingsService = securitySettingsService;
        _otpGenerator = otpGenerator;
        _otpHasher = otpHasher;
        _emailOtpSender = emailOtpSender;
        _smsOtpSender = smsOtpSender;
        _mfaOptions = mfaOptions.Value;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest req, CancellationToken ct = default)
    {
        _registrationValidator.Validate(req);

        var email = User.NormalizeEmail(req.Email);
        var username = User.NormalizeUsername(req.Username);
        var firstName = User.NormalizeFirstName(req.FirstName);
        var lastName = User.NormalizeLastName(req.LastName);
        var phoneNumber = User.NormalizePhoneNumber(req.PhoneNumber);

        if (await _uow.Users.EmailExistsAsync(email))
            throw BusinessConflictException.For(DomainErrorCodes.EmailAlreadyExists);

        if (await _uow.Users.UsernameExistsAsync(username))
            throw BusinessConflictException.For(DomainErrorCodes.UsernameAlreadyExists);

        if (await _uow.Users.PhoneNumberExistsAsync(phoneNumber))
            throw BusinessConflictException.For(DomainErrorCodes.PhoneNumberAlreadyExists);

        var defaultRoleId = (int)AppRole.User;
        var defaultRoleKey = Role.NormalizeKey(nameof(AppRole.User));
        var role = await _uow.Roles.FindByIdAsync(defaultRoleId, ct)
            ?? throw DomainException.For(DomainErrorCodes.DefaultRoleNotFound);

        var now = _clock.Utcnow;
        var passwordHash = _hasher.Hash(req.Password);

        var user = User.CreateNew(
            email,
            username,
            firstName,
            lastName,
            phoneNumber,
            passwordHash,
            now,
            actorUserId: null);

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
        return BuildAuthResponse(
            user,
            accessToken,
            refreshToken,
            refreshSession.ExpiresAtUtc,
            fallbackRoles: [role.Name]
            );
    }
    public async Task<LoginResponse?> LoginAsync(LoginRequest req, CancellationToken ct = default)
    {
        _loginValidator.Validate(req);

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

        var settings = await _securitySettingsService.GetEffectiveAsync(ct);

        if (!settings.IsMfaEnabled)
        {
            var authResponse = await IssueTokensAsync(user, ct);

            await _authProtectionService.ResetAsync(
                normalizedLoginIdentifier,
                _clientContext,
                ct);

            await _uow.SaveChangesAsync(ct);

            return LoginResponse.Authenticated(authResponse);
        }

        var availableChannels = GetAvailableMfaChannels(settings);

        if (availableChannels.Length == 0)
            throw new DomainException("MFA is enabled but no OTP channel is enabled.");

        var selectedChannel = ResolveRequestedChannel(req.MfaChannel, availableChannels);
        var otpCode = _otpGenerator.Generate(_mfaOptions.OtpLength);
        var now = _clock.Utcnow;
        var expiresAt = now.AddMinutes(settings.OtpExpirationMinutes);

        var challenge = MfaChallenge.Create(
            user.Id,
            selectedChannel,
            "pending",
            now,
            expiresAt,
            settings.OtpMaxAttempts,
            user.Id);

        var otpHash = _otpHasher.Hash(otpCode, challenge.Id);
        challenge = MfaChallenge.Create(
            user.Id,
            selectedChannel,
            otpHash,
            now,
            expiresAt,
            settings.OtpMaxAttempts,
            user.Id);

        _uow.MfaChallenges.Add(challenge);

        await SendOtpAsync(user, selectedChannel, otpCode, expiresAt, ct);

        await _authProtectionService.ResetAsync(
            normalizedLoginIdentifier,
            _clientContext,
            ct);

        await _uow.SaveChangesAsync(ct);

        return LoginResponse.Challenge(new MfaChallengeResponse
        {
            ChallengeId = challenge.Id,
            Channel = selectedChannel.ToString(),
            AvailableChannels = availableChannels.Select(x => x.ToString()).ToArray(),
            ExpiresAt = expiresAt
        });
    }

    public async Task<AuthResponse?> VerifyMfaAsync(VerifyMfaRequest req, CancellationToken ct = default)
    {
        if (req is null)
            throw new DomainException("Request is null.");

        if (req.ChallengeId == Guid.Empty)
            throw new DomainException("ChallengeId is required.");

        Guard.NotEmpty(req.OtpCode, nameof(req.OtpCode));

        var challenge = await _uow.MfaChallenges.FindByIdAsync(req.ChallengeId, ct);
        if (challenge is null || challenge.User is null || !challenge.User.IsActive)
            return null;

        var settings = await _securitySettingsService.GetEffectiveAsync(ct);
        var availableChannels = GetAvailableMfaChannels(settings);

        if (!settings.IsMfaEnabled || !availableChannels.Contains(challenge.Channel))
            throw new DomainException("MFA channel is disabled.");

        var now = _clock.Utcnow;
        var otpMatches = _otpHasher.Verify(req.OtpCode, challenge.Id, challenge.OtpHash);

        challenge.Verify(otpMatches, now, challenge.UserId);

        if (!otpMatches)
        {
            await _uow.SaveChangesAsync(ct);
            return null;
        }

        var response = await IssueTokensAsync(challenge.User, ct);
        await _uow.SaveChangesAsync(ct);

        return response;
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
        DateTimeOffset refreshTokenExpiresAtUtc,
        string[]? fallbackRoles = null
        )
    {
        var roles = user.UserRoles
            .Select(x => x.Role?.Name)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .Cast<string>()
            .ToArray();

        if (roles.Length == 0 && fallbackRoles is { Length: > 0 })
            roles = fallbackRoles;

        if (roles.Length == 0)
            roles = new[] { "user" };

        return new AuthResponse(
            user.Id,
            user.Email,
            user.Username,
            user.FirstName,
            user.LastName,
            user.PhoneNumber,
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

    private async Task<AuthResponse> IssueTokensAsync(User user, CancellationToken ct)
    {
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

        var accessToken = _jwt.CreateAccessToken(user);
        return BuildAuthResponse(user, accessToken, refreshToken, refreshSession.ExpiresAtUtc);
    }

    private static MfaChannel[] GetAvailableMfaChannels(SecuritySettingsResponse settings)
    {
        if (!settings.IsMfaEnabled || !settings.IsOtpEnabled)
            return Array.Empty<MfaChannel>();

        var channels = new List<MfaChannel>();

        if (settings.IsEmailOtpEnabled)
            channels.Add(MfaChannel.Email);

        if (settings.IsPhoneOtpEnabled)
            channels.Add(MfaChannel.Phone);

        return channels.ToArray();
    }

    private static MfaChannel ResolveRequestedChannel(string? requestedChannel, MfaChannel[] availableChannels)
    {
        if (availableChannels.Length == 0)
            throw new DomainException("No MFA channels are available.");

        if (string.IsNullOrWhiteSpace(requestedChannel))
            return availableChannels[0];

        if (!Enum.TryParse<MfaChannel>(requestedChannel.Trim(), ignoreCase: true, out var parsed) ||
            !Enum.IsDefined(typeof(MfaChannel), parsed))
        {
            throw new DomainException("MFA channel is invalid.");
        }

        if (!availableChannels.Contains(parsed))
            throw new DomainException("MFA channel is disabled.");

        return parsed;
    }

    private Task SendOtpAsync(
        User user,
        MfaChannel channel,
        string otpCode,
        DateTimeOffset expiresAt,
        CancellationToken ct)
    {
        return channel switch
        {
            MfaChannel.Email => _emailOtpSender.SendAsync(user, otpCode, expiresAt, ct),
            MfaChannel.Phone => _smsOtpSender.SendAsync(user, otpCode, expiresAt, ct),
            _ => throw new DomainException("MFA channel is invalid.")
        };
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