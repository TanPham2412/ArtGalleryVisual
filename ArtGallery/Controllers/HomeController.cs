using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ArtGallery.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using ArtGallery.Repositories.Interfaces;

namespace ArtGallery.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IHomeRepository _homeRepository;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IHomeRepository homeRepository, ILogger<HomeController> logger)
        {
            _homeRepository = homeRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var artworks = await _homeRepository.GetRandomArtworksFromFollowing(userId, 12);
            return View(artworks);
        }

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

                var currentUserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
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
            var currentUserId = int.Parse(User.FindFirst("UserId").Value);
            var users = await _homeRepository.SearchUsers(query, currentUserId);
            return Json(users);
        }

        [HttpPost]
        [Route("/Home/ToggleFollow")]
        public async Task<IActionResult> ToggleFollow([FromBody] int userId)
        {
            var currentUserId = int.Parse(User.FindFirst("UserId").Value);
            var result = await _homeRepository.ToggleFollow(currentUserId, userId);
            
            return Json(new { 
                success = result.success,
                message = result.message,
                followerCount = result.followerCount,
                isFollowing = result.isFollowing
            });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
