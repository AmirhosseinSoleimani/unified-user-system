using System.ComponentModel.DataAnnotations;

namespace UnifiedUserSystem.src.Contracts.DTOs.Auth
{
    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; } = default!;
    }
}
