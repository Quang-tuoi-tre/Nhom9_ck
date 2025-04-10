// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using BookWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookWeb.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [MaxLength(100, ErrorMessage = "Họ Tên không được vượt quá 100 ký tự.")]
            [Display(Name = "Họ Tên")]
            public string FullName { get; set; }

            [MaxLength(255, ErrorMessage = "Địa Chỉ không được vượt quá 255 ký tự.")]
            [Display(Name = "Địa Chỉ")]
            public string Address { get; set; }

            [Phone]
            [StringLength(11, MinimumLength = 10, ErrorMessage = "Số điện thoại phải có từ 10 đến 11 ký tự.")]
            [RegularExpression(@"^\d{10,11}$", ErrorMessage = "Số điện thoại chỉ có thể chứa số và phải có từ 10 đến 11 ký tự.")]
            [Display(Name = "Số điện thoại")]
            public string PhoneNumber { get; set; }

            // 25/11 - URL của avatar
            [Display(Name = "Ảnh đại diện")]
            public string AvatarUrl { get; set; }

            // Dùng để upload file
            [Display(Name = "Tải ảnh đại diện")]
            [DataType(DataType.Upload)]
            public IFormFile AvatarFile { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = user.PhoneNumber,
                FullName = user.FullName,
                Address = user.Address,
                // 25/11 - URL của avatar
                AvatarUrl = user.AvatarUrl
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);

            // Kiểm tra xem người dùng có tài khoản Google không
            var linkedAccounts = await _userManager.GetLoginsAsync(user);
            var isGoogleAccount = linkedAccounts.Any(login => login.LoginProvider == "Google");

            ViewData["IsGoogleAccount"] = isGoogleAccount;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            // Cập nhật các trường bổ sung
            user.FullName = Input.FullName;
            user.Address = Input.Address;

            // 25/11 - Xử lý avatar
            if (Input.AvatarFile != null)
            {
                var uploadPath = Path.Combine("wwwroot", "uploads", "avatars");
                Directory.CreateDirectory(uploadPath);

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(Input.AvatarFile.FileName)}";
                var filePath = Path.Combine(uploadPath, fileName);

                // Đảm bảo đường dẫn hợp lệ
                if (!filePath.StartsWith(uploadPath))
                {
                    throw new InvalidOperationException("Đường dẫn file không hợp lệ.");
                }

                // Lưu file mới
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Input.AvatarFile.CopyToAsync(stream);
                }

                // Xóa file cũ nếu có
                if (!string.IsNullOrEmpty(user.AvatarUrl))
                {
                    var oldFilePath = Path.Combine("wwwroot", user.AvatarUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        try
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Không thể xóa file cũ: {ex.Message}");
                        }
                    }
                }

                // Cập nhật đường dẫn mới
                user.AvatarUrl = $"/uploads/avatars/{fileName}";
            }

            //if (Input.AvatarFile != null)
            //{
            //    var uploadPath = Path.Combine("wwwroot", "uploads", "avatars");
            //    Directory.CreateDirectory(uploadPath);

            //    var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(Input.AvatarFile.FileName)}";
            //    var filePath = Path.Combine(uploadPath, fileName);

            //    using (var stream = new FileStream(filePath, FileMode.Create))
            //    {
            //        await Input.AvatarFile.CopyToAsync(stream);
            //    }

            //    // Xóa ảnh cũ nếu có
            //    if (!string.IsNullOrEmpty(user.AvatarUrl))
            //    {
            //        var oldFilePath = Path.Combine("wwwroot", user.AvatarUrl.TrimStart('/'));
            //        if (System.IO.File.Exists(oldFilePath))
            //        {
            //            System.IO.File.Delete(oldFilePath);
            //        }
            //    }

            //    user.AvatarUrl = $"/uploads/avatars/{fileName}";
            //}

            // Lưu các thay đổi
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                StatusMessage = "Unexpected error when updating profile.";
                return RedirectToPage();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
