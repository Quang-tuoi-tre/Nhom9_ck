namespace BookWeb.Models
{
    public class BookmarkDto
    {
        public int BookId { get; set; }
        public int PageNumber { get; set; }

        public string? Note { get; set; } // Thêm trường ghi chú
    }
}
