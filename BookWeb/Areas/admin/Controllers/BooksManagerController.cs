
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookWeb.Data;
using BookWeb.Models;
using BookWeb.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace BookWeb.Areas.admin.Controllers
{
    [Area("admin")]
    [Authorize(Roles = "admin")]
    public class BooksManagerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBookRepo _bookRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IRatingRepository _ratingRepository;

        public BooksManagerController(ApplicationDbContext context, IBookRepo bookRepository, ICommentRepository commentRepository, IRatingRepository ratingRepository)
        {
            _context = context;
            _bookRepository = bookRepository;
            _commentRepository = commentRepository;
            _ratingRepository = ratingRepository;
        }

        // GET: admin/BooksManager
        public async Task<IActionResult> Index(string sortColumn = "CreatedDate", string sortOrder = "desc", int pageNumber = 1, string query = "")
        {
            int pageSize = 10;

            // Khởi tạo truy vấn dữ liệu
            IQueryable<Book> booksQuery = _context.Books
                .Include(p => p.Category)
                .Include(b => b.Author);

            // Áp dụng bộ lọc nếu có chuỗi tìm kiếm
            if (!string.IsNullOrEmpty(query))
            {
                booksQuery = booksQuery.Where(b => b.Name.Contains(query) || b.Author.Name.Contains(query));
            }

            // Áp dụng sắp xếp theo cột và thứ tự
            switch (sortColumn.ToLower())
            {
                case "name":
                    booksQuery = sortOrder == "asc" ? booksQuery.OrderBy(b => b.Name) : booksQuery.OrderByDescending(b => b.Name);
                    break;
                case "rating":
                    booksQuery = sortOrder == "asc" ? booksQuery.OrderBy(b => b.AverageRating) : booksQuery.OrderByDescending(b => b.AverageRating);
                    break;
                case "comments":
                    booksQuery = sortOrder == "asc" ? booksQuery.OrderBy(b => b.Comments.Count) : booksQuery.OrderByDescending(b => b.Comments.Count);
                    break;
                case "views":
                    booksQuery = sortOrder == "asc" ? booksQuery.OrderBy(b => b.ViewCount) : booksQuery.OrderByDescending(b => b.ViewCount);
                    break;
                case "likes":
                    booksQuery = sortOrder == "asc" ? booksQuery.OrderBy(b => b.FavoriteCount) : booksQuery.OrderByDescending(b => b.FavoriteCount);
                    break;
                case "sold":
                    booksQuery = sortOrder == "asc" ? booksQuery.OrderBy(b => b.SoldQuantity) : booksQuery.OrderByDescending(b => b.SoldQuantity);
                    break;
                case "price":
                    booksQuery = sortOrder == "asc" ? booksQuery.OrderBy(b => b.Price) : booksQuery.OrderByDescending(b => b.Price);
                    break;
                case "stock":
                    booksQuery = sortOrder == "asc" ? booksQuery.OrderBy(b => b.StockQuantity) : booksQuery.OrderByDescending(b => b.StockQuantity);
                    break;
                default:
                    // Mặc định sắp xếp theo CreatedDate giảm dần
                    booksQuery = booksQuery.OrderByDescending(b => b.CreatedDate);
                    break;
            }

            // Phân trang
            var paginatedBooks = await PaginatedList<Book>.CreateAsync(booksQuery, pageNumber, pageSize);

            // Lấy số lượng sách đã bán cho mỗi sách
            var soldQuantities = new Dictionary<int, int>();

            foreach (var book in paginatedBooks)
            {
                soldQuantities[book.Id] = await _bookRepository.GetTotalSoldQuantityAsync(book.Id);
            }

            ViewBag.SoldQuantities = soldQuantities;

            // Truyền query vào ViewBag để giữ giá trị tìm kiếm trong View
            ViewBag.Query = query;
            // Truyền các tham số sắp xếp vào ViewBag để hiển thị trong view
            ViewBag.SortColumn = sortColumn;
            ViewBag.SortOrder = sortOrder;

            return View(paginatedBooks);
        }


        // GET: admin/BooksManager/Details/5
        public async Task<IActionResult> Details(int? id)
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

        [Area("admin")]
        // GET: admin/BooksManager/Create
        public async Task<IActionResult> Create()
        {
            ViewData["AuthorId"] = new SelectList(_context.Authors, "Id", "Name");
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: admin/BooksManager/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Area("admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(long.MaxValue)]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        public async Task<IActionResult> Create([Bind("Id,ImageUrl,Name,CategoryId,AuthorId,Description,Price,pdf,StockQuantity,AudioFileUrl,CreatedDate")] Book book, IFormFile imageUrl, IFormFile pdf, IFormFile audioFileUrl)
        {
            if (ModelState.IsValid)
            {
                if (imageUrl != null)
                {
                    book.ImageUrl = await SaveImage(imageUrl);
                }

                if (pdf != null)
                {
                    book.pdf = await SavePDF(pdf);
                }

                if (audioFileUrl != null)
                {
                    book.AudioFileUrl = await SaveAudio(audioFileUrl);
                }

                book.StockQuantity = 0;

                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["AuthorId"] = new SelectList(_context.Authors, "Id", "Name", book.AuthorId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoryId);
            return View(book);
        }


        // GET: admin/BooksManager/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            ViewData["AuthorId"] = new SelectList(_context.Authors, "Id", "Name", book.AuthorId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoryId);
            return View(book);
        }

        // POST: admin/BooksManager/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(long.MaxValue)]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,CategoryId,AuthorId,Description,Price,StockQuantity")] Book book, IFormFile imageUrl, IFormFile pdf, IFormFile audioFileUrl)
        {
            ModelState.Remove("ImageUrl");
            ModelState.Remove("pdf");
            ModelState.Remove("AudioFileUrl");

            if (id != book.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingBook = await _bookRepository.GetByIdAsync(id);

                    if (imageUrl != null && imageUrl.Length > 0)
                        existingBook.ImageUrl = await SaveImage(imageUrl);

                    if (pdf != null && pdf.Length > 0)
                        existingBook.pdf = await SavePDF(pdf);

                    if (audioFileUrl != null && audioFileUrl.Length > 0)
                        existingBook.AudioFileUrl = await SaveAudio(audioFileUrl);

                    existingBook.Name = book.Name;
                    existingBook.CategoryId = book.CategoryId;
                    existingBook.AuthorId = book.AuthorId;
                    existingBook.Description = book.Description;
                    existingBook.Price = book.Price;
                    existingBook.StockQuantity = book.StockQuantity;

                    await _bookRepository.UpdateAsync(existingBook);
                    TempData["SuccessMessage"] = "Thông tin sách đã được cập nhật.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Id)) return NotFound();
                    throw;
                }
            }

            ViewData["AuthorId"] = new SelectList(_context.Authors, "Id", "Name", book.AuthorId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoryId);
            return View(book);
        }

        // GET: admin/BooksManager/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

        // POST: admin/BooksManager/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }

        // HÌNH ẢNH / TÀI LIỆU / ÂM THANH--------------------------------------------------
        private async Task<string> SaveImage(IFormFile image)
        {
            var savePath = Path.Combine("wwwroot/images", image.FileName);
            // Thay đổi đường dẫn theo cấu hình của bạn
            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return "" + image.FileName; // Trả về đường dẫn tương đối
        }

        private async Task<string> SavePDF(IFormFile pdf)
        {
            var savePath = Path.Combine("wwwroot/pdf", pdf.FileName);
            // Thay đổi đường dẫn theo cấu hình của bạn
            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await pdf.CopyToAsync(fileStream);
            }
            return "/pdf/" + pdf.FileName; // Trả về đường dẫn tương đối
        }

        private async Task<string> SaveAudio(IFormFile audio)
        {
            var savePath = Path.Combine("wwwroot/audio", audio.FileName);
            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await audio.CopyToAsync(fileStream);
            }
            return "/audio/" + audio.FileName; // Trả về đường dẫn tương đối
        }

        // XEM CHI TIẾT ĐÁNH GIÁ /BÌNH LUẬN -------------------------
        public IActionResult Ratings(int id)
        {
            var book = _context.Books
                .Include(b => b.Ratings)
                .FirstOrDefault(b => b.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // Xóa đánh giá
        [HttpPost]
        public async Task<IActionResult> DeleteRating(int ratingId, int bookId)
        {
            // Lấy đánh giá cần xóa
            var rating = await _ratingRepository.GetByIdAsync(ratingId);
            if (rating == null)
            {
                return NotFound();
            }

            // Xóa đánh giá
            await _ratingRepository.DeleteRatingAsync(rating);

            // Cập nhật điểm đánh giá trung bình
            await _bookRepository.UpdateAverageRatingAsync(bookId);

            // Thông báo thành công
            ModelState.AddModelError(string.Empty, "Đánh giá đã được xóa!");

            return Redirect(Request.Headers["Referer"].ToString());
            //return RedirectToAction("Details", "Books", new { id = bookId });
        }

        // BÌNH LUẬN
        public IActionResult Comments(int id)
        {
            var book = _context.Books
                .Include(b => b.Comments)
                    .ThenInclude(c => c.Replies) // Nếu có replies
                .FirstOrDefault(b => b.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // Xóa một bình luận
        public async Task<IActionResult> DeleteComment(int id, int bookId)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            await _commentRepository.DeleteAsync(id);
            TempData["Success"] = "Comment has been deleted.";

            return Redirect(Request.Headers["Referer"].ToString());
            //return RedirectToAction("Details", "Books", new { id = bookId });
        }

        // CẬP NHẬT SỐ LƯỢNG -------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int bookId, int stockQuantity)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                return NotFound();
            }

            // Kiểm tra số lượng tồn kho hợp lệ
            if (stockQuantity < 0)
            {
                ModelState.AddModelError("", "Số lượng không thể nhỏ hơn 0.");
                return RedirectToAction(nameof(Index)); // Quay lại trang
            }

            try
            {
                // Cập nhật số lượng sách
                book.StockQuantity = stockQuantity;

                // Lưu thay đổi vào cơ sở dữ liệu
                _context.Update(book);
                await _context.SaveChangesAsync();

                // Thêm thông báo thành công vào TempData (hoặc ViewBag nếu muốn hiển thị trong View)
                TempData["SuccessMessage"] = "Số lượng sách đã được cập nhật thành công!";
            }
            catch (Exception ex)
            {
                // Log lỗi hoặc xử lý các tình huống lỗi khác
                ModelState.AddModelError("", "Đã có lỗi xảy ra khi cập nhật số lượng. Vui lòng thử lại.");
                return RedirectToAction(nameof(Index)); // Quay lại trang nếu có lỗi
            }

            return RedirectToAction(nameof(Index)); // Quay lại trang danh sách sách
        }

        // THÊM SỐ LƯỢNG
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuantity(int bookId, int addQuantity)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                return NotFound();
            }

            // Kiểm tra số lượng hợp lệ
            if (addQuantity <= 0)
            {
                TempData["ErrorMessage"] = "Số lượng thêm phải lớn hơn 0.";
                return RedirectToAction(nameof(Index));
            }

            // Thêm số lượng sách tồn
            book.StockQuantity = (book.StockQuantity ?? 0) + addQuantity;

            // Lưu thay đổi vào cơ sở dữ liệu
            _context.Update(book);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã thêm {addQuantity} sách vào kho.";
            return RedirectToAction(nameof(Index));
        }
    }
}
