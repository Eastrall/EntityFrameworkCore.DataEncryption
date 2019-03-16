using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.DataEncryption.Test.Context
{
    public sealed class AuthorEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Encrypted]
        public string FirstName { get; set; }
        
        [Required]
        [Encrypted]
        public string LastName { get; set; }

        [Required]
        public int Age { get; set; }

        public IList<BookEntity> Books { get; set; }

        public AuthorEntity(string firstName, string lastName, int age)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Age = age;
            this.Books = new List<BookEntity>();
        }
    }
}
