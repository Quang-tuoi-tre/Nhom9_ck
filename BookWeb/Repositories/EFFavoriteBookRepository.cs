using BookWeb.Data;
using BookWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace BookWeb.Repositories
{
    public class EFFavoriteBookRepository : IFavoriteBookRepository
    {
        private readonly ApplicationDbContext _context;

        public EFFavoriteBookRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsFavoriteAsync(string userId, int bookId)
        {
            return await _context.FavoriteBooks.AnyAsync(f => f.UserId == userId && f.BookId == bookId);
        }

        public async Task AddToFavoritesAsync(string userId, int bookId)
        {
            var favorite = new FavoriteBooks { UserId = userId, BookId = bookId };
            _context.FavoriteBooks.Add(favorite);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveFromFavoritesAsync(string userId, int bookId)
        {
            var favorite = await _context.FavoriteBooks.FirstOrDefaultAsync(f => f.UserId == userId && f.BookId == bookId);
            if (favorite != null)
            {
                _context.FavoriteBooks.Remove(favorite);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsBookFavoritedAsync(string userId, int bookId)
        {
            return await _context.FavoriteBooks.AnyAsync(f => f.UserId == userId && f.BookId == bookId);
        }

        public async Task<IEnumerable<Book>> GetFavoriteBooksAsync(string userId)
        {
            return await _context.FavoriteBooks
                .Where(f => f.UserId == userId)
                .Select(f => f.Book)
                .ToListAsync();
        }

        public async Task<int> GetFavoriteCountAsync(int bookId)
        {
            return await _context.FavoriteBooks.CountAsync(f => f.BookId == bookId);
        }

        // Phân trang
        public async Task<IEnumerable<Book>> GetFavoriteBooksAsync(string userId, int pageNumber, int pageSize)
        {
            return await _context.FavoriteBooks
                .Where(fb => fb.UserId == userId)
                .OrderBy(fb => fb.Book.Name)  // Hoặc bất kỳ trường nào để sắp xếp
                .Skip((pageNumber - 1) * pageSize)  // Bỏ qua các kết quả của các trang trước
                .Take(pageSize)  // Lấy số lượng bản ghi theo trang
                .Include(fb => fb.Book)  // Lấy thông tin Book từ bảng FavoriteBooks
                .Select(fb => fb.Book)  // Chỉ lấy danh sách sách
                .ToListAsync();
        }

        public async Task<int> GetFavoriteBooksCountAsync(string userId)
        {
            return await _context.FavoriteBooks
                .Where(fb => fb.UserId == userId)
                .CountAsync();
        }
    }
}
