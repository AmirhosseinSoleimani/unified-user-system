namespace UnifiedUserSystem.src.UnifiedUserSystem.Application.Interfaces
{
    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string password, string passwordHasd);
    }
}
