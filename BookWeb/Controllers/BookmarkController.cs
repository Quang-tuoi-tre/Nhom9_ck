using BookWeb.Data;
using BookWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookWeb.Controllers
{
    public class BookmarkController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookmarkController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> SetBookmark([FromBody] BookmarkDto bookmarkDto)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized(); // Nếu người dùng chưa đăng nhập, trả về lỗi Unauthorized
            }

            // Kiểm tra nếu đã có dấu sách cho sách này và trang cụ thể này
            var existingBookmark = await _context.Bookmarks
                .FirstOrDefaultAsync(b => b.UserId == userId && b.BookId == bookmarkDto.BookId && b.PageNumber == bookmarkDto.PageNumber);

            if (existingBookmark == null)
            {
                // Nếu chưa có dấu sách cho trang này, tạo mới
                var bookmark = new Bookmark
                {
                    UserId = userId,
                    BookId = bookmarkDto.BookId,
                    PageNumber = bookmarkDto.PageNumber,
                    Note = bookmarkDto.Note, // Lưu ghi chú
                    CreatedAt = DateTime.Now
                };
                _context.Bookmarks.Add(bookmark);
            }
            else
            {
                // Nếu đã có dấu sách cho trang này, có thể cập nhật ghi chú hoặc số trang
                existingBookmark.Note = bookmarkDto.Note;

                _context.Bookmarks.Update(existingBookmark);
            }

            await _context.SaveChangesAsync();

            // Trả về một thông báo thành công
            return Json(new { success = true, message = "Dấu trang đã được lưu thành công!" });
        }

        [HttpGet]
        public async Task<IActionResult> GetBookmarks(int bookId)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized(); // Nếu người dùng chưa đăng nhập
            }

            // Lấy tất cả dấu trang của người dùng cho cuốn sách
            var bookmarks = await _context.Bookmarks
                .Where(b => b.UserId == userId && b.BookId == bookId)
                .Select(b => new { b.PageNumber, b.Note})
                .ToListAsync();

            return Json(new { success = true, bookmarks = bookmarks }); // Trả về danh sách dấu trang
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBookmark(int bookId, int pageNumber)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Json(new { success = false, message = "Người dùng chưa đăng nhập." });
            }

            // Tìm dấu trang dựa trên userId, bookId và pageNumber
            var bookmark = await _context.Bookmarks
                .FirstOrDefaultAsync(b => b.UserId == userId && b.BookId == bookId && b.PageNumber == pageNumber);

            if (bookmark == null)
            {
                return Json(new { success = false, message = "Dấu trang không tồn tại." });
            }

            // Xóa dấu trang
            _context.Bookmarks.Remove(bookmark);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Dấu trang đã được xóa thành công!" });
        }

        public async Task<IActionResult> BooksWithBookmarks(int pageNumber = 1, int pageSize = 8)
        {
            // Lấy userId từ người dùng đã đăng nhập
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Tính toán số bản ghi cần bỏ qua (skip)
            var skip = (pageNumber - 1) * pageSize;

            // Lấy danh sách sách có đánh dấu cho người dùng hiện tại
            var booksWithBookmarks = await _context.Bookmarks
                .Where(b => b.UserId == userId)
                .Include(b => b.Book)
                    .ThenInclude(b => b.Bookmarks)
                .Select(b => b.Book)
                .Distinct()
                .Skip(skip) // Bỏ qua số lượng sách đã xem
                .Take(pageSize) // Lấy số lượng sách theo pageSize
                .ToListAsync();

            // Kiểm tra nếu không có sách nào có đánh dấu
            if (booksWithBookmarks == null || !booksWithBookmarks.Any())
            {
                return View(new List<Book>()); // Trả về view với danh sách sách rỗng
            }

            // Tính toán tổng số sách có đánh dấu để phân trang
            var totalBooks = await _context.Bookmarks
                .Where(b => b.UserId == userId)
                .Select(b => b.Book)
                .Distinct()
                .CountAsync();

            // Tính số trang tổng cộng
            var totalPages = (int)Math.Ceiling(totalBooks / (double)pageSize);

            // Thêm thông tin phân trang vào ViewBag để sử dụng trong view
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;

            return View(booksWithBookmarks); // Trả về view với danh sách sách có đánh dấu
        }
    }
}
