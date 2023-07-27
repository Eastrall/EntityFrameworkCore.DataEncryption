using System;

namespace Microsoft.EntityFrameworkCore.DataEncryption.Internal;

internal sealed class Base64SerializationProvider : ISerializationProvider
{
    public byte[] Serialize<TModel>(TModel input)
    {
        if (input is string str)
            return Convert.FromBase64String(str);
        throw new NotSupportedException();
    }

    public TModel Deserialize<TModel>(byte[] input)
    {
        if (typeof(TModel) == typeof(string))
            return (TModel)Convert.ChangeType(input, typeof(TModel));
        throw new NotSupportedException();
    }
}
