using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookWeb.Data;
using Microsoft.AspNetCore.Authorization;

namespace BookWeb.Areas.admin.Controllers
{
    [Area("admin")]
    [Authorize(Roles = "admin")]
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ManagerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy số lượng người dùng
            var userCount = await _context.Users.CountAsync();

            // Lấy số lượng sách
            var bookCount = await _context.Books.CountAsync();

            // Lấy số lượng đơn hàng
            var orderCount = await _context.Orders.CountAsync();

            // Tính tổng doanh thu
            decimal totalRevenue = await _context.Orders
                .Where(o => o.Status == "Giao hàng thành công")
                .SumAsync(o => (decimal?)o.TotalPrice) ?? 0;

            // Truyền dữ liệu qua ViewData hoặc ViewBag
            ViewData["UserCount"] = userCount;
            ViewData["BookCount"] = bookCount;
            ViewData["OrderCount"] = orderCount;
            ViewData["TotalRevenue"] = totalRevenue;

            // BIỂU ĐỒ 2
            var totalOrders = _context.Orders.Count();
            var pendingOrders = _context.Orders.Count(o => o.Status == "Chờ xác nhận");
            var inProgressOrders = _context.Orders.Count(o => o.Status == "Đang giao hàng");
            var completedOrders = _context.Orders.Count(o => o.Status == "Giao hàng thành công");
            var canceledOrders = _context.Orders.Count(o => o.Status == "Đã huỷ");

            ViewBag.TotalOrders = totalOrders;
            ViewBag.PendingPercentage = totalOrders > 0 ? (pendingOrders * 100) / totalOrders : 0;
            ViewBag.InProgressPercentage = totalOrders > 0 ? (inProgressOrders * 100) / totalOrders : 0;
            ViewBag.CompletedPercentage = totalOrders > 0 ? (completedOrders * 100) / totalOrders : 0;
            ViewBag.CanceledPercentage = totalOrders > 0 ? (canceledOrders * 100) / totalOrders : 0;

            // Lấy danh sách đơn hàng để hiển thị
            var orders = await _context.Orders.ToListAsync();

            // Truyền danh sách đơn hàng qua model
            return View(orders);
        }
    }
}
