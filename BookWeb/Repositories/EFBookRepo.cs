using BookWeb.Data;
using BookWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace BookWeb.Repositories
{
    public class EFBookRepo : IBookRepo
    {
        private readonly ApplicationDbContext _context;

        public EFBookRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Book>> GetAllAsync()
        {
            return await _context.Books.Include(p => p.Category).ToListAsync();
        }

        public async Task<Book> GetByIdAsync(int id)
        {
            return await _context.Books.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddAsync(Book book)
        {
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Book book)
        {
            _context.Books.Update(book);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
        }

        //Top LIKE
        public async Task<IEnumerable<Book>> GetTopFavoritedBooksAsync(int count)
        {
            return await _context.Books
                .OrderByDescending(b => b.FavoriteCount)
                .Take(count)
                .ToListAsync();
        }

        //Top VIEW
        public async Task<IEnumerable<Book>> GetTopViewedBooksAsync(int count)
        {
            return await _context.Books
                .OrderByDescending(b => b.ViewCount)
                .Take(count)
                .ToListAsync();
        }

        // RATING
        public async Task<double> GetAverageRatingByBookIdAsync(int bookId)
        {
            var ratings = await _context.Ratings
                .Where(r => r.BookId == bookId)
                .ToListAsync();

            return ratings.Any() ? ratings.Average(r => r.Star) : 0;
        }

        public async Task UpdateAverageRatingAsync(int bookId)
        {            
            // Lấy tất cả đánh giá của sách
            var ratings = await _context.Ratings
                .Where(r => r.BookId == bookId)
                .ToListAsync();

            if (ratings.Any())
            {
                // Tính lại trung bình sao
                double averageRating = ratings.Average(r => r.Star);

                // Cập nhật giá trị trung bình vào bảng sách
                var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookId);
                if (book != null)
                {
                    book.AverageRating = averageRating;
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                // Nếu không có đánh giá, đặt trung bình là 0
                var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookId);
                if (book != null)
                {
                    book.AverageRating = 0;
                    await _context.SaveChangesAsync();
                }
            }
        }

        // QUANTITY
        public int GetStockQuantity(int bookId)
        {
            var book = _context.Books.FirstOrDefault(b => b.Id == bookId);
            return book?.StockQuantity ?? 0;
        }

        public async Task<int> GetTotalSoldQuantityAsync(int bookId)
        {
            // Giả sử bạn có bảng OrderDetails lưu trữ chi tiết đơn hàng
            var soldQuantity = await _context.OrderDetails
                                              .Where(od => od.BookId == bookId && (od.Order.Status == "Đang giao hàng"
                                                                               || od.Order.Status == "Chờ xác nhận"))
                                              .SumAsync(od => od.Quantity);

            return soldQuantity;
        }

        // Thêm phương thức trong BookRepo để lấy các sách bán chạy nhất
        public async Task<List<Book>> GetTopSellingBooksAsync(int topCount)
        {
            return await _context.Books
                .OrderByDescending(b => b.SoldQuantity)  // Giả sử SoldQuantity là thuộc tính chứa số lượng bán được
                .Take(topCount)
                .ToListAsync();
        }
    }
}
