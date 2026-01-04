using UnifiedUserSystem.src.UnifiedUserSystem.Domain.Common;

namespace UnifiedUserSystem.src.UnifiedUserSystem.Domain.Abstractions
{
    public interface ILoggableEntityNameAndID : IEntity
    {
        EntityNameAndIDLogData logData { get; set; }
    }
}
