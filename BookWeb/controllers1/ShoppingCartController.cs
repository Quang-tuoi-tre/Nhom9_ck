using BookWeb.Data;
using BookWeb.Models;
using BookWeb.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using BookWeb.Services.Momo;
using BookWeb.Services.Vnpay;
using BookWeb.Models.Vnpay;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace BookWeb.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBookRepo _bookRepository;
        private readonly IVnPayService _vnPayService;
        private IMomoService _momoService;
        private readonly IOrderRepo _orderRepo;

        public ShoppingCartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IBookRepo bookRepository, IMomoService momoService, IVnPayService vnPayService, IOrderRepo orderRepo)
        {
            _context = context;
            _userManager = userManager;
            _bookRepository = bookRepository;
            _momoService = momoService;
            _vnPayService = vnPayService;
            _orderRepo = orderRepo;
        }

        public async Task<ActionResult> Index()
        {
            var carts = GetCartItems();
            var totalAmount = carts.Sum(p => p.Price * p.Quantity);

            foreach (var item in carts)
            {
                item.StockQuantity = _bookRepository.GetStockQuantity(item.Id);
            }

            // Lấy thông tin người dùng
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                ViewData["FullName"] = user.FullName;
                ViewData["PhoneNumber"] = user.PhoneNumber;
                ViewData["Address"] = user.Address;
            }

            ViewBag.TotalAmount = totalAmount; // Gán giá trị tổng tiền vào ViewBag
            ViewBag.TongTien = 0;
            ViewBag.TongSoLuong = carts.Sum(p => p.Quantity);

            return View(carts);
        }

        //----------------------------------------------------------
        public async Task<ActionResult> AddToCart(int id)
        {
            Book? itemProduct = _context.Books.FirstOrDefault(p => p.Id == id);
            if (itemProduct == null)
                return BadRequest("Sản phẩm không tồn tại!");

            var carts = GetCartItems();
            var findCartItem = carts.FirstOrDefault(p => p.Id.Equals(id));
            if (findCartItem == null)
            {
                // Thêm mới sản phẩm nếu còn hàng
                if (itemProduct.StockQuantity > 0)
                {
                    // Thêm mới sản phẩm vào giỏ hàng
                    findCartItem = new CartItem()
                    {
                        Id = itemProduct.Id,
                        Name = itemProduct.Name,
                        Image = itemProduct.ImageUrl,
                        Price = itemProduct.Price,
                        Quantity = 1
                    };
                    carts.Add(findCartItem);
                }
                else
                {
                    return BadRequest("Sản phẩm đã hết hàng!");
                }
            }
            else
            {
                // Kiểm tra số lượng tồn kho trước khi tăng
                if (findCartItem.Quantity < itemProduct.StockQuantity)
                {
                    findCartItem.Quantity++;
                }
                else
                {
                    return BadRequest("Vượt quá số lượng tồn kho!");
                }
            }

            SaveCartSession(carts);

            // Kiểm tra xem người dùng có nhấn "Mua ngay" không để cập nhật giá trị tổng tiền
            if (Request.Headers["Referer"].ToString().Contains("Details"))
            {
                // Cập nhật giá trị tổng tiền khi nhấn "Mua ngay"
                ViewBag.TongTien = carts.Sum(p => p.Price * p.Quantity);
                return RedirectToAction("Index", "ShoppingCart"); // Quay lại giỏ hàng và cập nhật tổng tiền
            }

            // Nếu là "Thêm vào giỏ hàng", không cập nhật tổng tiền
            ViewBag.TongTien = carts.Sum(p => p.Price * p.Quantity);
            return RedirectToAction("Index", "ShoppingCart"); // Quay lại giỏ hàng mà không cập nhật tổng tiền
        }

        public ActionResult UpdateCart(int id, int quantity, bool isChecked)
        {
            if (quantity <= 0) return BadRequest("Số lượng phải lớn hơn 0!");

            var carts = GetCartItems();
            var findCartItem = carts.FirstOrDefault(p => p.Id == id);

            if (findCartItem != null)
            {
                var product = _context.Books.FirstOrDefault(b => b.Id == id);
                if (product == null) return BadRequest("Sản phẩm không tồn tại!");

                // Kiểm tra số lượng tồn kho
                if (quantity <= product.StockQuantity)
                {
                    findCartItem.Quantity = quantity;
                    findCartItem.IsSelected = isChecked;
                    SaveCartSession(carts);
                }
                else
                {
                    return BadRequest("Vượt quá số lượng tồn kho!");
                }
            }

            // Nếu checkbox không được chọn, không tính sản phẩm này vào tổng tiền
            var selectedItems = isChecked
                ? carts.Where(p => p.Id == id).ToList()
                : new List<CartItem>();
            ViewBag.TongTien = selectedItems.Sum(p => p.Price * p.Quantity);

            ViewBag.StockQuantities = _context.Books.ToDictionary(b => b.Id, b => b.StockQuantity);

            return RedirectToAction("Index");
        }

        public ActionResult DeleteCart(int id)
        {
            var carts = GetCartItems();
            var findCartItem = carts.FirstOrDefault(p => p.Id == id);
            if (findCartItem != null)
            {
                carts.Remove(findCartItem);
                SaveCartSession(carts);
            }

            // Kiểm tra nếu giỏ hàng trống, hiển thị giao diện thông báo
            if (!carts.Any())
            {
                ViewBag.IsCartEmpty = true; // Đánh dấu giỏ hàng trống
                return View("Index", carts); // Trả về giao diện giỏ hàng
            }

            return RedirectToAction("Index");
        }

        //---------------------------------------------------------------  
        public ActionResult Delete()
        {
            HttpContext.Session.Remove("cart");
            return RedirectToAction("Index");
        }

        private void ClearCartItems()
        {
            var carts = new List<CartItem>();
            SaveCartSession(carts);
        }

        private async Task<Book> GetProductFromDatabase(int productId)
        {
            // Truy vấn cơ sở dữ liệu để lấy thông tin sản phẩm
            return await _bookRepository.GetByIdAsync(productId);
        }

        List<CartItem> GetCartItems()
        {
            string? jsoncart = HttpContext.Session.GetString("cart");
            return jsoncart != null ? JsonConvert.DeserializeObject<List<CartItem>>(jsoncart) ?? new List<CartItem>() : new List<CartItem>();
        }

        void SaveCartSession(List<CartItem> ls)
        {
            string jsoncart = JsonConvert.SerializeObject(ls);
            HttpContext.Session.SetString("cart", jsoncart);
        }

        //-----------------------------------------------------------
        // Các actions khác...
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var carts = GetCartItems();
            var totalAmount = carts.Sum(i => i.Price * i.Quantity);
            ViewBag.TotalAmount = totalAmount; // Gán giá trị tổng tiền vào ViewBag

            // Lấy thông tin người dùng
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                ViewData["FullName"] = user.FullName;
                ViewData["PhoneNumber"] = user.PhoneNumber;
                ViewData["Address"] = user.Address;
            }

            return View(new Order());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Checkout(string PaymentMethod)
        {
            // Lấy giỏ hàng từ session
            var carts = GetCartItems();

            // Xác thực giỏ hàng
            if (carts == null || !carts.Any())
            {// Giỏ hàng trống, quay lại trang giỏ hàng
                TempData["ErrorMessage"] = "Giỏ hàng của bạn hiện tại không có sản phẩm.";
                // Giỏ hàng trống, quay lại trang Index
                return RedirectToAction("Index");
            }

            foreach (var item in carts)
            {
                var product = await _context.Books.FirstOrDefaultAsync(b => b.Id == item.Id);
                if (product == null || product.StockQuantity < item.Quantity)
                {
                    return BadRequest($"Sản phẩm '{item.Name}' không đủ số lượng.");
                }
            }

            // Xác thực người dùng
            var user = await _userManager.GetUserAsync(User);
            var order = new Order();

            if (user != null)
            {
                order.UserId = user.Id; // Gán ID người dùng nếu đăng nhập
            }
            else
            {
                order.UserId = null; // Để trống nếu người dùng không đăng nhập
                //order.Notes = "Thanh toán bằng COD";
            }

            order.Name = order.Name ?? user.FullName;
            order.PhoneNumber = order.PhoneNumber ?? user.PhoneNumber;
            order.ShippingAddress = order.ShippingAddress ?? user.Address;
            order.Notes = order.Notes ?? "";

            if (PaymentMethod != null)
            {
                order.PaymentMethod = "Momo - " + PaymentMethod;
            }
            else
            {
                order.PaymentMethod = "Thanh toán khi nhận hàng (COD)";
            }

            order.Status = "Chờ xác nhận";
            order.OrderDate = DateTime.UtcNow;
            order.TotalPrice = carts.Sum(i => i.Price * i.Quantity);
            order.OrderDetails = carts.Select(i => new OrderDetail
            {
                BookId = i.Id,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList();

            _context.Orders.Add(order);

            // Cập nhật tồn kho
            foreach (var item in carts)
            {
                var product = await _context.Books.FirstOrDefaultAsync(b => b.Id == item.Id);
                if (product != null)
                {
                    product.StockQuantity -= item.Quantity;
                }
            }

            await _context.SaveChangesAsync();

            // Xóa giỏ hàng sau khi thanh toán
            HttpContext.Session.Remove("cart");

            // Chuyển đến trang xác nhận hoàn tất đơn hàng
            return View("OrderCompleted", order);
        }

        //-------------------------------------
        // 30/11 - MOMO
        [Authorize]
        [HttpPost]
        [Route("CreatePaymentUrl")]
        public async Task<IActionResult> CreatePaymentMomo(OrderInfo model)
        {
            var response = await _momoService.CreatePaymentMomo(model);
            // Chuyển hướng người dùng tới trang thanh toán MoMo
            return Redirect(response.PayUrl);
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallBack(MomoInfoModel model)
        {
            // Gửi thông tin thanh toán đến dịch vụ MoMo
            var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
            var requestQuery = HttpContext.Request.Query;

            if (requestQuery["resultCode"].ToString() != "0") //giao dịch ko thành công lưu db nên là == 0 //chuỗi hay số nhỉ
            {
                var newMomoInsert = new MomoInfoModel
                {
                    OrderId = requestQuery["orderId"],
                    FullName = User.FindFirstValue(ClaimTypes.Email),
                    Amount = decimal.Parse(requestQuery["amount"]),
                    OrderInfo = requestQuery["orderInfo"],
                    DatePaid = DateTime.Now,
                    Status = "Success"
                };

                _context.Add(newMomoInsert);
                await _context.SaveChangesAsync();

                //Tiến hành đặt đơn hàng khi thanh toán momo thành công
                await Checkout(requestQuery["orderId"]);
            }
            else
            {
                TempData["success"] = "Giao dịch Momo ko thành công.";
                return RedirectToAction("Index", "ShoppingCart");
            }

            return View(response);
        }

        //-------------------------------------
        // 01/12 - VNPAY
        [HttpPost]
        public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model)
        {
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);
            return Redirect(url);
        }

        [HttpGet]
        public IActionResult PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            return Json(response);
        }

        //-------------------------------------
        [Authorize]
        public async Task<IActionResult> OrderHistory(int pageNumber = 1, int pageSize = 10)
        {
            // Lấy thông tin người dùng hiện tại
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                TempData["error"] = "Bạn cần đăng nhập để xem lịch sử đơn hàng.";
                return RedirectToAction("Login", "Account");
            }

            // Truy vấn tổng số lượng đơn hàng của người dùng
            var totalOrders = await _context.Orders
                .Where(o => o.UserId == user.Id)
                .CountAsync();

            // Tính tổng số trang
            var totalPages = (int)Math.Ceiling(totalOrders / (double)pageSize);

            // Truy vấn danh sách đơn hàng của người dùng
            var orders = await _context.Orders
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.OrderDate) // Sắp xếp theo ngày mới nhất
                .Skip((pageNumber - 1) * pageSize)  // Bỏ qua các đơn hàng ở các trang trước
                .Take(pageSize) // Lấy số lượng đơn hàng theo trang
                .ToListAsync();

            // Lưu thông tin phân trang vào ViewBag
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;

            return View(orders);
        }

        // chi tiết đơn hàng
        [Authorize]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails) // Include chi tiết đơn hàng
                .ThenInclude(od => od.Book) // Nếu bạn muốn lấy thông tin sách
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == _userManager.GetUserId(User));

            if (order == null)
            {
                TempData["error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("OrderHistory");
            }

            return View(order);
        }

        // Yêu cầu Hủy đơn hàng
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RequestCancel(int orderId)
        {
            var user = await _userManager.GetUserAsync(User);
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == user.Id);

            if (order == null)
            {
                TempData["error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("OrderHistory");
            }

            if (order.Status != "Chờ xác nhận")
            {
                TempData["error"] = "Đơn hàng không thể yêu cầu huỷ vì không ở trạng thái 'Chờ xác nhận'.";
                return RedirectToAction("OrderHistory");
            }

            // Cập nhật trạng thái yêu cầu huỷ
            order.Status = "Đang yêu cầu huỷ"; // Trạng thái yêu cầu huỷ
            await _context.SaveChangesAsync();

            TempData["success"] = "Yêu cầu huỷ đơn hàng của bạn đã được gửi. Vui lòng chờ admin xác nhận.";
            return RedirectToAction("OrderHistory");
        }

        // Xác nhận đã nhận hàng
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ConfirmDelivery(int orderId)
        {
            // Lấy đơn hàng của người dùng
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == _userManager.GetUserId(User));

            if (order == null)
            {
                TempData["error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("OrderHistory");
            }

            // Kiểm tra trạng thái đơn hàng, chỉ cho phép xác nhận nếu trạng thái là 'Đang giao hàng'
            if (order.Status != "Đang giao hàng")
            {
                TempData["error"] = "Đơn hàng không thể xác nhận nhận hàng vì không ở trạng thái 'Đang giao hàng'.";
                return RedirectToAction("OrderHistory");
            }

            // Cập nhật trạng thái đơn hàng thành 'Giao hàng thành công'
            order.Status = "Giao hàng thành công";

            // Lưu thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            TempData["success"] = "Đơn hàng đã được xác nhận là 'Giao hàng thành công'.";
            return RedirectToAction("OrderHistory");
        }
    }
}
