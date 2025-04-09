namespace BookWeb.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }

        // 25/11 - Trường để lưu phản hồi
        public int? ParentId { get; set; }
        public Comment Parent { get; set; }
        public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();

        // Relationships
        public int BookId { get; set; }
        public Book Book { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}