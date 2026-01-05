
using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.src.UnifiedUserSystem.Domain.Entities
{
    public class Role : AuditableEntity<int>
    {
        public string Name { get; private set; } = default!;
        public Role() { }
    }
}
