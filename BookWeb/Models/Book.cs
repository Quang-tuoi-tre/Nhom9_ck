using System.ComponentModel.DataAnnotations;

namespace BookWeb.Models
{
    public class Book
    {
        public int Id { get; set; }

        public string? ImageUrl { get; set; }

        [Required, StringLength(100)]
        public string? Name { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public int AuthorId { get; set; }
        public Author? Author { get; set; }

        public string? Description { get; set; }
        [Range(0, 999)]
        public int? StockQuantity { get; set; } = 0; // Số lượng tồn kho
        public int SoldQuantity { get; set; }
        public int TotalQuantity => (StockQuantity ?? 0) + SoldQuantity;

        [Range(0, 9999999)]
        public decimal Price { get; set; }
        public string? pdf { get; set; }
        public string? AudioFileUrl { get; set; } // URL hoặc đường dẫn file audio
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Số lượt thích
        public int FavoriteCount { get; set; }
        // New property for view count
        public int ViewCount { get; set; }

        // 27/11 - COMMENT
        public ICollection<Comment>? Comments { get; set; }
        // Thuộc tính mới để lưu tổng số comment
        public int CommentCount => Comments?.Count() ?? 0;

        // RATING
        // Thuộc tính để lưu trung bình đánh giá
        public double AverageRating { get; set; }
        // Danh sách đánh giá liên kết
        public ICollection<Rating>? Ratings { get; set; } = new List<Rating>();

        // 29/11 - NOTE
        public ICollection<Bookmark>? Bookmarks { get; set; } // Mối quan hệ giữa sách và dấu trang
    }
}
