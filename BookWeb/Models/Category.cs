using System.ComponentModel.DataAnnotations;

namespace BookWeb.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; }

        public string? Description { get; set; }

        public List<Book>? Books { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
