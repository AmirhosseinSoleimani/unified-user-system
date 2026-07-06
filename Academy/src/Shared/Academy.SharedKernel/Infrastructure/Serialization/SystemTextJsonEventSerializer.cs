using System.Text.Json;

namespace Academy.src.Shared.Academy.SharedKernel.Infrastructure.Serialization;

public sealed class SystemTextJsonEventSerializer : IEventSerializer
{
    private static readonly JsonSerializerOptions DefaultOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    private readonly JsonSerializerOptions _options;

    public SystemTextJsonEventSerializer()
        : this(DefaultOptions)
    {
    }

    public SystemTextJsonEventSerializer(JsonSerializerOptions options)
    {
        _options = options;
    }

    public string Serialize(object value, Type valueType)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(valueType);

        return JsonSerializer.Serialize(value, valueType, _options);
    }
}

