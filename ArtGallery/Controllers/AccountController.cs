using Microsoft.AspNetCore.Mvc;
using ArtGallery.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using ArtGallery.Models.Account;
using Microsoft.AspNetCore.Authentication.Google;

namespace ArtGallery.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAccountRepository accountRepository, ILogger<AccountController> logger)
        {
            _accountRepository = accountRepository;
            _logger = logger;
        }

        [HttpGet]
        [HttpGet]
        public IActionResult Login1()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login1(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _accountRepository.ValidateLogin1(model);
                if (result.success)
                {
                    var claims = await _accountRepository.GenerateUserClaims(result.user);
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity));

                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", result.message);
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _accountRepository.RegisterUser(model);
                if (result.success)
                {
                    TempData["SuccessMessage"] = result.message;
                    return RedirectToAction("Login");
                }
                ModelState.AddModelError("", result.message);
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _accountRepository.ValidateLogin(model);
                if (result.success)
                {
                    var claims = await _accountRepository.GenerateUserClaims(result.user);
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.GhiNhoDangNhap
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    TempData["SuccessMessage"] = "Đăng nhập thành công!";
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", result.message);
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["SuccessMessage"] = "Đăng xuất thành công!";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties 
            { 
                RedirectUri = Url.Action("GoogleResponse", "Account"),
                Items =
                {
                    { "LoginProvider", "Google" },
                    { "scheme", "Google" }
                }
            };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            if (!result.Succeeded)
                return RedirectToAction("Login");

            var claims = result.Principal.Identities.FirstOrDefault().Claims;
            
            // Lấy thông tin từ Google
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value; // Lấy tên từ Google
            
            // Kiểm tra xem email đã tồn tại trong hệ thống chưa
            var user = await _accountRepository.FindUserByEmail(email);
            
            if (user == null)
            {
                // Tạo tài khoản mới nếu chưa tồn tại
                var newUser = new NguoiDung
                {
                    Email = email,
                    TenDangNhap = email.Split('@')[0], // Username vẫn lấy từ email
                    TenNguoiDung = name ?? email.Split('@')[0], // Sử dụng tên từ Google, nếu không có thì dùng phần đầu của email
                    NgayTao = DateTime.Now
                };
                
                user = await _accountRepository.CreateGoogleUser(newUser);
            }

            // Tạo claims cho user
            var userClaims = await _accountRepository.GenerateUserClaims(user);
            
            // Đăng nhập user
            var identity = new ClaimsIdentity(userClaims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Home");
        }
    }
}