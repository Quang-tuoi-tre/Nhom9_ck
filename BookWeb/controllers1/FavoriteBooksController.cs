using BookWeb.Data;
using BookWeb.Models;
using BookWeb.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;
using System.Security.Claims;

namespace BookWeb.Controllers
{
    [Authorize]
    public class FavoriteBooksController : Controller
    {
        private readonly IBookRepo _bookRepo;
        private readonly IFavoriteBookRepository _repository;
        private readonly UserManager<ApplicationUser> _userManager;

        public FavoriteBooksController(IFavoriteBookRepository repository, UserManager<ApplicationUser> userManager, IBookRepo bookRepo)
        {
            _repository = repository;
            _userManager = userManager;
            _bookRepo = bookRepo;
        }

        public async Task<IActionResult> AddToFavorites(int bookId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!await _repository.IsFavoriteAsync(userId, bookId))
            {
                await _repository.AddToFavoritesAsync(userId, bookId);

                // Cập nhật số lượt thích
                var book = await _bookRepo.GetByIdAsync(bookId); // Use _bookRepo to fetch the book
                if (book != null)
                {
                    book.FavoriteCount++;
                    await _bookRepo.UpdateAsync(book);
                }
            }

            return RedirectToAction("Details", "Books", new { id = bookId });
        }

        public async Task<IActionResult> RemoveFromFavorites(int bookId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (await _repository.IsFavoriteAsync(userId, bookId))
            {
                await _repository.RemoveFromFavoritesAsync(userId, bookId);
                // Cập nhật số lượt thích
                var book = await _bookRepo.GetByIdAsync(bookId);
                if (book != null)
                {
                    book.FavoriteCount--;
                    await _bookRepo.UpdateAsync(book);
                }
            }

            return RedirectToAction("Details", "Books", new { id = bookId });
        }

        public async Task<IActionResult> RemoveFromMyFavorites(int bookId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (await _repository.IsFavoriteAsync(userId, bookId))
            {
                await _repository.RemoveFromFavoritesAsync(userId, bookId);
                // Cập nhật số lượt thích
                var book = await _bookRepo.GetByIdAsync(bookId);
                if (book != null)
                {
                    book.FavoriteCount--;
                    await _bookRepo.UpdateAsync(book);
                }
            }

            return RedirectToAction("MyFavorites", "FavoriteBooks");
        }

        public async Task<IActionResult> MyFavorites(int pageNumber = 1, int pageSize = 8)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // Lấy danh sách yêu thích đã phân trang
            var favorites = await _repository.GetFavoriteBooksAsync(user.Id, pageNumber, pageSize);

            // Lấy tổng số lượng yêu thích để tính tổng số trang
            var totalFavorites = await _repository.GetFavoriteBooksCountAsync(user.Id);
            var totalPages = (int)Math.Ceiling(totalFavorites / (double)pageSize);

            // Lưu thông tin phân trang vào ViewBag
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;

            ViewBag.AvatarUrl = string.IsNullOrEmpty(user.AvatarUrl) ? Url.Content("~/images/taixuong1.jpg") : user.AvatarUrl;

            return View(favorites);
        }
    }
}
