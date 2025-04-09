using BookWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
namespace BookWeb.Component
{
    public class UserViewComponent: ViewComponent
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserViewComponent(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.GetUserAsync(UserClaimsPrincipal);
            var fullName = !string.IsNullOrEmpty(user.FullName) ? $" {user.FullName}" : "Tài khoản";
            return View("Default", fullName);
        }
       
    }
}
