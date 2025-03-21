using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ArtGallery.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using ArtGallery.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace ArtGallery.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHomeRepository _homeRepository;
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<NguoiDung> _signInManager;
        private readonly UserManager<NguoiDung> _userManager;
        private readonly ArtGalleryContext _context;

        public HomeController(IHomeRepository homeRepository, ILogger<HomeController> logger, SignInManager<NguoiDung> signInManager, UserManager<NguoiDung> userManager, ArtGalleryContext context)
        {
            _homeRepository = homeRepository;
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Kiểm tra nếu người dùng đã đăng nhập
            if (!User.Identity.IsAuthenticated)
            {
                // Nếu chưa đăng nhập, chuyển hướng đến trang Login_register
                return RedirectToAction("LoginRegister");
            }
            
            // Nếu đã đăng nhập, hiển thị trang Index bình thường
            var currentUserId = User.FindFirst("UserId")?.Value ?? "";
            var model = await _homeRepository.GetRandomArtworksFromFollowing(currentUserId, 8);
            return View(model);
        }

        // Trang Login_register - không cần [Authorize]
        [AllowAnonymous]
        public IActionResult LoginRegister()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [Authorize] // Chỉ thêm Authorize ở các phương thức cần xác thực
        [HttpGet]
        public async Task<IActionResult> Add()
        {
            try 
            {
                ViewBag.Categories = await _homeRepository.GetCategories();
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải trang thêm tranh");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải trang";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([FromForm] Tranh tranh, IFormFile ImageFile, string TagsInput, List<int> SelectedCategories)
        {
            try
            {
                _logger.LogInformation("Bắt đầu thêm tranh mới: {@Tranh}", tranh);

                // Xóa validation errors cho DuongDanAnh và MaNguoiDungNavigation
                ModelState.Remove("DuongDanAnh");
                ModelState.Remove("MaNguoiDungNavigation");

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState không hợp lệ: {@ModelState}", 
                        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    ViewBag.Categories = await _homeRepository.GetCategories();
                    return View(tranh);
                }

                if (ImageFile == null || ImageFile.Length == 0)
                {
                    _logger.LogWarning("Không có file ảnh được tải lên");
                    ModelState.AddModelError("", "Vui lòng chọn file ảnh");
                    ViewBag.Categories = await _homeRepository.GetCategories();
                    return View(tranh);
                }

                var currentUserId = User.FindFirst("UserId")?.Value;
                _logger.LogInformation("Người dùng hiện tại: {UserId}", currentUserId);

                var result = await _homeRepository.AddArtwork(tranh, ImageFile, TagsInput, SelectedCategories, currentUserId);  

                if (result.success)
                {
                    _logger.LogInformation("Thêm tranh thành công");
                    TempData["SuccessMessage"] = result.message;
                    return RedirectToAction("Index");
                }
                else
                {
                    _logger.LogWarning("Thêm tranh thất bại: {Message}", result.message);
                    ModelState.AddModelError("", result.message);
                    ViewBag.Categories = await _homeRepository.GetCategories();
                    return View(tranh);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi không xác định khi thêm tranh");
                ModelState.AddModelError("", "Có lỗi xảy ra khi thêm tranh");
                ViewBag.Categories = await _homeRepository.GetCategories();
                return View(tranh);
            }
        }

        [HttpGet]
        public async Task<IActionResult> SearchUsers(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return Json(new List<object>());
            }

            try 
            {
                string currentUserId = null;
                if (User.Identity.IsAuthenticated)
                {
                    currentUserId = _userManager.GetUserId(User);
                }

                var users = await _context.Users
                    .Where(u => (u.TenNguoiDung != null && u.TenNguoiDung.Contains(query)) || 
                               (u.UserName != null && u.UserName.Contains(query)))
                    .Select(u => new {
                        maNguoiDung = u.Id,
                        tenNguoiDung = u.TenNguoiDung ?? u.UserName,
                        tenDangNhap = u.UserName,
                        anhDaiDien = u.AnhDaiDien,
                        daTheoDoi = currentUserId != null && _context.TheoDois
                            .Any(t => t.MaNguoiTheoDoi == currentUserId && 
                                     t.MaNguoiDuocTheoDoi == u.Id)
                    })
                    .Take(10)
                    .ToListAsync();

                _logger.LogInformation($"Tìm kiếm người dùng với từ khóa '{query}': Tìm thấy {users.Count} kết quả");
                return Json(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi tìm kiếm người dùng với từ khóa '{query}'");
                return Json(new { error = true, message = "Có lỗi xảy ra khi tìm kiếm" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFollow([FromBody] string userId)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return Json(new { success = false, message = "Bạn cần đăng nhập để thực hiện chức năng này" });
                }

                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser.Id == userId)
                {
                    return Json(new { success = false, message = "Bạn không thể theo dõi chính mình" });
                }

                var existingFollow = await _context.TheoDois
                    .FirstOrDefaultAsync(t => t.MaNguoiTheoDoi == currentUser.Id && t.MaNguoiDuocTheoDoi == userId);

                if (existingFollow != null)
                {
                    // Unfollow
                    _context.TheoDois.Remove(existingFollow);
                }
                else
                {
                    // Follow
                    _context.TheoDois.Add(new TheoDoi
                    {
                        MaNguoiTheoDoi = currentUser.Id,
                        MaNguoiDuocTheoDoi = userId,
                        NgayTheoDoi = DateTime.Now
                    });
                }

                await _context.SaveChangesAsync();

                var followerCount = await _context.TheoDois.CountAsync(t => t.MaNguoiDuocTheoDoi == userId);

                return Json(new { 
                    success = true, 
                    message = existingFollow != null ? "Đã hủy theo dõi" : "Đã theo dõi",
                    followerCount = followerCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thực hiện theo dõi/hủy theo dõi");
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout([FromServices] SignInManager<NguoiDung> signInManager)
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login_register", "Account");
        }

        public async Task<IActionResult> LogoutDirect([FromServices] SignInManager<NguoiDung> signInManager)
        {
            await signInManager.SignOutAsync();
            
            // Chuyển hướng trực tiếp đến trang chủ sau khi đăng xuất
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous] // Trang lỗi không cần xác thực
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
