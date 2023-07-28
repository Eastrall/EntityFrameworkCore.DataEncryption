using MessagePack;

namespace Microsoft.EntityFrameworkCore.DataEncryption.Serialization;

public sealed class BinarySerializationProvider : ISerializationProvider
{
    public byte[] Serialize<TModel>(TModel input)
    {
        return MessagePackSerializer.Serialize(input);
    }

    public TModel Deserialize<TModel>(byte[] input)
    {
        return MessagePackSerializer.Deserialize<TModel>(input);
    }
}
