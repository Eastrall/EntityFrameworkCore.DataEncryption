using System;
using System.IO;
using System.Security;
using System.Text;

namespace Microsoft.EntityFrameworkCore.DataEncryption.Internal
{
    internal static class StandardConverters
    {
        internal static Stream BytesToStream(byte[] bytes) => new MemoryStream(bytes);

        internal static byte[] StreamToBytes(Stream stream)
        {
            if (stream is MemoryStream ms)
            {
                return ms.ToArray();
            }

            using var output = new MemoryStream();
            stream.CopyTo(output);
            return output.ToArray();
        }

        internal static string StreamToBase64String(Stream stream) => Convert.ToBase64String(StreamToBytes(stream));

        internal static string StreamToString(Stream stream)
        {
            using var reader = new StreamReader(stream, Encoding.UTF8);
            return reader.ReadToEnd().Trim('\0');
        }

        internal static SecureString StreamToSecureString(Stream stream)
        {
            using var reader = new StreamReader(stream, Encoding.UTF8);

            var result = new SecureString();
            var buffer = new char[100];
            while (!reader.EndOfStream)
            {
                var charsRead = reader.Read(buffer, 0, buffer.Length);
                if (charsRead != 0)
                {
                    for (int index = 0; index < charsRead; index++)
                    {
                        char c = buffer[index];
                        if (c != '\0')
                        {
                            result.AppendChar(c);
                        }
                    }
                }
            }

            return result;
        }
    }
}