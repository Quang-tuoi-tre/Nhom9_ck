
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookWeb.Data;
using BookWeb.Models;
using Microsoft.AspNetCore.Authorization;

namespace BookWeb.Areas.admin.Controllers
{
    [Area("admin")]
    [Authorize(Roles = "admin")]
    public class AuthorsManagerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthorsManagerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: admin/AuthorsManager
        public async Task<IActionResult> Index(string query = "", int pageNumber = 1)
        {
            int pageSize = 10;

            // Truy vấn danh sách tác giả
            IQueryable<Author> authorsQuery = _context.Authors
                .OrderByDescending(a => a.CreatedDate);

            // Lọc theo từ khóa tìm kiếm nếu có
            if (!string.IsNullOrEmpty(query))
            {
                authorsQuery = authorsQuery.Where(a => a.Name.Contains(query));
            }

            // Phân trang
            var paginatedAuthors = await PaginatedList<Author>.CreateAsync(authorsQuery, pageNumber, pageSize);

            // Truyền query để giữ giá trị tìm kiếm trong View
            ViewBag.Query = query;

            return View(paginatedAuthors);
        }

        [Authorize(Roles = "admin")]
        // GET: admin/AuthorsManager/Create
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [Authorize(Roles = "admin")]
        // POST: admin/AuthorsManager/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ImageUrl,Name,Description,CreatedDate")] Author author, IFormFile imageUrl)
        {
            if (ModelState.IsValid)
            {
                author.ImageUrl = await SaveImage(imageUrl);

                _context.Add(author);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(author);
        }

        [Authorize(Roles = "admin")]
        // GET: admin/AuthorsManager/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var author = await _context.Authors.FindAsync(id);
            if (author == null)
            {
                return NotFound();
            }

            return View(author);
        }

        [Authorize(Roles = "admin")]
        // POST: admin/AuthorsManager/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ImageUrl,Name,Description,CreatedDate")] Author author, IFormFile imageUrl)
        {
            if (id != author.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    author.ImageUrl = await SaveImage(imageUrl);

                    _context.Update(author);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AuthorExists(author.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            return View(author);
        }

        [Authorize(Roles = "admin")]
        // GET: admin/AuthorsManager/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var author = await _context.Authors
                .FirstOrDefaultAsync(m => m.Id == id);
            if (author == null)
            {
                return NotFound();
            }

            return View(author);
        }

        [Authorize(Roles = "admin")]
        // POST: admin/AuthorsManager/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var author = await _context.Authors.FindAsync(id);
            if (author != null)
            {
                _context.Authors.Remove(author);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AuthorExists(int id)
        {
            return _context.Authors.Any(e => e.Id == id);
        }

        // HÌNH ẢNH --------------------------------------------------
        private async Task<string> SaveImage(IFormFile image)
        {
            var savePath = Path.Combine("wwwroot/images", image.FileName);
            // Thay đổi đường dẫn theo cấu hình của bạn
            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return "/images/" + image.FileName; // Trả về đường dẫn tương đối
        }
    }
}
