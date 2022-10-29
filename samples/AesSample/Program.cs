using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DataEncryption.Providers;
using System;
using System.Linq;
using System.Security;

namespace AesSample;

static class Program
{
    static void Main()
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: "MyInMemoryDatabase")
            .Options;

        // AES key randomly generated at each run.
        AesKeyInfo keyInfo = AesProvider.GenerateKey(AesKeySize.AES256Bits);
        byte[] encryptionKey = keyInfo.Key;
        byte[] encryptionIV = keyInfo.IV;
        var encryptionProvider = new AesProvider(encryptionKey, encryptionIV);

        using var context = new DatabaseContext(options, encryptionProvider);

        var user = new UserEntity
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@doe.com",
            EncryptedData = new byte[2] { 1, 2 },
            EncryptedDataAsString = new byte[2] { 3, 4 }
        };

        context.Users.Add(user);
        context.SaveChanges();

        Console.WriteLine($"Users count: {context.Users.Count()}");

        user = context.Users.First();

        Console.WriteLine($"User: {user.FirstName} {user.LastName} - {user.Email}");
    }
}
