namespace BookWeb.Models
{
    public class MomoInfoModel
    {
        public int Id { get; set; }
        public string OrderId { get; set; }
        public string OrderInfo { get; set; }
        public string FullName { get; set; }
        public decimal Amount { get; set; }
        public DateTime DatePaid { get; set; }
        public string Status { get; set; }  // "Success" or "Failed"
    }
}
