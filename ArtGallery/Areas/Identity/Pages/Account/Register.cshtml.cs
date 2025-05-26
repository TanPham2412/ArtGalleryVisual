// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using ArtGallery.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace ArtGallery.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<NguoiDung> _signInManager;
        private readonly UserManager<NguoiDung> _userManager;
        private readonly IUserStore<NguoiDung> _userStore;
        private readonly IUserEmailStore<NguoiDung> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<NguoiDung> userManager,
            IUserStore<NguoiDung> userStore,
            SignInManager<NguoiDung> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
            [Display(Name = "Tên đăng nhập")]
            [StringLength(100, ErrorMessage = "Tên đăng nhập phải có ít nhất {2} và tối đa {1} ký tự.", MinimumLength = 3)]
            public string UserName { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập địa chỉ email")]
            [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
            [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} và tối đa {1} ký tự.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu")]
            public string Password { get; set; }

            [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
            [DataType(DataType.Password)]
            [Display(Name = "Xác nhận mật khẩu")]
            [Compare("Password", ErrorMessage = "Mật khẩu và xác nhận mật khẩu không khớp.")]
            public string ConfirmPassword { get; set; }
            
            //[Required(ErrorMessage = "Vui lòng đồng ý với điều khoản & điều kiện")]
            //[Range(typeof(bool), "true", "true", ErrorMessage = "Vui lòng đồng ý với điều khoản & điều kiện")]
            //[Display(Name = "Tôi đồng ý với điều khoản & điều kiện")]
            //public bool AgreeToTerms { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            _logger.LogInformation("=== BẮT ĐẦU XỬ LÝ ĐĂNG KÝ ===");
            _logger.LogInformation("UserName: {UserName}, Email: {Email}", Input?.UserName, Input?.Email);

            returnUrl = "/Home/LoginRegister";
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            // Kiểm tra ModelState
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState không hợp lệ:");
                foreach (var modelState in ModelState)
                {
                    foreach (var error in modelState.Value.Errors)
                    {
                        _logger.LogWarning("Lỗi {Key}: {Error}", modelState.Key, error.ErrorMessage);
                    }
                }
                return Page();
            }

            _logger.LogInformation("ModelState hợp lệ, bắt đầu tạo user");

            try
            {
                var user = CreateUser();
                user.UserName = Input.UserName;
                user.TenNguoiDung = Input.UserName;
                user.Email = Input.Email;
                user.NgayTao = DateTime.Now;

                _logger.LogInformation("Đã tạo user object, bắt đầu set thông tin");

                await _userStore.SetUserNameAsync(user, Input.UserName, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                _logger.LogInformation("Bắt đầu tạo user trong database");
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("✅ Tạo user thành công!");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation("✅ Đăng nhập thành công, chuyển hướng");
                    return Redirect("/Home/LoginRegister");
                }

                _logger.LogError("❌ Tạo user thất bại:");
                foreach (var error in result.Errors)
                {
                    _logger.LogError("Lỗi {Code}: {Description}", error.Code, error.Description);
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Exception khi tạo user");
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra trong quá trình đăng ký");
            }

            _logger.LogInformation("=== KẾT THÚC XỬ LÝ ĐĂNG KÝ ===");
            return Page();
        }


        private NguoiDung CreateUser()
        {
            try
            {
                return Activator.CreateInstance<NguoiDung>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(NguoiDung)}'. " +
                    $"Ensure that '{nameof(NguoiDung)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<NguoiDung> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<NguoiDung>)_userStore;
        }
    }
}
