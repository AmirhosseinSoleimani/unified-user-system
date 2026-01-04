using UnifiedUserSystem.src.UnifiedUserSystem.Domain.Abstractions;

namespace UnifiedUserSystem.src.UnifiedUserSystem.Domain.Common
{
    public class LoggableEntityNameAndID<TKey> : Entity<TKey>, ILoggableEntityNameAndID
        where TKey : struct
    {
        public LoggableEntityNameAndID() 
        {
            LogData = new EntityNameAndIDLogData();
        }
        public virtual EntityNameAndIDLogData LogData { get; set; }
        public EntityNameAndIDLogData logData { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }

    public class LoggableEntityNameAndID : LoggableEntityNameAndID<int> { }
}
