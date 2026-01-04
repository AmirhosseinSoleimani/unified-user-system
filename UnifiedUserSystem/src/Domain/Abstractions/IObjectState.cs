using UnifiedUserSystem.src.UnifiedUserSystem.Domain.Common;

namespace UnifiedUserSystem.src.UnifiedUserSystem.Domain.Abstractions
{
    public interface IObjectState
    {
        ObjectState ObjectState { get; set;}
    }
}
