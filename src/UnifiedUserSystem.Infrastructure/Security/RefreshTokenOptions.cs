namespace UnifiedUserSystem.src.Infrastructure.Security
{
    public class RefreshTokenOptions
    {
        public int ExpiresDays { get; set; } = 7;
        public int TokenSizeBytes { get; set; } = 64;
    }
}
