
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookWeb.Data;
using BookWeb.Models;
using Microsoft.AspNetCore.Authorization;

namespace BookWeb.Areas.admin.Controllers
{
    [Area("admin")]
    [Authorize(Roles = "admin")]
    public class CategoriesManagerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesManagerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: admin/CategoriesManager
        public async Task<IActionResult> Index(int pageNumber = 1, string query = "")
        {
            int pageSize = 10;

            // Khởi tạo truy vấn
            IQueryable<Category> categoriesQuery = _context.Categories
                .OrderByDescending(b => b.CreatedDate);

            // Lọc theo chuỗi tìm kiếm nếu có
            if (!string.IsNullOrEmpty(query))
            {
                categoriesQuery = categoriesQuery.Where(c => c.Name.Contains(query));
            }

            // Phân trang
            var paginatedCategories = await PaginatedList<Category>.CreateAsync(categoriesQuery, pageNumber, pageSize);

            // Truyền query vào ViewBag để giữ giá trị tìm kiếm trong View
            ViewBag.Query = query;

            return View(paginatedCategories);
        }

        // GET: admin/CategoriesManager/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: admin/CategoriesManager/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,CreatedDate")] Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: admin/CategoriesManager/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: admin/CategoriesManager/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,CreatedDate")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
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

            return View(category);
        }

        // GET: admin/CategoriesManager/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: admin/CategoriesManager/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
