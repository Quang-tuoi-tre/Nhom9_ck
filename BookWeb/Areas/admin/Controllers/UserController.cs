
using BookWeb.Data;
using BookWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookWeb.Areas.admin.Controllers
{
    [Area("admin")]
    [Authorize(Roles = "admin")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(string query = "", int pageNumber = 1)
        {
            int pageSize = 10;

            // Truy vấn danh sách người dùng
            IQueryable<ApplicationUser> usersQuery = _userManager.Users;

            // Lọc theo từ khóa tìm kiếm nếu có
            if (!string.IsNullOrEmpty(query))
            {
                usersQuery = usersQuery.Where(u =>
                    u.UserName.Contains(query) ||
                    u.Email.Contains(query) ||
                    u.PhoneNumber.Contains(query));
            }

            // Phân trang
            var paginatedUsers = await PaginatedList<ApplicationUser>.CreateAsync(usersQuery, pageNumber, pageSize);

            // Lấy tất cả các role từ RoleManager
            var allRoles = await _roleManager.Roles.ToListAsync();
            // Truyền danh sách role vào ViewBag
            ViewBag.AllRoles = allRoles;
                        // Truyền query để giữ giá trị tìm kiếm trong View
            ViewBag.Query = query;

            return View(paginatedUsers);
        }

        //----------------------PHÂN QUYỀN--------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(string userId, string role)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
            {
                return BadRequest("Thông tin không hợp lệ.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("Người dùng không tồn tại.");
            }

            var roleExist = await _roleManager.RoleExistsAsync(role);
            if (!roleExist)
            {
                return NotFound("Quyền không tồn tại.");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                return BadRequest("Xóa quyền cũ không thành công.");
            }

            var addResult = await _userManager.AddToRoleAsync(user, role);
            if (addResult.Succeeded)
            {
                return RedirectToAction("Index"); // Reload lại trang để hiển thị quyền đã thay đổi
            }

            foreach (var error in addResult.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View("Index");
        }

        //----------------------Lock/Unlock User--------------------
        [HttpGet]
        public async Task<IActionResult> LockUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("User ID is required.");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Set LockoutEnd to a future date, which effectively locks the user
            var result = await _userManager.SetLockoutEndDateAsync(user, DateTime.UtcNow.AddYears(100)); // Lock user for a long time
            if (result.Succeeded)
            {
                return RedirectToAction("Index"); // Hoặc trang quản lý users
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> UnlockUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("User ID is required.");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Set LockoutEnd to null, which unlocks the user
            var result = await _userManager.SetLockoutEndDateAsync(user, null);
            if (result.Succeeded)
            {
                return RedirectToAction("Index"); // Quay lại trang quản lý users
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return RedirectToAction("Index");
        }

        //----------------------Role--------------------
        public async Task<IActionResult> Roles()
        {
            var roles = _roleManager.Roles.ToList();
            return View(roles);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
            {
                ModelState.AddModelError("", "Tên quyền không được để trống.");
                return View();
            }

            var role = new IdentityRole(roleName);
            var result = await _roleManager.CreateAsync(role);

            if (result.Succeeded)
            {
                return RedirectToAction("Roles"); // Reload lại trang hiện tại để thấy quyền mới
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRole(string roleName, string newRoleName)
        {
            if (string.IsNullOrEmpty(newRoleName))
            {
                ModelState.AddModelError("", "Tên quyền không được để trống.");
                return View();
            }

            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                return NotFound("Quyền không tồn tại.");
            }

            role.Name = newRoleName;
            var result = await _roleManager.UpdateAsync(role);

            if (result.Succeeded)
            {
                return RedirectToAction("Roles"); // Reload lại trang hiện tại để thấy quyền đã thay đổi
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View();
        }

        // Xóa role
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
            {
                return BadRequest("Role name is required.");
            }

            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                return NotFound("Role not found.");
            }

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                // Nếu xóa thành công, quay lại trang danh sách
                return RedirectToAction("Roles");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            // Nếu có lỗi, quay lại trang danh sách và hiển thị thông báo lỗi
            return RedirectToAction("Roles");
        }
    }
}
