using System;
using System.Text;
using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.DataEncryption.Internal;

[Obsolete("Use only for old projects that used an old version of this library")]
internal sealed class GenericSerializationProvider : ISerializationProvider
{
    public byte[] Serialize<TModel>(TModel input)
    {
        byte[] inputData = input switch
        {
            string => !string.IsNullOrEmpty(input.ToString()) ? Encoding.UTF8.GetBytes(input.ToString()) : null,
            byte[] => input as byte[],
            _ => JsonSerializer.SerializeToUtf8Bytes(input),
        };
        return inputData;
    }

    public TModel Deserialize<TModel>(byte[] input)
    {
        if (typeof(TModel) == typeof(string))
        {
            string decryptedData = Encoding.UTF8.GetString(input).Trim('\0');
            return (TModel)Convert.ChangeType(decryptedData, typeof(TModel));
        }
        else if (typeof(TModel) == typeof(byte[]))
        {
            return (TModel)Convert.ChangeType(input, typeof(TModel));
        }

        return JsonSerializer.Deserialize<TModel>(input);
    }
}
