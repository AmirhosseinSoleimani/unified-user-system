namespace UnifiedUserSystem.src.Infrastructure.Security
{
    public interface IPasswordPolicy
    {
        void Validate(string password);
    }
}
