using BookWeb.Models;

namespace BookWeb.Repositories
{
    public interface IBookRepo
    {
        Task<IEnumerable<Book>> GetAllAsync();
        Task<Book> GetByIdAsync(int id);
        Task AddAsync(Book book);
        Task UpdateAsync(Book book);
        Task DeleteAsync(int id);

        //Top LIKE
        Task<IEnumerable<Book>> GetTopFavoritedBooksAsync(int count);
        //Top VIEW
        Task<IEnumerable<Book>> GetTopViewedBooksAsync(int count);

        // RATING
        // Lấy đánh giá trung bình theo BookId
        Task<double> GetAverageRatingByBookIdAsync(int bookId);
        // Cập nhật lại đánh giá trung bình cho sách
        Task UpdateAverageRatingAsync(int bookId);

        // QUANTITY
        int GetStockQuantity(int bookId);
        Task<int> GetTotalSoldQuantityAsync(int bookId);
        Task<List<Book>> GetTopSellingBooksAsync(int topCount);
    }
}
