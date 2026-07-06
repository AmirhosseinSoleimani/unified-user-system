namespace Academy.src.Shared.Academy.SharedKernel.Infrastructure.Serialization;

public interface IEventSerializer
{
    string Serialize(object value, Type valueType);
}
