using System.ComponentModel.DataAnnotations;

namespace UnifiedUserSystem.src.Contracts.DTOs.Auth
{
    public class LogoutRequest
    {
        [Required]
        public string RefreshToken { get; set; } = default!;
    }
}
