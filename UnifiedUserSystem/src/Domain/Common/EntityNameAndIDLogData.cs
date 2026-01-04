using UnifiedUserSystem.src.UnifiedUserSystem.Domain.Abstractions;

namespace UnifiedUserSystem.src.UnifiedUserSystem.Domain.Common
{
    public class EntityNameAndIDLogData : IObjectState
    {
        public DateTime? InsertDateTime { get; set; }
        public string? InsertUserName { get; set; }
        public int? InsertUserId { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public string? UpdateUserName { get; set; }
        public int? UpdateUserId { get; set; }
        public ObjectState ObjectState { get; set; } = ObjectState.Unchanged;
        public int ID { get; set; }
    }
}
