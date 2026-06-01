namespace UnifiedUserSystem.src.Application.Interfaces.Security
{
    public interface ICurrentUser
    {
        Guid? UserId { get; }
        bool IsAuthenticated { get; }
    }
}
