namespace UnifiedUserSystem.src.Domain.Common
{
    public class Guard
    {
        public static void NotNull(object? value, string name)
        {
            if (value is null) throw new DomainException($"{name} is null");
        }
        public static string NotEmpty(string? value, string name)
        {
            var v = (value ?? "").Trim();
            if (string.IsNullOrWhiteSpace(v))
                throw new DomainException($"{name} is required.");
            return v;
        }
        public static void MaxLen(string value, int max, string name)
        {
            if (value.Length > max)
                throw new DomainException($"{name} max length is {max}.");
        }
        public static void MinLen(string value, int min, string name)
        {
            if (value.Length < min)
                throw new DomainException($"{name} min length is {min}");
        }
        public static void AllowedLen(string value, int max, int min, string name)
        {
            if (value.Length > max || value.Length < min)
                throw new DomainException($"{name} must be between {min} and {max} characters.");
        }
        public static void True(bool condition, string message)
        {
            if (!condition) throw new DomainException(message);
        }
    }
}
