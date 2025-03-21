using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

public class AccountController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(SignInManager<IdentityUser> signInManager, ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    // Đảm bảo tên phương thức khớp với tên file (có dấu gạch dưới)
    public IActionResult Login_register()
    {
        return View();  // Tự động tìm Views/Account/Login_register.cshtml
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        // Đăng xuất người dùng hiện tại
        await _signInManager.SignOutAsync();
        
        // Thêm thông báo thành công nếu cần
        _logger.LogInformation("Người dùng đã đăng xuất thành công.");
        
        // Chuyển hướng đến trang Login_register sau khi đăng xuất
        return RedirectToAction("Login_register", "Account");
    }
} 