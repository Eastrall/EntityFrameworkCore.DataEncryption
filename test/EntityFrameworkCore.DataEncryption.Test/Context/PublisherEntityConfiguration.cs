using Microsoft.EntityFrameworkCore.DataEncryption;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Encryption.Test.Context
{
    public sealed class PublisherEntityConfiguration : IEntityTypeConfiguration<PublisherEntity>
    {
        private readonly IEncryptionProvider _encryptionProvider;

        public PublisherEntityConfiguration(IEncryptionProvider encryptionProvider)
        {
            _encryptionProvider = encryptionProvider;
        }

        public void Configure(EntityTypeBuilder<PublisherEntity> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .IsEncrypted(_encryptionProvider);

            builder.Property(x => x.Address)
                .IsRequired()
                .IsEncrypted(_encryptionProvider);
        }
    }
}