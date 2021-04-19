# EntityFrameworkCore.DataEncryption

[![Build Status](https://dev.azure.com/eastrall/EntityFrameworkCore.DataEncryption/_apis/build/status/EntityFrameworkCore.DataEncryption?branchName=master)](https://dev.azure.com/eastrall/EntityFrameworkCore.DataEncryption/_build/latest?definitionId=9&branchName=master)
[![codecov](https://codecov.io/gh/Eastrall/EntityFrameworkCore.DataEncryption/branch/master/graph/badge.svg)](https://codecov.io/gh/Eastrall/EntityFrameworkCore.DataEncryption)
[![Nuget](https://img.shields.io/nuget/v/EntityFrameworkCore.DataEncryption.svg)](https://www.nuget.org/packages/EntityFrameworkCore.DataEncryption)

`EntityFrameworkCore.DataEncryption` is a [Microsoft Entity Framework Core](https://github.com/aspnet/EntityFrameworkCore) extension to add support of encrypted fields using built-in or custom encryption providers.

## Disclaimer

This library has been developed initialy for a personal project of mine. It provides a simple way to encrypt column data.

I **do not** take responsability if you use this in a production environment and loose your encryption key.

## How to install

Install the package from [NuGet](https://www.nuget.org/) or from the `Package Manager Console` :
```powershell
PM> Install-Package EntityFrameworkCore.DataEncryption
```

## How to use

To use `EntityFrameworkCore.DataEncryption`, you will need to decorate your `string` properties of your entities with the `[Encrypted]` attribute and enable the encryption on the `ModelBuilder`. 

To enable the encryption correctly, you will need to use an **encryption provider**, there is a list of the available providers:

| Name | Class | Extra |
|------|-------|-------|
| [AES](https://docs.microsoft.com/en-US/dotnet/api/system.security.cryptography.aes?view=netcore-2.2) | [AesProvider](https://github.com/Eastrall/EntityFrameworkCore.DataEncryption/blob/master/src/EntityFrameworkCore.DataEncryption/Providers/AesProvider.cs) | Can use a 128bits, 192bits or 256bits key |

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

> :warning: This section is outdated and doesn't work for V3.0.0 and will be updated soon.

`EntityFrameworkCore.DataEncryption` gives the possibility to create your own encryption providers. To do so, create a new class and make it inherit from `IEncryptionProvider`. You will need to implement the `Encrypt(string)` and `Decrypt(string)` methods.

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

## Important notes

### AES Provider structure

The following section describes how encrypted fields using the built-in AES provider encrypts data.
There is two available modes :

* Fixed IV
* Dynamic IV

#### Fixed IV

A fixed IV is generated at setup and is used for every encrypted fields on the database.
This might be a security issue depending on your context.

#### Dynamic IV

For each encrypted field, the provider generates a new IV with a length of `16 bytes`. These 16 bytes are written at the begining of the `CryptoStream` followed by the actual input to encrypt.

Similarly, for reading, the provider reads the first **16 bytes** from the input data converted as a `byte[]` to retrieve the initialization vector and then read the encrypted content.

For more information, checkout the [`AesProvider`](https://github.com/Eastrall/EntityFrameworkCore.DataEncryption/blob/master/src/EntityFrameworkCore.DataEncryption/Providers/AesProvider.cs#L58) class.

> :warning: When using Dynamic IV, you cannot use the Entity Framework LINQ extensions because the provider will generate a new IV per value, which will create unexpected behaviors.

## Thanks

I would like to thank all the people that supports and contributes to the project and helped to improve the library. :smile:

## Credits

Package Icon : from [Icons8](https://icons8.com/)