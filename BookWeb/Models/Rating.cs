using System.ComponentModel.DataAnnotations;

namespace BookWeb.Models
{
    public class Rating
    {
        public int Id { get; set; }

        public int BookId { get; set; }
        public Book Book { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Range(1, 5, ErrorMessage = "Đánh giá phải trong khoảng từ 1 đến 5")]
        public int Star { get; set; }  // Số sao (1 - 5)
        public DateTime CreatedAt { get; set; }
    }
}
