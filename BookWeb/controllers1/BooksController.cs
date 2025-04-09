using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookWeb.Data;
using BookWeb.Models;
using BookWeb.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BookWeb.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBookRepo _bookRepo;
        private readonly ICategoryRepo _categoryRepo;
        private readonly IFavoriteBookRepository _favoriteBookRepository;
        private readonly ICommentRepository _commentRepo;
        private readonly IRatingRepository _ratingRepository;

        public BooksController(ApplicationDbContext context, IBookRepo bookRepo, ICategoryRepo categoryRepo, IFavoriteBookRepository favoriteBookRepository, ICommentRepository commentRepo, IRatingRepository ratingRepository)
        {
            _context = context;
            _bookRepo = bookRepo;
            _categoryRepo = categoryRepo;
            _favoriteBookRepository = favoriteBookRepository;
            _commentRepo = commentRepo;
            _ratingRepository = ratingRepository;
        }

        //----------------------------------------------
        // GET: Books
        public async Task<IActionResult> Index(string query, int? categoryId, string sortOrder, int pageNumber = 1)
        {
           int pageSize = 12;
            // IQueryable<Book> productsQuery = _context.Books.Include(p => p.Category).Include(b => b.Author);
            // var books = await PaginatedList<Book>.CreateAsync(productsQuery, pageNumber, pageSize);
            //return View(books);

            IQueryable<Book> booksQuery = _context.Books.Include(b => b.Category);

            // Lọc theo tên nếu có query
            if (!string.IsNullOrEmpty(query))
            {
                booksQuery = booksQuery.Where(b => b.Name.Contains(query));
            }

            // Lọc theo thể loại nếu có categoryId
            if (categoryId.HasValue)
            {
                booksQuery = booksQuery.Where(b => b.CategoryId == categoryId);
            }

            // Sắp xếp theo lựa chọn
            switch (sortOrder)
            {
                case "price_asc":
                    booksQuery = booksQuery.OrderBy(b => b.Price); // Giá thấp đến cao
                    break;
                case "price_desc":
                    booksQuery = booksQuery.OrderByDescending(b => b.Price); // Giá cao đến thấp
                    break;
                case "date_asc":
                    booksQuery = booksQuery.OrderBy(b => b.CreatedDate); // Ngày cũ nhất trước
                    break;
                case "date_desc":
                default:
                    booksQuery = booksQuery.OrderByDescending(b => b.CreatedDate); // Ngày mới nhất trước
                    break;
            }

            // Phân trang
            var paginatedBooks = await PaginatedList<Book>.CreateAsync(booksQuery, pageNumber, pageSize);

            // Lấy tất cả thể loại để hiển thị trong dropdown
            var categories = await _categoryRepo.GetAllAsync();
            ViewBag.Categories = categories.Select(x => new SelectListItem()
            {
                Text = x.Name,
                Value = x.Id.ToString(),
                Selected = categoryId.HasValue && categoryId.Value == x.Id
            });

            // Truyền categoryId vào ViewData để giữ giá trị trong dropdown
            ViewData["CategoryId"] = categoryId;
            ViewData["SortOrder"] = sortOrder;
            ViewData["Query"] = query;

            return PartialView(paginatedBooks); // Hoặc View nếu bạn trả về toàn bộ trang
        }

        // DANH MỤC ---------------------------         
        public async Task<IActionResult> ViewPDF(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // PHÂN TRANG -------------------------------------------------------------
        public async Task<IActionResult> PagingNoLibrary(int pageNumber)
        {
            int pageSize = 5;
            IQueryable<Book> booksQuery = _context.Books.Include(p => p.Category);
            var pagedBooks = await booksQuery.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return View(pagedBooks);
        }

        // TÌM KIẾM -------------------------------------------------
        [HttpGet]
        public IActionResult SearchSuggestions(string query)
{
    var suggestions = _context.Books
        .Where(p => p.Name.StartsWith(query))
        .Select(p => new 
        {
            name= string.IsNullOrEmpty(p.Name) ? "Tên không xác định" : p.Name,
            ImageUrl = string.IsNullOrEmpty(p.ImageUrl) ? "/images/default.jpg" : p.ImageUrl,
            price= p.Price
        })
        .ToList();
    return Json(suggestions); // Trả về JSON cho AJAX


}


        // 28/11 - FILTER / SORT-------------------------------------------------------------
        public async Task<IActionResult> SearchProducts(string query, int? categoryId, string sortOrder, int pageNumber = 1)
        {
            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.Trim(); // Loại bỏ khoảng trắng ở đầu và cuối chuỗi
            }

            IQueryable<Book> booksQuery = _context.Books.Include(b => b.Category);

            // Lọc theo tên nếu có query
            if (!string.IsNullOrEmpty(query))
            {
                booksQuery = booksQuery.Where(b => b.Name.Contains(query));
            }

            // Lọc theo thể loại nếu có categoryId
            if (categoryId.HasValue)
            {
                booksQuery = booksQuery.Where(b => b.CategoryId == categoryId);
            }

            // Sắp xếp theo lựa chọn
            switch (sortOrder)
            {
                case "price_asc":
                    booksQuery = booksQuery.OrderBy(b => b.Price); // Giá thấp đến cao
                    break;
                case "price_desc":
                    booksQuery = booksQuery.OrderByDescending(b => b.Price); // Giá cao đến thấp
                    break;
                case "date_asc":
                    booksQuery = booksQuery.OrderBy(b => b.CreatedDate); // Ngày cũ nhất trước
                    break;
                case "date_desc":
                default:
                    booksQuery = booksQuery.OrderByDescending(b => b.CreatedDate); // Ngày mới nhất trước
                    break;
            }

            // Phân trang
            var paginatedBooks = await PaginatedList<Book>.CreateAsync(booksQuery, pageNumber, 12);

            // Lấy tất cả thể loại để hiển thị trong dropdown
            var categories = await _categoryRepo.GetAllAsync();
            ViewBag.Categories = categories.Select(x => new SelectListItem()
            {
                Text = x.Name,
                Value = x.Id.ToString(),
                Selected = categoryId.HasValue && categoryId.Value == x.Id
            });

            // Truyền categoryId vào ViewData để giữ giá trị trong dropdown
            ViewData["CategoryId"] = categoryId;
            ViewData["SortOrder"] = sortOrder;
            ViewData["Query"] = query;

            return PartialView(paginatedBooks); // Hoặc View nếu bạn trả về toàn bộ trang
        }


        //----------------------------------------------
        // GET: Books/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (id <= 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var book = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Include(b => b.Comments)
                    .ThenInclude(c => c.Replies) // Nạp tất cả phản hồi lồng nhau
                .Include(b => b.Comments)
                    .ThenInclude(c => c.User)
                .Include(b => b.Ratings)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            // Tính toán số lượt đánh giá
            int ratingCount = book.Ratings.Count();
            // Lấy điểm đánh giá trung bình
            var averageRating = await _bookRepo.GetAverageRatingByBookIdAsync(id);

            // Tạo view model với thông tin sách và điểm trung bình
            var viewModel = new BookDetailsViewModel
            {
                Book = book,
                AverageRating = averageRating,
                // Sắp xếp bình luận theo ngày tạo
                Comments = book.Comments.OrderBy(c => c.CreatedAt).ToList(),
            };

            // Tăng số lượt xem
            book.ViewCount++;
            await _bookRepo.UpdateAsync(book);

            // Kiểm tra sách có nằm trong danh sách yêu thích của người dùng hay không
            var isFavorite = userId != null && await _favoriteBookRepository.IsFavoriteAsync(userId, id);

            // Kiểm tra người dùng đã đánh giá sách này chưa
            var existingRating = userId != null ? await _ratingRepository.GetRatingByUserAndBookAsync(userId, id) : null;

            // Lấy số sao đã đánh giá
            int ratedStar = existingRating?.Star ?? 0;

            // Nếu TempData chứa giá trị sao, ưu tiên sử dụng
            if (TempData.ContainsKey("RatedStar"))
            {
                ratedStar = Convert.ToInt32(TempData["RatedStar"]);
            }

            // Kiểm tra trạng thái đã đánh giá
            var hasRated = ratedStar > 0;

            // Lấy thông tin dấu trang (bookmark)
            var bookmark = userId != null
                ? await _context.Bookmarks.FirstOrDefaultAsync(b => b.BookId == id && b.UserId == userId)
                : null;

            var bookmarkPage = bookmark?.PageNumber ?? 1;

            // Truyền dữ liệu vào ViewBag và ViewData
            ViewData["IsFavorite"] = isFavorite;
            ViewData["HasRated"] = hasRated;
            ViewData["BookmarkPage"] = bookmarkPage;
            ViewData["RatingCount"] = ratingCount;
            ViewBag.PdfPath = book.pdf; // Đường dẫn PDF của sách
            ViewBag.RatedStar = ratedStar;

            return View(book);
        }

        public class BookDetailsViewModel
        {
            public Book? Book { get; set; }
            public double AverageRating { get; set; }
            public List<Comment>? Comments { get; set; }
        }


        // RATING -------------------------------------------------------------------
        // Thêm đánh giá
        [HttpPost]
        [Authorize]
       public async Task<IActionResult> AddRating(int bookId, int star)
{
    if (star < 1 || star > 5)
    {
        return BadRequest("Rating must be between 1 and 5 stars.");
    }

    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Lấy thông tin người dùng
    if (userId == null)
    {
        return RedirectToAction("Login", "Account");
    }

    // Kiểm tra xem người dùng đã đánh giá sách này chưa
    var existingRating = await _ratingRepository.GetExistingRatingAsync(userId, bookId);
    if (existingRating != null)
    {
        // Nếu đã đánh giá, cập nhật sao
        existingRating.Star = star;
        existingRating.CreatedAt = DateTime.UtcNow;
        await _ratingRepository.UpdateRatingAsync(existingRating);
    }
    else
    {
        // Nếu chưa đánh giá, thêm mới
        var rating = new Rating
        {
            BookId = bookId,
            UserId = userId,
            Star = star,
            CreatedAt = DateTime.UtcNow
        };

        await _ratingRepository.AddRatingAsync(rating);
    }

    // Cập nhật điểm đánh giá trung bình
    await _bookRepo.UpdateAverageRatingAsync(bookId);

    // Cập nhật ViewBag.RatedStar cho thông báo
    TempData["RatedStar"] = star;

    // Chuyển hướng về trang chi tiết
    return RedirectToAction("Details", "Books", new { id = bookId });
}

    }
}
