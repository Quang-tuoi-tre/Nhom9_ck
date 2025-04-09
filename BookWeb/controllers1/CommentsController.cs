
using BookWeb.Models;
using BookWeb.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookWeb.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ICommentRepository _commentRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommentsController(ICommentRepository commentRepository, UserManager<ApplicationUser> userManager)
        {
            _commentRepository = commentRepository;
            _userManager = userManager;
        }

        [HttpPost("Add")]
        [Authorize]
        public async Task<IActionResult> AddComment(int bookId, string content)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Bình luận không được để trống.";
                return RedirectToAction("Details", "Books", new { id = bookId });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "Người dùng không hợp lệ.";
                return RedirectToAction("Details", "Books", new { id = bookId });
            }

            var comment = new Comment
            {
                BookId = bookId,
                UserId = userId,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            comment.User = await _userManager.FindByIdAsync(userId);

            await _commentRepository.AddCommentAsync(comment);

            return RedirectToAction("Details", "Books", new { id = bookId });
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

        // Phản hồi comment
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddReply(int parentId, string content)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Kiểm tra nội dung phản hồi không rỗng
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Phản hồi không được để trống.";
                return Redirect(Request.Headers["Referer"].ToString());
                //return RedirectToAction("Details", "Books", new { id = parentId });
            }

            // Lấy thông tin bình luận cha
            var parentComment = await _commentRepository.GetByIdAsync(parentId);
            if (parentComment == null)
            {
                TempData["Error"] = "Bình luận không tồn tại.";
                return Redirect(Request.Headers["Referer"].ToString());
            }

            // Tạo đối tượng phản hồi mới
            var reply = new Comment
            {
                BookId = parentComment.BookId,
                UserId = userId,
                Content = content,
                CreatedAt = DateTime.UtcNow,
                ParentId = parentId // Liên kết với bình luận cha
            };

            // Thêm phản hồi vào cơ sở dữ liệu
            await _commentRepository.AddCommentAsync(reply);
            TempData["Success"] = "Phản hồi đã được thêm.";

            // Truyền lại thông tin bình luận và phản hồi mới
            var comments = await _commentRepository.GetCommentsByBookIdAsync(parentComment.BookId);

            //return Redirect(Request.Headers["Referer"].ToString());
            return RedirectToAction("Details", "Books", new { id = parentComment.BookId });
        }

    }
}
