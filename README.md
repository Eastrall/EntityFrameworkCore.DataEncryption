# EntityFrameworkCore.Encryption

[![Build Status](https://travis-ci.org/Eastrall/EntityFrameworkCore.Encryption.svg?branch=master)](https://travis-ci.org/Eastrall/EntityFrameworkCore.Encryption)
[![Codacy Badge](https://api.codacy.com/project/badge/Grade/74fc74231f7542848fdc221014de2109)](https://www.codacy.com/app/Eastrall/EntityFrameworkCore.Encryption?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=Eastrall/EntityFrameworkCore.Encryption&amp;utm_campaign=Badge_Grade)
[![codecov](https://codecov.io/gh/Eastrall/EntityFrameworkCore.Encryption/branch/master/graph/badge.svg)](https://codecov.io/gh/Eastrall/EntityFrameworkCore.Encryption)
![Nuget](https://img.shields.io/badge/nuget-soon-blue.svg)

`EntityFrameworkCore.Encryption` is a [Microsoft Entity Framework Core](https://github.com/aspnet/EntityFrameworkCore) extension to add support of encrypted fields using built-in or custom encryption providers.

## How to install

![Nuget](https://img.shields.io/badge/nuget-soon-blue.svg)

Install the package from [NuGet](https://www.nuget.org/) or from the `Package Manager Console` :
```powershell
PM> Install-Package EntityFrameworkCore.Extensions
```

## How to use

To use `EntityFrameworkCore.Encryption`, you will need to decorate your `string` properties of your entities with the `[Encrypted]` attribute and enable the encryption on the `ModelBuilder`. 

To enable the encryption correctly, you will need to use an **encryption provider**, there is a list of the available providers:

| Name | Class | Extra |
|------|-------|-------|
| [AES](https://docs.microsoft.com/en-US/dotnet/api/system.security.cryptography.aes?view=netcore-2.2) | [AesProvider](https://github.com/Eastrall/EntityFrameworkCore.Encryption/blob/master/src/EntityFrameworkCore.Encryption/Providers/AesProvider.cs) | Can use a 128bits, 192bits or 256bits key |

### Example with `AesProvider`

```csharp
public class UserEntity
{
	public int Id { get; set; }
	
	[Encrypted]
	public string Username { get; set; }
	
	[Encrypted]
	public string Password { get; set; }
	
	public int Age { get; set; }
}

public class DatabaseContext : DbContext
{
	// Get key and IV from a Base64String or any other ways.
	// You can generate a key and IV using "AesProvider.GenerateKey()"
	private readonly byte[] _encryptionKey = ...; 
	private readonly byte[] _encryptionIV = ...;
	private readonly IEncryptionProvider _provider;

	public DbSet<UserEntity> Users { get; set; }
	
	public DatabaseContext(DbContextOptions options)
		: base(options)
	{
		this._provider = new AesProvider(this._encryptionKey, this._encryptionIV);
	}
	
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.UseEncryption(this._provider);
	}
}
```
The code bellow creates a new `AesEncryption` provider and gives it to the current model. It will encrypt every `string` fields of your model that has the `[Encrypted]` attribute when saving changes to database. As for the decrypt process, it will be done when reading the `DbSet<T>` of your `DbContext`.

## Create an encryption provider

`EntityFrameworkCore.Encryption` gives the possibility to create your own encryption providers. To do so, create a new class and make it inherit from `IEncryptionProvider`. You will need to implement the `Encrypt(string)` and `Decrypt(string)` methods.

```csharp
public class MyCustomEncryptionProvider : IEncryptionProvider
{
	public string Encrypt(string dataToEncrypt)
	{
		// Encrypt data and return as Base64 string
	}
	
	public string Decrypt(string dataToDecrypt)
	{
		// Decrypt a Base64 string to plain string
	}
}
```

To use it, simply create a new `MyCustomEncryptionProvider` in your `DbContext` and pass it to the `UseEncryption` method:
```csharp
public class DatabaseContext : DbContext
{
	private readonly IEncryptionProvider _provider;

	public DatabaseContext(DbContextOptions options)
		: base(options)
	{
		this._provider = new MyCustomEncryptionProvider();
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.UseEncryption(this._provider);
	}
}
```
