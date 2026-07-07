using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.src.Domain.Authorization.Entities
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
        public static Operation Create(
            string key,
            string title,
            DateTimeOffset nowUtc,
            Guid? actoractorUserId)
        {
            key = NormalizeKey(key);
            title = NormalizeTitle(title);

            ValidateKey(key);
            ValidateTitle(title);

            var operation = new Operation 
            {
                Id = Guid.NewGuid(),
                Key = key,
                Title = title,
                IsActive = true,
            };
            operation.SetCreated(nowUtc, actoractorUserId);
            return operation;
        }
        public void RenameTitle(
            string newTitle,
            DateTimeOffset nowUtc,
            Guid? actorUserId)
        {
            newTitle = NormalizeTitle(newTitle);

            ValidateTitle(newTitle);

            if (Title == newTitle) return;

            Title = newTitle;
            Touch(nowUtc, actorUserId);
        }
        public void ChangeKey(
            string newKey,
            DateTimeOffset nowUtc,
            Guid? actorUserId)
        {
            newKey = NormalizeKey(newKey);

            ValidateKey(newKey);

            if (Key == newKey) return;

            Key = newKey;
            Touch(nowUtc, actorUserId);
        }
        public void Deactivate(
            DateTimeOffset nowUtc,
            Guid? actorUserId)
        {
            if (!IsActive) return;

            IsActive = false;
            Touch(nowUtc, actorUserId);
        }
        public void Activate(
            DateTimeOffset nowUtc,
            Guid? actorUserId)
        {
            if (IsActive) return;

            IsActive = true;
            Touch(nowUtc, actorUserId);
        }

        [Obsolete("Use Deactivate instead.")]
        public void Deactive(
            DateTimeOffset nowUtc,
            Guid? actorUserId)
        {
            Deactivate(nowUtc, actorUserId);
        }

        [Obsolete("Use Activate instead.")]
        public void Active(
            DateTimeOffset nowUtc,
            Guid? actorUserId)
        {
            Activate(nowUtc, actorUserId);
        }

        public void Delete(DateTimeOffset nowUtc, Guid? actorUserId)
            => SoftDelete(nowUtc, actorUserId);

        public void UnDelete(DateTimeOffset nowUtc, Guid? actorUserId)
            => Restore(nowUtc, actorUserId);

        public static string NormalizeKey(string key)
        => (key ?? "").Trim().ToLowerInvariant();

        public static string NormalizeTitle(string title)
        => (title ?? "").Trim();

        private static void ValidateKey(string key)
        {
            Guard.NotEmpty(key, nameof(key));
            Guard.MaxLen(key, KeyMaxLength, nameof(key));

            for(int i = 0; i < key.Length; i++)
            {
                var ch = key[i];
                var isAllowed =
                    ch >= 'a' && ch <= 'z' ||
                    ch >= '0' && ch <= '9' ||
                    ch == '.' || ch == '-' || ch == '_';
                if (!isAllowed)
                    throw new ArgumentException("Operation key contains invalid characters.");
            }
        }

        private static void ValidateTitle(string title)
        {
            Guard.NotEmpty(title, nameof(title));
            Guard.MaxLen(title, TitleMaxLength, nameof(title));
        }
    }
}
