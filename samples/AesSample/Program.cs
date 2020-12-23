using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DataEncryption.Providers;
using System;
using System.Linq;

namespace AesSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: "MyInMemoryDatabase")
                .Options;

            // AES key randomly generated at each run.
            byte[] encryptionKey = AesProvider.GenerateKey(AesKeySize.AES256Bits).Key;
            var encryptionProvider = new AesProvider(encryptionKey);

            using var context = new DatabaseContext(options, encryptionProvider);

            var user = new UserEntity
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@doe.com"
            };

            context.Users.Add(user);
            context.SaveChanges();

            Console.WriteLine($"Users count: {context.Users.Count()}");

            user = context.Users.FirstOrDefault();

            Console.WriteLine($"User: {user.FirstName} {user.LastName} - {user.Email}");
        }
    }
}
