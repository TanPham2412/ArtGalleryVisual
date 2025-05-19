using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using ArtGallery.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.Linq;

namespace ArtGallery.Areas.Identity.Pages.Account.Manage
{
    public class EmailModel : PageModel
    {
        private readonly UserManager<NguoiDung> _userManager;
        private readonly SignInManager<NguoiDung> _signInManager;
        private readonly EmailService _emailService;

        public EmailModel(
            UserManager<NguoiDung> userManager,
            SignInManager<NguoiDung> signInManager,
            EmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
        }

        [Display(Name = "Email hiện tại")]
        public string Email { get; set; }

        public bool IsEmailConfirmed { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public bool IsExternalAccount { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập địa chỉ email mới")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ")]
            [Display(Name = "Email mới")]
            public string NewEmail { get; set; }
        }

        private async Task LoadAsync(NguoiDung user)
        {
            var email = await _userManager.GetEmailAsync(user);
            Email = email;

            Input = new InputModel
            {
                NewEmail = email,
            };

            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
            
            var logins = await _userManager.GetLoginsAsync(user);
            IsExternalAccount = logins.Any(l => l.LoginProvider.Equals("Google", StringComparison.OrdinalIgnoreCase));
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Không thể tải thông tin người dùng với ID '{_userManager.GetUserId(User)}'.");
            }

            if (TempData["EmailConfirmSuccess"] != null && (bool)TempData["EmailConfirmSuccess"])
            {
                StatusMessage = "Email của bạn đã được thay đổi thành công.";
                if (TempData["NewEmail"] != null)
                {
                    user = await _userManager.FindByIdAsync(user.Id);
                }
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostChangeEmailAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Không thể tải thông tin người dùng với ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var email = await _userManager.GetEmailAsync(user);
            if (Input.NewEmail != email)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateChangeEmailTokenAsync(user, Input.NewEmail);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmailChange",
                    pageHandler: null,
                    values: new { userId = userId, email = Input.NewEmail, code = code },
                    protocol: Request.Scheme);
                
                try
                {
                    await _emailService.SendEmailConfirmationAsync(
                        Input.NewEmail,
                        user.TenNguoiDung ?? user.UserName,
                        callbackUrl);

                    StatusMessage = "Đã gửi liên kết xác nhận thay đổi email. Vui lòng kiểm tra email của bạn.";
                    return RedirectToPage();
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Lỗi khi gửi email: {ex.Message}";
                    return RedirectToPage();
                }
            }

            StatusMessage = "Email của bạn không thay đổi.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSendVerificationEmailAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Không thể tải thông tin người dùng với ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = userId, code = code },
                protocol: Request.Scheme);
            
            try
            {
                await _emailService.SendEmailVerificationAsync(
                    email,
                    user.TenNguoiDung ?? user.UserName,
                    callbackUrl);

                StatusMessage = "Đã gửi email xác nhận. Vui lòng kiểm tra email của bạn.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Lỗi khi gửi email: {ex.Message}";
                return RedirectToPage();
            }
        }
    }
} 