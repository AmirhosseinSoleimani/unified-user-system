namespace UnifiedUserSystem.src.UnifiedUserSystem.Domain.Abstractions
{
    public interface IEntity : IObjectState, ICloneable
    {
        object this[string name] { get; set; }
        void SetDefaultID();
        string IdString { get; }
        Type KeyType { get; }
    }
}
