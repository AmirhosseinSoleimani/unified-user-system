namespace UnifiedUserSystem.src.Domain.Common
{
    public class Entity<TKey> where TKey : struct
    {
        public TKey Id { get; protected set; }
    }
}
