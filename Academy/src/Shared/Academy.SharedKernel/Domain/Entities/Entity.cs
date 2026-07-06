using Academy.src.Shared.Academy.SharedKernel.Domain.Abstractions;

namespace Academy.src.Shared.Academy.SharedKernel.Domain.Entities;

public abstract class Entity<TKey>: IEntity<TKey>
    where TKey : notnull
{
    public TKey Id { get; protected set; } = default!;

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TKey> other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (EqualityComparer<TKey>.Default.Equals(Id, default!) ||
            EqualityComparer<TKey>.Default.Equals(other.Id, default!))
            return false;

        return GetType() == other.GetType()
            && EqualityComparer<TKey>.Default.Equals(Id, other.Id);
    }

    public override int GetHashCode()
    {
        return EqualityComparer<TKey>.Default.Equals(Id, default!)
            ? base.GetHashCode()
            : HashCode.Combine(GetType(), Id);
    }

    public static bool operator ==(Entity<TKey>? left, Entity<TKey>? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Entity<TKey>? left, Entity<TKey>? right)
    {
        return !Equals(left, right);
    }
}
