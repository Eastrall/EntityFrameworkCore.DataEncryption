using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DataEncryption.Providers;
using System;
using System.Linq;
using System.Security;

namespace AesSample
{
	static class Program
	{
		static void Main()
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
				Email = "john@doe.com",
				Password = BuildPassword(),
			};

			context.Users.Add(user);
			context.SaveChanges();

			Console.WriteLine($"Users count: {context.Users.Count()}");

			user = context.Users.First();

			Console.WriteLine($"User: {user.FirstName} {user.LastName} - {user.Email} ({user.Password.Length})");
		}

		static SecureString BuildPassword()
		{
			SecureString result = new();
			result.AppendChar('L');
			result.AppendChar('e');
			result.AppendChar('t');
			result.AppendChar('M');
			result.AppendChar('e');
			result.AppendChar('I');
			result.AppendChar('n');
			result.AppendChar('!');
			result.MakeReadOnly();
			return result;
		}
	}
}
