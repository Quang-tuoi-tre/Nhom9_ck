using System.ComponentModel.DataAnnotations;

namespace BookWeb.Models
{
    public class Bookmark
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BookId { get; set; }
        public Book Book { get; set; }

        // Liên kết với bảng User nếu dùng Identity
        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        
        // Trang được đánh dấu
        public int PageNumber { get; set; }
        public string? Note { get; set; } // Ghi chú cho dấu trang
        public DateTime CreatedAt { get; set; }
    }
}
