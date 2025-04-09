using BookWeb.Data;
using BookWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace BookWeb.Repositories
{
    public class EFRatingRepository : IRatingRepository
    {
        private readonly ApplicationDbContext _context;

        public EFRatingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddRatingAsync(Rating rating)
        {
            _context.Ratings.Add(rating);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Rating>> GetRatingsByBookIdAsync(int bookId)
        {
            return await _context.Ratings
                .Where(r => r.BookId == bookId)
                .Include(r => r.User)  // Eager load User to display username/fullname
                .ToListAsync();
        }

        public async Task<Rating> GetRatingByUserAndBookAsync(string? userId, int bookId)
        {
            return await _context.Ratings.FirstOrDefaultAsync(r => r.UserId == userId && r.BookId == bookId);
        }

        // 27/11 - Quản lý đánh giá
        // Lấy tất cả các đánh giá
        public async Task<IEnumerable<Rating>> GetAllRatingsAsync()
        {
            return await _context.Ratings.Include(r => r.Book).ToListAsync();
        }

        // Lấy đánh giá theo ID
        public async Task<Rating> GetByIdAsync(int ratingId)
        {
            return await _context.Ratings.FirstOrDefaultAsync(r => r.Id == ratingId);
        }

        // Xóa đánh giá
        public async Task DeleteRatingAsync(Rating rating)
        {
            _context.Ratings.Remove(rating);
            await _context.SaveChangesAsync();
        }       
        
        // Get the average rating for a specific book
        public async Task<double?> GetAverageRatingAsync(int bookId)
        {
            return await _context.Ratings
                .Where(r => r.BookId == bookId)
                .AverageAsync(r => (double?)r.Star);  // Return average of star ratings, or null if no ratings
        }

        // Get the existing rating for a user and a specific book
        public async Task<Rating?> GetExistingRatingAsync(string userId, int bookId)
        {
            return await _context.Ratings
                .FirstOrDefaultAsync(r => r.UserId == userId && r.BookId == bookId);
        }
        public async Task UpdateRatingAsync(Rating rating)
        {
            _context.Ratings.Update(rating); // Cập nhật đối tượng
            await _context.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu
        }
    }
}
