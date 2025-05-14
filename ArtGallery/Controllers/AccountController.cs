using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ArtGallery.Models;
using ArtGallery.Areas.Identity.Pages.Account;

namespace ArtGallery.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<NguoiDung> _userManager;
        private readonly SignInManager<NguoiDung> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<NguoiDung> userManager,
            SignInManager<NguoiDung> signInManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public IActionResult Login_register()
        {
            return View(); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            
            _logger.LogInformation("Người dùng đã đăng xuất thành công.");
            
            return RedirectToAction("Login_register", "Account");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel.InputModel model)
        {
            if (ModelState.IsValid)
            {
                // Đầu tiên kiểm tra mật khẩu có đúng không
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                
                if (result.Succeeded || result.IsLockedOut)
                {
                    // Nếu mật khẩu đúng, kiểm tra xem tài khoản có bị khóa không
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    
                    if (user != null && await _userManager.IsLockedOutAsync(user))
                    {
                        // Tài khoản bị khóa
                        var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                        
                        if (lockoutEnd.HasValue && lockoutEnd.Value > DateTimeOffset.Now)
                        {
                            // Lưu thông tin khóa để hiển thị
                            TempData["LockoutMessage"] = "Tài khoản của bạn đang bị khóa";
                            TempData["LockoutReason"] = user.LockoutReason ?? "Vi phạm quy định của trang web";
                            TempData["LockoutTime"] = (lockoutEnd.Value - DateTimeOffset.Now).ToString();
                            TempData["LockoutEndTime"] = lockoutEnd.Value.LocalDateTime.ToString("yyyy-MM-ddTHH:mm:ss");
                            
                            // Đăng nhập người dùng dù họ bị khóa
                            await _signInManager.SignInAsync(user, isPersistent: model.RememberMe);
                            
                            // Chuyển hướng đến trang Index
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("Người dùng đăng nhập thành công.");
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Đăng nhập không thành công. Vui lòng kiểm tra email và mật khẩu.");
                    return View("Login_register", model);
                }
            }
            
            // Nếu có lỗi, hiển thị lại form
            return View("Login_register", model);
        }
    }
} 