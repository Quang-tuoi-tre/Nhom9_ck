using BookWeb.Models;

namespace BookWeb.Repositories
{
    public class ShoppingCart
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        public void AddItem(CartItem item)
        {
            var existingItem = Items.FirstOrDefault(i => i.Id == item.Id);
            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                Items.Add(item);
            }
        }

        public void RemoveItem(int productId)
        {
            Items.RemoveAll(i => i.Id == productId);
        }

        //SỬA------------------------------------------------
        public void UpdateItem(int productId, int newQuantity)
        {
            var existingItem = Items.FirstOrDefault(i => i.Id == productId);
            if (existingItem != null)
            {
                existingItem.Quantity = newQuantity;
            }
        }

        //--------Tổng số lượng sản phẩm trong giỏ hàng---------
        public int GetTotalQuantity()
        {
            return Items.Sum(item => item.Quantity);
        }
    }
}
