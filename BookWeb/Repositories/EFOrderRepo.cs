using BookWeb.Data;
using BookWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace BookWeb.Repositories
{
    public class EFOrderRepo : IOrderRepo
    {
        private readonly ApplicationDbContext _context;

        public EFOrderRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        // Lấy đơn hàng theo ID
        public Order GetOrderById(int id)
        {
            return _context.Orders
                           .Include(o => o.OrderDetails)
                           .ThenInclude(od => od.Book)
                           .FirstOrDefault(o => o.Id == id);
        }

        // Cập nhật thông tin đơn hàng
        public void UpdateOrder(Order order)
        {
            _context.Orders.Update(order);
            Save();
        }

        // Lưu thay đổi vào cơ sở dữ liệu
        public void Save()
        {
            _context.SaveChanges();
        }

        // Lấy danh sách đơn hàng của người dùng
        public List<Order> GetOrdersByUserId(string userId)
        {
            return _context.Orders
                           .Where(o => o.UserId == userId)
                           .Include(o => o.OrderDetails)
                           .ThenInclude(od => od.Book)
                           .ToList();
        }
    }
}
