using System;
using System.Security;

namespace Microsoft.EntityFrameworkCore.DataEncryption.Test.Helpers
{
    public static class DataHelper
    {
        private static readonly string Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private static readonly Random Randomizer = new();

        public static byte[] RandomBytes(int length)
        {
            var result = new byte[length];
            Randomizer.NextBytes(result);
            return result;
        }

        public static string RandomString(int length)
        {
            var result = new char[length];
            for (int i = 0; i < length; i++)
            {
                char c = Characters[Randomizer.Next(Characters.Length)];
                result[i] = c;
            }

            return new(result);
        }

        public static SecureString RandomSecureString(int length)
        {
            var result = new SecureString();
            for (int i = 0; i < length; i++)
            {
                char c = Characters[Randomizer.Next(Characters.Length)];
                result.AppendChar(c);
            }

            return result;
        }
    }
}
