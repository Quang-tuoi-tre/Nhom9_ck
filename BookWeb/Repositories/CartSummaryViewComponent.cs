using BookWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BookWeb.Repositories
{
    public class CartSummaryViewComponent : ViewComponent
    {
        public CartSummaryViewComponent() { }

        public IViewComponentResult Invoke()
        {
            List<CartItem> cart = GetCartItems();
            CartSummaryViewModel viewModel = new CartSummaryViewModel();
            viewModel.NumberOfItems = cart.Count;
            return View(viewModel);
        }

        List<CartItem> GetCartItems()
        {
            var sessionCart = HttpContext.Session.GetString("cart");
            if (sessionCart != null)
            {
                return JsonConvert.DeserializeObject<List<CartItem>>(sessionCart);
            }
            return new List<CartItem>();
        }
    }
}
