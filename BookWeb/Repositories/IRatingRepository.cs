using BookWeb.Models;

namespace BookWeb.Repositories
{
    public interface IRatingRepository
    {
        Task AddRatingAsync(Rating rating);
        // Lấy tất cả các đánh giá của một sách theo ID
        Task<IEnumerable<Rating>> GetRatingsByBookIdAsync(int bookId);
        // Lấy đánh giá của một người dùng đối với một cuốn sách
        Task<Rating> GetRatingByUserAndBookAsync(string userId, int bookId);

        // 27/11 - Quản lý đánh giá
        // Lấy tất cả các đánh giá
        Task<IEnumerable<Rating>> GetAllRatingsAsync();

        // Lấy một đánh giá theo ID
        Task<Rating> GetByIdAsync(int ratingId);

        // Xóa một đánh giá
        Task DeleteRatingAsync(Rating rating);

        // LẤY ĐÁNH GIÁ TRUNG BÌNH
        Task<double?> GetAverageRatingAsync(int bookId);
        Task<Rating?> GetExistingRatingAsync(string userId, int bookId);

        Task UpdateRatingAsync(Rating rating); 

    }
}
