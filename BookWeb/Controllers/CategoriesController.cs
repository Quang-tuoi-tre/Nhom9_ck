
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookWeb.Data;
using BookWeb.Models;
using BookWeb.Repositories;

namespace BookWeb.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBookRepo _bookRepo;

        public CategoriesController(ApplicationDbContext context, IBookRepo bookRepo)
        {
            _context = context;
            _bookRepo = bookRepo;
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }

        public async Task<IActionResult> ForeignLanguageTopic(int pageNumber = 1)
        {
            int pageSize = 12;
            IQueryable<Book> productsQuery = _context.Books.Include(p => p.Category).Include(b => b.Author).Where(p => p.CategoryId == 8);
            var paginatedProducts = await PaginatedList<Book>.CreateAsync(productsQuery, pageNumber, pageSize);
            return View(paginatedProducts);
        }

        public async Task<IActionResult> TechnologyTopic(int pageNumber = 1)
        {
            int pageSize = 12;
            IQueryable<Book> productsQuery = _context.Books.Include(p => p.Category).Include(b => b.Author).Where(p => p.CategoryId == 6);
            var paginatedProducts = await PaginatedList<Book>.CreateAsync(productsQuery, pageNumber, pageSize);
            return View(paginatedProducts);
        }

        public async Task<IActionResult> MarketingTopic(int pageNumber = 1)
        {
            int pageSize = 12;
            IQueryable<Book> productsQuery = _context.Books.Include(p => p.Category).Include(b => b.Author).Where(p => p.CategoryId == 4);
            var paginatedProducts = await PaginatedList<Book>.CreateAsync(productsQuery, pageNumber, pageSize);
            return View(paginatedProducts);
        }

        public async Task<IActionResult> FoodTopic(int pageNumber = 1)
        {
            int pageSize = 12;
            IQueryable<Book> productsQuery = _context.Books.Include(p => p.Category).Include(b => b.Author).Where(p => p.CategoryId == 10);
            var paginatedProducts = await PaginatedList<Book>.CreateAsync(productsQuery, pageNumber, pageSize);
            return View(paginatedProducts);
        }

        public async Task<IActionResult> FreeBook(int pageNumber = 1)
        {
            int pageSize = 12;

            // Truy vấn chỉ lấy sách có giá = 0 (miễn phí)
            IQueryable<Book> freeBooksQuery = _context.Books
                .Where(b => b.Price == 0)
                .Include(b => b.Category)
                .Include(b => b.Author);

            // Phân trang danh sách sách miễn phí
            var paginatedFreeBooks = await PaginatedList<Book>.CreateAsync(freeBooksQuery, pageNumber, pageSize);

            return View(paginatedFreeBooks);
        }

        public async Task<IActionResult> TalkBook(int pageNumber = 1)
        {
            int pageSize = 12;
            // Lọc sách có giá = 0 và có AudioFileUrl
            IQueryable<Book> productsQuery = _context.Books
                .Where(b => !string.IsNullOrEmpty(b.AudioFileUrl)) // Lọc sách miễn phí và có AudioFileUrl
                .Include(p => p.Category)
                .Include(b => b.Author);

            var paginatedProducts = await PaginatedList<Book>.CreateAsync(productsQuery, pageNumber, pageSize);
            return View(paginatedProducts);
        }

        // TOP BOOK ---------------------------
        public async Task<IActionResult> TopFavorites(int currentGroup = 1)
        {
            var books = await _bookRepo.GetTopFavoritedBooksAsync(10); // Get top 10 books from repo
            var groupSize = 4; // Number of books per group
            int totalBooks = books.Count(); // Ensure books.Count is an int

            // Calculate the total number of groups
            var totalGroups = (int)Math.Ceiling((double)totalBooks / groupSize);

            // Calculate the start and end index for the current group
            var startIndex = (currentGroup - 1) * groupSize;
            var endIndex = Math.Min(startIndex + groupSize, totalBooks);

            var groupBooks = books.Skip(startIndex).Take(groupSize).ToList();

            // Passing startIndex to the view
            ViewBag.StartIndex = startIndex;

            // Passing totalGroups to the view
            ViewBag.TotalGroups = totalGroups;
            ViewBag.CurrentGroup = currentGroup;

            return View(groupBooks);
        }

        public async Task<IActionResult> TopViewed(int currentGroup = 1)
        {
            var books = await _bookRepo.GetTopViewedBooksAsync(10); // Fetch top 10 viewed books
            var groupSize = 4; // Number of books per group
            int totalBooks = books.Count(); // Ensure books.Count is an int

            // Calculate the total number of groups
            var totalGroups = (int)Math.Ceiling((double)totalBooks / groupSize);

            // Calculate the start and end index for the current group
            var startIndex = (currentGroup - 1) * groupSize;
            var endIndex = Math.Min(startIndex + groupSize, totalBooks);

            var groupBooks = books.Skip(startIndex).Take(groupSize).ToList();

            // Passing startIndex to the view
            ViewBag.StartIndex = startIndex;

            // Passing totalGroups to the view
            ViewBag.TotalGroups = totalGroups;
            ViewBag.CurrentGroup = currentGroup;

            return View(groupBooks);
        }
    }
}
