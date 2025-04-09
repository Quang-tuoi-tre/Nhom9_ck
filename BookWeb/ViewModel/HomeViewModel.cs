using BookWeb.Models;
namespace BookWeb.ViewModel
{
    public class HomeViewModel
    {
        public IEnumerable<Category> Categories { get; set; }
        public IEnumerable<Book> Books { get; set; }

        // Top Favorited Books
        public IEnumerable<Book> TopBooksFavorites { get; set; }
        public int CurrentGroupFavorites { get; set; }
        public int TotalGroupsFavorites { get; set; }
        public int StartIndexFavorites { get; set; }

        // Top Viewed Books
        public IEnumerable<Book> TopBooksViewed { get; set; }
        public int CurrentGroupViewed { get; set; }
        public int TotalGroupsViewed { get; set; }
        public int StartIndexViewed { get; set; }

        // Thêm thuộc tính sách bán chạy
        public IEnumerable<Book> TopBooksSelling { get; set; }  
        public int TotalGroupsSelling { get; set; }
        public int CurrentGroupSelling { get; set; }
        public int StartIndexSelling { get; set; }
    }
}
