using BookWeb.Models;

namespace BookWeb.Repositories
{
    public interface IFavoriteBookRepository
    {
        Task<bool> IsFavoriteAsync(string userId, int bookId);
        Task AddToFavoritesAsync(string userId, int bookId);
        Task RemoveFromFavoritesAsync(string userId, int bookId);
        Task<bool> IsBookFavoritedAsync(string userId, int bookId);
        Task<IEnumerable<Book>> GetFavoriteBooksAsync(string userId);
        Task<int> GetFavoriteCountAsync(int bookId);

        // Phân trang
        Task<IEnumerable<Book>> GetFavoriteBooksAsync(string userId, int pageNumber, int pageSize);
        Task<int> GetFavoriteBooksCountAsync(string userId);
    }
}
