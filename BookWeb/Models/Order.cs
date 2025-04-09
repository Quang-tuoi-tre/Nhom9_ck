
namespace BookWeb.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string ShippingAddress { get; set; }
        public string? Notes { get; set; }
        public string? PaymentMethod { get; set; }
        public string? Status { get; set; }  // "Paid", "Pending", etc.
        public string? UserId { get; set; }
        public ApplicationUser User { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }
    }
}
