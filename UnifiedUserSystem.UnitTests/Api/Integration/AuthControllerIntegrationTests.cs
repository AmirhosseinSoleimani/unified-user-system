using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using UnifiedUserSystem.src.Contracts.Common;
using UnifiedUserSystem.src.Contracts.DTOs.Auth;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;

namespace UnifiedUserSystem.UnitTests.Api.Integration
{
    public class AuthControllerIntegrationTests : IClassFixture<AuthApiFactory>
    {
        private readonly AuthApiFactory _factory;
        private readonly HttpClient _client;

        public AuthControllerIntegrationTests(AuthApiFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task PostRegister_ShouldReturnAuthResponseWithRefreshToken()
        {
            var response = await RegisterAsync("register1@example.com", "register1");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var payload = await ReadAuthResponseAsync(response);
            payload.Data!.AccessToken.Should().NotBeNullOrWhiteSpace();
            payload.Data.RefreshToken.Should().NotBeNullOrWhiteSpace();
            payload.Data.RefreshTokenExpiresAtUtc.Should().BeAfter(DateTimeOffset.UtcNow);
        }

        [Fact]
        public async Task PostLogin_ShouldReturnAuthResponseWithRefreshToken()
        {
            await RegisterAsync("login1@example.com", "login1");

            var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
            {
                EmailOrUsername = "login1@example.com",
                Password = "Password123!"
            });

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var payload = await ReadAuthResponseAsync(response);
            payload.Data!.AccessToken.Should().NotBeNullOrWhiteSpace();
            payload.Data.RefreshToken.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task PostRefresh_WithValidRefreshToken_ShouldReturnRotatedRefreshToken()
        {
            var registered = await RegisterAndReadAsync("refresh1@example.com", "refresh1");

            var response = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest
            {
                RefreshToken = registered.RefreshToken
            });

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var refreshed = (await ReadAuthResponseAsync(response)).Data!;
            refreshed.AccessToken.Should().NotBeNullOrWhiteSpace();
            refreshed.RefreshToken.Should().NotBe(registered.RefreshToken);
        }

        [Fact]
        public async Task PostRefresh_WithOldRefreshTokenAfterRotation_ShouldReturnUnauthorized()
        {
            var registered = await RegisterAndReadAsync("reuse1@example.com", "reuse1");

            var firstRefresh = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest
            {
                RefreshToken = registered.RefreshToken
            });
            firstRefresh.StatusCode.Should().Be(HttpStatusCode.OK);

            var reuseResponse = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest
            {
                RefreshToken = registered.RefreshToken
            });

            reuseResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task PostLogout_WithValidRefreshToken_ShouldReturnOk()
        {
            var registered = await RegisterAndReadAsync("logout1@example.com", "logout1");

            var request = CreateAuthorizedPostRequest(
                "/api/auth/logout",
                registered.AccessToken,
                JsonContent.Create(new LogoutRequest
                {
                    RefreshToken = registered.RefreshToken
                }));

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task PostLogout_WithInvalidRefreshToken_ShouldReturnUnauthorized()
        {
            var registered = await RegisterAndReadAsync("logout2@example.com", "logout2");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", registered.AccessToken);

            var response = await _client.PostAsJsonAsync("/api/auth/logout", new LogoutRequest
            {
                RefreshToken = "invalid-refresh-token"
            });

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task PostRevokeAllSessions_ShouldRevokeAllCurrentUserSessions()
        {
            var registered = await RegisterAndReadAsync("revoke1@example.com", "revoke1");

            var request = CreateAuthorizedPostRequest(
            "/api/auth/revoke-all-sessions",
            registered.AccessToken);

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.RefreshTokenSessions
                .Where(x => x.UserId == registered.Id)
                .Should()
                .OnlyContain(x => x.RevokedAtUtc != null);
        }

        [Fact]
        public async Task PostRefresh_AfterRevokeAllSessions_ShouldReturnUnauthorized()
        {
            var registered = await RegisterAndReadAsync("revoke2@example.com", "revoke2");

            var revokeRequest = CreateAuthorizedPostRequest(
                "/api/auth/revoke-all-sessions",
                registered.AccessToken);

            var revokeResponse = await _client.SendAsync(revokeRequest);
            revokeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            revokeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest
            {
                RefreshToken = registered.RefreshToken
            });

            refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task PostRefresh_WithMalformedRequest_ShouldReturnBadRequest()
        {
            var response = await _client.PostAsJsonAsync("/api/auth/refresh", new { });

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task PostLogout_WithMalformedRequest_ShouldReturnBadRequest()
        {
            var registered = await RegisterAndReadAsync("malformedlogout@example.com", "malogout");

            var request = CreateAuthorizedPostRequest(
            "/api/auth/logout",
            registered.AccessToken,
            JsonContent.Create(new { }));

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task PostRevokeAllSessions_WithoutAccessToken_ShouldReturnUnauthorized()
        {
            var response = await _client.PostAsync("/api/auth/revoke-all-sessions", null);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task PostRegister_Response_ShouldNotExposeRefreshTokenHash()
        {
            var response = await RegisterAsync("hash1@example.com", "hashuser1");
            var body = await response.Content.ReadAsStringAsync();

            body.Should().NotContain("refreshTokenHash");
            body.Should().NotContain("RefreshTokenHash");
        }

        [Fact]
        public async Task PostLogin_Response_ShouldNotExposeRefreshTokenHash()
        {
            await RegisterAsync("hash2@example.com", "hashuser2");

            var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
            {
                EmailOrUsername = "hash2@example.com",
                Password = "Password123!"
            });

            var body = await response.Content.ReadAsStringAsync();

            body.Should().NotContain("refreshTokenHash");
            body.Should().NotContain("RefreshTokenHash");
        }

        private async Task<HttpResponseMessage> RegisterAsync(string email, string username)
        {
            return await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
            {
                Email = email,
                Username = username,
                FullName = "Test User",
                Password = "Password123!"
            });
        }

        private async Task<AuthResponse> RegisterAndReadAsync(string email, string username)
        {
            var response = await RegisterAsync(email, username);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            return (await ReadAuthResponseAsync(response)).Data!;
        }

        private static async Task<ApiResponse<AuthResponse>> ReadAuthResponseAsync(HttpResponseMessage response)
        {
            var payload = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
            payload.Should().NotBeNull();
            payload!.Success.Should().BeTrue();
            payload.Data.Should().NotBeNull();

            return payload;
        }

        private static HttpRequestMessage CreateAuthorizedPostRequest(string uri, string accessToken, HttpContent? content = null)
        {
            accessToken.Should().NotBeNullOrWhiteSpace();

            var request = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = content
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            return request;
        }
    }
}