using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.EntityFrameworkCore.DataEncryption.Test.Helpers
{
    public static class StringHelper
    {
        private static readonly string Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private static readonly Random Randomizer = new Random();

        /// <summary>
        /// Generates a new string.
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string RandomString(int length) 
            => new string(Enumerable.Repeat(Characters, length).Select(s => s[Randomizer.Next(s.Length)]).ToArray());
    }
}
