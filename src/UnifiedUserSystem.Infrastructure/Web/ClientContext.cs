using Microsoft.AspNetCore.Http;
using UnifiedUserSystem.src.Application.Abstractions.Web;

namespace UnifiedUserSystem.src.Infrastructure.Web
{
    public class ClientContext : IClientContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClientContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? DeviceName => GetHeaderValue("X-Device-Name");

        public string? UserAgent => GetHeaderValue("User-Agent");

        public string? ClientId => GetHeaderValue("X-Client-Id");

        public string? IpAddress
        {
            get
            {
                var forwardedFor = GetHeaderValue("X-Forwarded-For");
                if (!string.IsNullOrWhiteSpace(forwardedFor))
                {
                    return forwardedFor
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .FirstOrDefault();
                }

                var remoteIp = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
                return Normalize(remoteIp);
            }
        }

        private string? GetHeaderValue(string headerName)
        {
            var value = _httpContextAccessor.HttpContext?.Request.Headers[headerName].ToString();
            return Normalize(value);
        }

        private static string? Normalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return value.Trim();
        }
    }
}
