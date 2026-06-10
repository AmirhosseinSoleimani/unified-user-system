namespace Academy.src.Shared.Academy.SharedKernel.Domain.Exceptions;

public sealed class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, object id)
        : base($"{entityName} with id '{id}' was not found.")
    {
        EntityName = entityName;
        Id = id;
    }

    public string EntityName { get; }
    public object Id { get; }
}