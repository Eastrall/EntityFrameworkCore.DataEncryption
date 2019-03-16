using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.DataEncryption.Test.Context
{
    public sealed class BookEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Encrypted]
        public string Name { get; set; }

        [Required]
        public int NumberOfPages { get; set; }

        [Required]
        public int AuthorId { get; set; }

        [ForeignKey(nameof(AuthorId))]
        public AuthorEntity Author { get; set; }

        public BookEntity(string name, int numberOfPages)
        {
            this.Name = name;
            this.NumberOfPages = numberOfPages;
        }
    }
}
