using System;

namespace Microsoft.EntityFrameworkCore.DataEncryption.Internal;

internal sealed class ByteArraySerializationProvider : ISerializationProvider
{
    public byte[] Serialize<TModel>(TModel input)
    {
        if (input is byte[] arr)
            return arr;
        throw new NotSupportedException();
    }

    public TModel Deserialize<TModel>(byte[] input)
    {
        if (typeof(TModel) == typeof(byte[]))
            return (TModel)Convert.ChangeType(input, typeof(TModel));
        throw new NotSupportedException();
    }
}
