using BookWeb.Models;

namespace BookWeb.Repositories
{
    public interface IOrderRepo
    {
        Order GetOrderById(int id); // Lấy đơn hàng theo ID
        void UpdateOrder(Order order); // Cập nhật thông tin đơn hàng
        void Save(); // Lưu thay đổi vào cơ sở dữ liệu
        List<Order> GetOrdersByUserId(string userId); // Lấy danh sách đơn hàng của người dùng
    }
}
