using System.Globalization;
using System.Reflection;
using UnifiedUserSystem.src.UnifiedUserSystem.Domain.Abstractions;

namespace UnifiedUserSystem.src.UnifiedUserSystem.Domain.Common
{
    public class Entity<TKey> : IEntity<TKey>, IEquatable<Entity<TKey>>
        where TKey : struct
    {
        public object this[string propName]
        {
            get
            {
                var prop = GetProperty(propName);
                return prop.GetValue(this, null);
            }
            set
            {
                var prop = GetProperty(propName);
                SetPropertyValue(prop, value);
            }
        }

        public ObjectState ObjectState { get; set; }
        public TKey ID { get; set; }

        public string IdString => ID.ToString();

        public Type KeyType => typeof(TKey);

        public object Clone() => MemberwiseClone();

        public void SetDefaultID()
        {
            switch (ID)
            {
                case int intId when intId < 0:
                    ID = default;
                    break;
                case long longId when longId < 0:
                    ID = default;
                    break;
                case short shortId when shortId < 0:
                    ID = default;
                    break;
                default:
                    break;
            }
        }

        public bool Equals(Entity<TKey> other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return EqualityComparer<TKey>.Default.Equals(other);
        }
        public override bool Equals(object? obj)
        {
            return obj is Entity<TKey> other && Equals(other);
        }
        public override int GetHashCode()
        {
            return EqualityComparer<TKey>.Default.GetHashCode(ID);
        }

        //Helper
        private PropertyInfo GetProperty(string propName)
        {
            if (string.IsNullOrWhiteSpace(propName))
            {
                throw new ArgumentException("Property name is empty.", nameof(propName));
            }
            var prop = GetType().GetProperty(
                propName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase
                );
            if (prop == null)
                throw new ArgumentException($"Property '{propName}' not found on type '{GetType().Name}'.");
            if (!prop.CanRead || !prop.CanWrite)
                throw new InvalidOperationException($"Property '{propName}' must be readable and writable.");
            return prop;
        }

        private void SetPropertyValue(PropertyInfo pInfo, object? value)
        {
            if (value is string s && pInfo.PropertyType.IsEnum)
            {
                if (int.TryParse(s, out var intValue))
                    pInfo.SetValue(this, Enum.ToObject(pInfo.PropertyType, intValue), null);
                else
                    pInfo.SetValue(this, Enum.Parse(pInfo.PropertyType, s, ignoreCase: true), null);
                return;
            }
            if (pInfo.PropertyType.IsGenericType &&
            pInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var underlyingType = Nullable.GetUnderlyingType(pInfo.PropertyType)!;

                if (value is string str && underlyingType.IsEnum)
                {
                    object enumObj = int.TryParse(str, out var intValue)
                        ? Enum.ToObject(underlyingType, intValue)
                        : Enum.Parse(underlyingType, str, ignoreCase: true);

                    pInfo.SetValue(this, Convert.ChangeType(enumObj, underlyingType, CultureInfo.InvariantCulture), null);
                    return;
                }

                if (value is string str2)
                {
                    pInfo.SetValue(this,
                        string.IsNullOrWhiteSpace(str2) ? null : Convert.ChangeType(str2, underlyingType, CultureInfo.InvariantCulture),
                        null);
                    return;
                }

                pInfo.SetValue(this, value is null ? null : Convert.ChangeType(value, underlyingType, CultureInfo.InvariantCulture), null);
                return;
            }

            if (value is string str3)
            {
                pInfo.SetValue(this,
                    string.IsNullOrWhiteSpace(str3) ? null : Convert.ChangeType(str3, pInfo.PropertyType, CultureInfo.InvariantCulture),
                    null);
                return;
            }
            pInfo.SetValue(this, value is null ? null : Convert.ChangeType(value, pInfo.PropertyType, CultureInfo.InvariantCulture), null);
        }
    }
}
