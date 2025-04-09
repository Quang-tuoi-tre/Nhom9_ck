using BookWeb.Data;
using BookWeb.Models;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace BookWeb.Repositories
{
    public class EFCommentRepository : ICommentRepository
    {
        private readonly ApplicationDbContext _context;

        public EFCommentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddCommentAsync(Comment comment)
        {
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Comment>> GetCommentsByBookIdAsync(int bookId)
        {
            return await _context.Comments
                .Where(c => c.BookId == bookId && c.ParentId == null)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.Replies)
                    .ThenInclude(r => r.User)
                .Include(c => c.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<Comment>> GetAllAsync()
        {
            return await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.User)
                .Include(c => c.Book)
                .ToListAsync();
        }

        public async Task<Comment> GetByIdAsync(int id)
        {
            return await _context.Comments.FindAsync(id);
        }

        public async Task UpdateAsync(Comment comment)
        {
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var comment = await _context.Comments
                .Include(c => c.Replies)
                    .ThenInclude(r => r.Replies) // Bao gồm phản hồi cấp 2
                .FirstOrDefaultAsync(c => c.Id == id);
            if (comment != null)
            {
                // Xóa tất cả phản hồi cấp 2 (sub-replies)
                foreach (var reply in comment.Replies)
                {
                    _context.Comments.RemoveRange(reply.Replies); // Xóa các phản hồi cấp 2
                }

                if (comment.Replies != null && comment.Replies.Any())
                {
                    // Xóa các bình luận con
                    _context.Comments.RemoveRange(comment.Replies);
                }
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
            }
        }

        // 25/11 - Phản hồi comment
        public async Task AddReplyAsync(int parentCommentId, string userId, string content)
        {
            var parentComment = await _context.Comments.FindAsync(parentCommentId);
            if (parentComment == null) throw new Exception("Parent comment not found.");
             
            var reply = new Comment
            {
                BookId = parentComment.BookId,
                ParentId = parentCommentId,
                UserId = userId,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(reply);
            await _context.SaveChangesAsync();
        }
    }
}
