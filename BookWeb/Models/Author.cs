using System.ComponentModel.DataAnnotations;

namespace BookWeb.Models
{
    public class Author
    {
        public int Id { get; set; }

        public string? ImageUrl { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        public string? Description { get; set; }

        public List<Book>? Books { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
