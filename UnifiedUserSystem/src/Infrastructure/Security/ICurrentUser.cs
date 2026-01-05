namespace UnifiedUserSystem.src.Infrastructure.Security
{
    public interface ICurrentUser
    {
        Guid? UserId { get; }
    }
}
