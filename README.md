# EntityFrameworkCore.DataEncryption

[![.NET](https://github.com/Eastrall/EntityFrameworkCore.DataEncryption/actions/workflows/build.yml/badge.svg)](https://github.com/Eastrall/EntityFrameworkCore.DataEncryption/actions/workflows/build.yml)
[![codecov](https://codecov.io/gh/Eastrall/EntityFrameworkCore.DataEncryption/branch/master/graph/badge.svg)](https://codecov.io/gh/Eastrall/EntityFrameworkCore.DataEncryption)
[![Nuget](https://img.shields.io/nuget/v/EntityFrameworkCore.DataEncryption.svg)](https://www.nuget.org/packages/EntityFrameworkCore.DataEncryption)
[![Nuget Downloads](https://img.shields.io/nuget/dt/EntityFrameworkCore.DataEncryption)](https://www.nuget.org/packages/EntityFrameworkCore.DataEncryption)

`EntityFrameworkCore.DataEncryption` is a [Microsoft Entity Framework Core](https://github.com/aspnet/EntityFrameworkCore) extension to add support of encrypted fields using built-in or custom encryption providers.

## Disclaimer

This library has been developed initialy for a personal project of mine which suits my use case. It provides a simple way to encrypt column data.

I **do not** take responsability if you use/deploy this in a production environment and loose your encryption key or corrupt your data.

## How to install

Install the package from [NuGet](https://www.nuget.org/) or from the `Package Manager Console` :
```powershell
PM> Install-Package EntityFrameworkCore.DataEncryption
```

## Supported types

| Type | Default storage type |
|------|----------------------|
| `string` | Base64 string |
| `byte[]` | BINARY |

## Built-in providers

| Name | Class | Extra |
|------|-------|-------|
| [AES](https://learn.microsoft.com/en-US/dotnet/api/system.security.cryptography.aes?view=net-6.0) | [AesProvider](https://github.com/Eastrall/EntityFrameworkCore.DataEncryption/blob/main/src/EntityFrameworkCore.DataEncryption/Providers/AesProvider.cs) | Can use a 128bits, 192bits or 256bits key |

## How to use

`EntityFrameworkCore.DataEncryption` supports 2 differents initialization methods:
* Attribute
* Fluent configuration

Depending on the initialization method you will use, you will need to decorate your `string` or `byte[]` properties of your entities with the `[Encrypted]` attribute or use the fluent `IsEncrypted()` method in your model configuration process.
To use an encryption provider on your EF Core model, and enable the encryption on the `ModelBuilder`. 

### Example with `AesProvider` and attribute

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
		_provider = new AesProvider(this._encryptionKey, this._encryptionIV);
	}
	
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.UseEncryption(_provider);
	}
}
```
The code bellow creates a new [`AesProvider`](https://github.com/Eastrall/EntityFrameworkCore.DataEncryption/blob/main/src/EntityFrameworkCore.DataEncryption/Providers/AesProvider.cs) and gives it to the current model. It will encrypt every `string` fields of your model that has the `[Encrypted]` attribute when saving changes to database. As for the decrypt process, it will be done when reading the `DbSet<T>` of your `DbContext`.

### Example with `AesProvider` and fluent configuration

```csharp
public class UserEntity
{
	public int Id { get; set; }
	public string Username { get; set; }
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
		_provider = new AesProvider(this._encryptionKey, this._encryptionIV);
	}
	
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		// Entities builder *MUST* be called before UseEncryption().
		var userEntityBuilder = modelBuilder.Entity<UserEntity>();
		
		userEntityBuilder.Property(x => x.Username).IsRequired().IsEncrypted();
		userEntityBuilder.Property(x => x.Password).IsRequired().IsEncrypted();

		modelBuilder.UseEncryption(_provider);
	}
}
```

## Create an encryption provider

`EntityFrameworkCore.DataEncryption` gives the possibility to create your own encryption providers. To do so, create a new class and make it inherit from `IEncryptionProvider`. You will need to implement the `Encrypt(string)` and `Decrypt(string)` methods.

```csharp
public class MyCustomEncryptionProvider : IEncryptionProvider
{
	public byte[] Encrypt(byte[] input)
	{
		// Encrypt the given input and return the encrypted data as a byte[].
	}
	
	public byte[] Decrypt(byte[] input)
	{
		// Decrypt the given input and return the decrypted data as a byte[].
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
		_provider = new MyCustomEncryptionProvider();
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.UseEncryption(_provider);
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