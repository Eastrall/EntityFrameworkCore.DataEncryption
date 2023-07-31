using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.DataEncryption.Serialization;

public sealed class STJSerializationProvider : ISerializationProvider
{
    public byte[] Serialize<TModel>(TModel input)
    {
        return JsonSerializer.SerializeToUtf8Bytes(input);
    }

    public TModel Deserialize<TModel>(byte[] input)
    {
        return JsonSerializer.Deserialize<TModel>(input);
    }
}
