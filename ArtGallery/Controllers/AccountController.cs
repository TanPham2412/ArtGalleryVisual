using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ArtGallery.Models;

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
    }
} 