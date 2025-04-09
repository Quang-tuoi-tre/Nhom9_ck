
using BookWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace BookWeb.Areas.admin.Controllers
{
    [Area("admin")]
    [Authorize(Roles = "admin")]
    public class OrderManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> ManageOrders(int page = 1)
        {
            const int pageSize = 10; // Số lượng đơn hàng mỗi trang
            // Lấy tổng số đơn hàng
            var totalOrders = await _context.Orders.CountAsync();

            // Phân trang
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Truyền dữ liệu phân trang qua ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalOrders / (double)pageSize);

            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> FilterOrders(
            DateTime? fromDate,
            DateTime? toDate,
            string paymentMethod,
            string status,
            string search,
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.Orders.AsQueryable();

            // Lọc theo khoảng ngày
            if (fromDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(o => o.OrderDate <= toDate.Value);
            }

            // Lọc theo phương thức thanh toán
            if (!string.IsNullOrEmpty(paymentMethod))
            {
                //query = query.Where(o => o.PaymentMethod == paymentMethod);
                if (paymentMethod == "COD")
                {
                    query = query.Where(o => o.PaymentMethod.Contains("COD")); // Lọc theo COD
                }
                else if (paymentMethod == "Momo")
                {
                    query = query.Where(o => o.PaymentMethod.Contains("Momo"));
                }
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status == status);
            }

            // Tìm kiếm (theo tên khách hàng hoặc mã đơn hàng)
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(o => o.Name.Contains(search) || o.Id.ToString().Contains(search));
            }

            // Tổng số đơn hàng
            int totalOrders = await query.CountAsync();

            // Phân trang
            var filteredOrders = await query
                .Include(o => o.OrderDetails)
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Truyền dữ liệu vào ViewBag để sử dụng trong view
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalOrders / pageSize);
            ViewBag.TotalOrders = totalOrders;  // Thêm số lượng tổng đơn hàng
            ViewBag.Search = search;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.PaymentMethod = paymentMethod;
            ViewBag.Status = status;

            return View("ManageOrders", filteredOrders); // Hiển thị lại danh sách đơn hàng
        }

        [HttpGet]
        public async Task<IActionResult> OrderDetails(int id)
        {
            // Lấy thông tin đơn hàng từ cơ sở dữ liệu
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Book) // Nếu có thông tin sách trong chi tiết đơn hàng
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                TempData["error"] = "Đơn hàng không tồn tại.";
                return RedirectToAction("ManageOrders");
            }

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int id, string status)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)  // Bao gồm các chi tiết đơn hàng
                .ThenInclude(od => od.Book)  // Bao gồm sách trong chi tiết đơn hàng
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                TempData["error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("ManageOrders");
            }

            // Lưu lại trạng thái cũ để xác định việc cập nhật số lượng tồn kho
            var oldStatus = order.Status;

            // Cập nhật trạng thái đơn hàng
            order.Status = status;
            await _context.SaveChangesAsync();

            // Cập nhật tồn kho khi đơn hàng được cập nhật
            //if (oldStatus != "Đã huỷ" && status == "Đã huỷ")
            //{
            //    // Nếu đơn hàng chuyển sang trạng thái "Đã huỷ", cập nhật lại tồn kho
            //    foreach (var orderDetail in order.OrderDetails)
            //    {
            //        var book = orderDetail.Book;
            //        book.StockQuantity += orderDetail.Quantity;  // Thêm lại số lượng sách đã bị giảm
            //    }
            //}
            //else if (oldStatus == "Đang giao hàng" && status == "Giao hàng thành công")
            //{
            //    // Nếu đơn hàng chuyển sang trạng thái "Giao hàng thành công", giảm tồn kho
            //    foreach (var orderDetail in order.OrderDetails)
            //    {
            //        var book = orderDetail.Book;
            //        book.StockQuantity -= orderDetail.Quantity;  // Giảm số lượng sách trong kho
            //    }
            //}
            // await _context.SaveChangesAsync();  // Lưu thay đổi tồn kho vào cơ sở dữ liệu
            // TempData["success"] = "Cập nhật trạng thái đơn hàng thành công.";

            if (oldStatus != status)
            {
                TempData["success"] = "Cập nhật trạng thái đơn hàng thành công.";
            }
            else
            {
                TempData["error"] = "Trạng thái không thay đổi.";
            }

            return RedirectToAction("ManageOrders");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)  // Bao gồm chi tiết đơn hàng
                .ThenInclude(od => od.Book)  // Bao gồm sách trong chi tiết đơn hàng
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                TempData["error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("ManageOrders");
            }

            // Kiểm tra trạng thái của đơn hàng trước khi xóa
            //if (order.Status == "Đã huỷ")
            //{
            //    // Nếu đơn hàng đã huỷ, hoàn tác việc giảm tồn kho
            //    foreach (var orderDetail in order.OrderDetails)
            //    {
            //        var book = orderDetail.Book;
            //        book.StockQuantity += orderDetail.Quantity;  // Thêm lại số lượng sách trong kho
            //    }
            //}

            // Xóa chi tiết đơn hàng trước
            _context.OrderDetails.RemoveRange(order.OrderDetails);

            // Xóa đơn hàng
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            TempData["success"] = "Đơn hàng đã được xóa và số lượng tồn kho đã được cập nhật.";
            return RedirectToAction("ManageOrders");
        }

        //-------------------------------------------------------------
        // XÁC NHẬN ĐƠN HÀNG ------------------------------------------
        [HttpPost]
        public async Task<IActionResult> ConfirmOrder(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails) // Bao gồm chi tiết đơn hàng
                .ThenInclude(od => od.Book) // Bao gồm sách trong đơn hàng
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                TempData["error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("FilterOrders");
            }

            if (order.Status != "Chờ xác nhận")
            {
                TempData["error"] = "Đơn hàng không thể xác nhận vì không ở trạng thái 'Chờ xác nhận'.";
                return RedirectToAction("FilterOrders");
            }

            // Cập nhật trạng thái đơn hàng thành 'Đang giao hàng'
            order.Status = "Đang giao hàng";

            await _context.SaveChangesAsync();
            TempData["success"] = "Đơn hàng đã được xác nhận và chuyển sang trạng thái 'Đang giao hàng'.";
            return RedirectToAction("FilterOrders");
        }

        // TỪ CHỐI ĐƠN HÀNG ------------------------------------------
        [HttpPost]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails) // Bao gồm chi tiết đơn hàng
                .ThenInclude(od => od.Book) // Bao gồm sách trong đơn hàng
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                TempData["error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("FilterOrders");
            }

            if (order.Status != "Chờ xác nhận")
            {
                TempData["error"] = "Đơn hàng không thể huỷ vì không ở trạng thái 'Chờ xác nhận'.";
                return RedirectToAction("FilterOrders");
            }

            // Cập nhật trạng thái đơn hàng thành 'Đã huỷ'
            order.Status = "Đã huỷ";

            // Cập nhật số lượng tồn kho của các sách trong đơn hàng
            foreach (var item in order.OrderDetails)
            {
                var book = item.Book;
                book.StockQuantity += item.Quantity; // Thêm số lượng sách vào kho
            }

            await _context.SaveChangesAsync();
            TempData["success"] = "Đơn hàng đã được huỷ và số lượng kho được cập nhật.";
            return RedirectToAction("FilterOrders");
        }

        //----------------------------------------------------------------
        // GIAO HÀNG THÀNH CÔNG ------------------------------------------
        [HttpPost]
        public async Task<IActionResult> MarkOrderAsDelivered(int orderId)
        {
            //var order = await _context.Orders.FindAsync(orderId);
            var order = await _context.Orders
                 .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Book)
                .FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
            {
                return NotFound();
            }

            // Kiểm tra trạng thái đơn hàng
            if (order.Status != "Đang giao hàng")
            {
                TempData["error"] = "Đơn hàng không thể giao vì không ở trạng thái 'Đang giao hàng'.";
                return RedirectToAction("FilterOrders");
            }

            // Cập nhật trạng thái đơn hàng thành "Giao hàng thành công"
            order.Status = "Giao hàng thành công";

            // Cập nhật số lượng bán của từng sản phẩm
            foreach (var detail in order.OrderDetails)
            {
                detail.Book.SoldQuantity += detail.Quantity;
            }

            await _context.SaveChangesAsync();

            TempData["success"] = "Đơn hàng đã Giao hàng thành công.";
            return RedirectToAction("FilterOrders");
        }

        // GIAO HÀNG THẤT BẠI ------------------------------------------
        [HttpPost]
        public async Task<IActionResult> MarkOrderAsFailed(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Book)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null || order.Status != "Đang giao hàng")
            {
                TempData["error"] = "Không thể cập nhật đơn hàng.";
                return RedirectToAction("FilterOrders");
            }

            // Cập nhật trạng thái đơn hàng thành "Giao hàng thất bại"
            order.Status = "Đã huỷ";

            // Cập nhật số lượng sách trong kho
            foreach (var item in order.OrderDetails)
            {
                item.Book.StockQuantity += item.Quantity;
            }

            await _context.SaveChangesAsync();

            TempData["success"] = "Đơn hàng đã được cập nhật là Đã huỷ.";
            return RedirectToAction("FilterOrders");
        }

        //---------------------------------------------------------------------------------------
        // XÁC NHẬN HUỶ ĐƠN HÀNG TỪ KHÁCH HÀNG YÊU CẦU ------------------------------------------
        [HttpPost]
        public async Task<IActionResult> ConfirmCancel(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails) // Bao gồm chi tiết đơn hàng
                .ThenInclude(od => od.Book) // Bao gồm sách trong đơn hàng
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                TempData["error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("FilterOrders");
            }

            if (order.Status != "Đang yêu cầu huỷ")
            {
                TempData["error"] = "Đơn hàng không yêu cầu huỷ.";
                return RedirectToAction("FilterOrders");
            }

            // Cập nhật trạng thái đơn hàng thành 'Đã huỷ'
            order.Status = "Đã huỷ";

            // Cập nhật số lượng tồn kho
            foreach (var item in order.OrderDetails)
            {
                var book = item.Book;
                book.StockQuantity += item.Quantity; // Cộng lại số lượng sách vào kho
            }

            await _context.SaveChangesAsync();
            TempData["success"] = "Đơn hàng đã được huỷ.";
            return RedirectToAction("FilterOrders");
        }

        // TỪ CHỐI HUỶ ĐƠN HÀNG TỪ KHÁCH HÀNG YÊU CẦU ------------------------------------------
        [HttpPost]
        public async Task<IActionResult> RejectCancel(int orderId)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                TempData["error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("FilterOrders");
            }

            if (order.Status != "Đang yêu cầu huỷ")
            {
                TempData["error"] = "Đơn hàng không yêu cầu huỷ.";
                return RedirectToAction("FilterOrders");
            }

            // Cập nhật trạng thái đơn hàng thành trạng thái ban đầu
            order.Status = "Chờ xác nhận"; // Hoặc trạng thái ban đầu

            await _context.SaveChangesAsync();
            TempData["success"] = "Yêu cầu huỷ đã bị từ chối.";
            return RedirectToAction("FilterOrders");
        }        
    }
}
