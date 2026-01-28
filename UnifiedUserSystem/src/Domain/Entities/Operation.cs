using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.src.UnifiedUserSystem.Domain.Entities
{
    public class Operation : AuditableEntity<Guid>
    {
        public const int KeyMaxLength = 150;
        public const int TitleMaxLength = 250;
        public string Key { get; private set; } = default!;
        public string Title { get; private set; } = default!;
        public bool IsActive { get; private set; } = true;
        public ICollection<RoleOperation> RoleOperations { get; private set; } = new List<RoleOperation>();
        private Operation() { }
        public static Operation Create(string key, string title, DateTimeOffset nowUtc, Guid? actoractorUserId)
        {
            key = NormalizeKey(key);
            title = NormalizeTitle(title);
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Operation key is required.");
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Operation title is required.");

            var op = new Operation 
            {
                Id = Guid.NewGuid(),
                Key = key,
                Title = title,
            };
            op.SetCreated(nowUtc, actoractorUserId);
            return op;
        }
        public void RenameTitle(string newTitle, DateTimeOffset nowUtc, Guid? actorUserId)
        {
            newTitle = NormalizeKey(newTitle);
            ValidateTitle();
        }
        public static string NormalizeKey(string key)
        => (key ?? "").Trim().ToLowerInvariant();
        public static string NormalizeTitle(string title)
        => (title ?? "").Trim();
        private static void ValidateKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Operation key is required.");
            if (key.Length > KeyMaxLength)
                throw new ArgumentException($"Operation key max length is {KeyMaxLength}.");
            for(int i = 0; i < key.Length; i++)
            {
                var ch = key[i];
                var ok =
                    (ch >= 'a' && ch <= 'z') ||
                    (ch >= '0' && ch <= '9') ||
                    ch == '.' || ch == '-' || ch == '_';
                if (!ok)
                    throw new ArgumentException("Operation key contains invalid characters.");
            }
        }
    }
}
