using System;
using System.Text;
using System.Threading.Tasks;
using ArtGallery.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace ArtGallery.Areas.Identity.Pages.Account
{
    public class ConfirmEmailChangeModel : PageModel
    {
        private readonly UserManager<NguoiDung> _userManager;
        private readonly SignInManager<NguoiDung> _signInManager;

        public ConfirmEmailChangeModel(UserManager<NguoiDung> userManager, SignInManager<NguoiDung> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public bool IsConfirmed { get; set; } = false;

        public async Task<IActionResult> OnGetAsync(string userId, string email, string code)
        {
            if (userId == null || email == null || code == null)
            {
                StatusMessage = "Yêu cầu xác nhận không hợp lệ.";
                return Page();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                StatusMessage = "Không tìm thấy người dùng.";
                return Page();
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ChangeEmailAsync(user, email, code);
            if (!result.Succeeded)
            {
                StatusMessage = "Lỗi khi xác nhận địa chỉ email.";
                return Page();
            }

            // Đồng thời cập nhật tên đăng nhập nếu sử dụng email làm tên đăng nhập
            var setUserNameResult = await _userManager.SetUserNameAsync(user, email);
            if (!setUserNameResult.Succeeded)
            {
                StatusMessage = "Lỗi khi cập nhật tên người dùng.";
                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
            IsConfirmed = true;
            StatusMessage = "Cảm ơn bạn đã xác nhận thay đổi email.";

            // Lưu trạng thái thành công vào TempData để hiển thị khi quay lại trang Email
            TempData["EmailConfirmSuccess"] = true;
            TempData["NewEmail"] = email;
            
            return Page();
        }
    }
} 