using Microsoft.EntityFrameworkCore.DataEncryption.Test.Context;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Encryption.Test.Context
{
    public sealed class PublisherEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }

        public IList<AuthorEntity> Authors { get; set; }

        public PublisherEntity(string name, string address)
        {
            this.Name = name;
            this.Address = address;

            this.Authors = new List<AuthorEntity>();
        }
    }
}