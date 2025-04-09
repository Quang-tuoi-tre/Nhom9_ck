using BookWeb.Models;

namespace BookWeb.Repositories
{
    public interface ICommentRepository
    {
        Task AddCommentAsync(Comment comment);
        Task<IEnumerable<Comment>> GetCommentsByBookIdAsync(int bookId);
        Task<IEnumerable<Comment>> GetAllAsync();
        Task<Comment> GetByIdAsync(int id);
        Task UpdateAsync(Comment comment);
        Task DeleteAsync(int id);
        // 25/11 - Phản hồi comment
        Task AddReplyAsync(int parentCommentId, string userId, string content);
    }
}
