namespace Academy.src.Shared.Academy.SharedKernel.Domain.Abstractions;

public interface IEntity
{
}


public interface IEntity<TKey> : IEntity
    where TKey : notnull
{
    TKey Id { get; }
}