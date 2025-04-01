using Microsoft.AspNetCore.Mvc;
using ArtGallery.Models;
using ArtGallery.Repositories.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ArtGallery.Controllers
{
    public class ArtworkController : Controller
    {
        private readonly IArtworkRepository _artworkRepository;
        private readonly ILogger<ArtworkController> _logger;
        private readonly ArtGalleryContext _context;

        public ArtworkController(IArtworkRepository artworkRepository, ILogger<ArtworkController> logger, ArtGalleryContext context)
        {
            _artworkRepository = artworkRepository;
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Display(int id)
        {
            var result = await _artworkRepository.GetArtworkForDisplay(id);
            if (result.artwork == null)
                return NotFound();

            ViewBag.OtherWorks = result.otherWorks;
            return View(result.artwork);
        }

        public async Task<IActionResult> ByCategory(int id)
        {
            var result = await _artworkRepository.GetArtworksByCategory(id);
            if (result.category == null)
                return NotFound();

            ViewBag.CategoryName = result.category.TenTheLoai;
            return View(result.artworks);
        }

        public async Task<IActionResult> ByTag(int id)
        {
            var result = await _artworkRepository.GetArtworksByTag(id);
            if (result.tag == null)
                return NotFound();

            ViewBag.TagName = result.tag.TenTag;
            return View(result.artworks);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            
            var artwork = await _artworkRepository.GetArtworkForEdit(id, currentUserId, isAdmin);
            
            if (artwork == null)
                return NotFound();

            ViewBag.Categories = await _artworkRepository.GetCategories();
            return View(artwork);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Tranh model, IFormFile? ImageFile, string TagsInput, List<int> SelectedCategories)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            
            _logger.LogInformation($"Current User ID: {currentUserId}, IsAdmin: {isAdmin}");

            try 
            {
                ModelState.Remove("MaNguoiDungNavigation");
                ModelState.Remove("TagsInput");

                if (!ModelState.IsValid)
                {
                    ViewBag.Categories = await _artworkRepository.GetCategories();
                    TempData["ErrorMessage"] = "Dữ liệu không hợp lệ";
                    return View(model);
                }

                if (model.Gia < 0)
                {
                    ViewBag.Categories = await _artworkRepository.GetCategories();
                    TempData["ErrorMessage"] = "Giá không thể là số âm";
                    return View(model);
                }
                
                if (model.SoLuongTon < 0)
                {
                    ViewBag.Categories = await _artworkRepository.GetCategories();
                    TempData["ErrorMessage"] = "Số lượng không thể là số âm";
                    return View(model);
                }

                var result = await _artworkRepository.UpdateArtwork(model, ImageFile, TagsInput, SelectedCategories, currentUserId, isAdmin);

                if (result.success)
                {
                    TempData["SuccessMessage"] = "Cập nhật thành công!";
                    return RedirectToAction("Display", new { id = model.MaTranh });
                }
                else
                {
                    ViewBag.Categories = await _artworkRepository.GetCategories();
                    TempData["ErrorMessage"] = result.message;
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật tranh");
                ViewBag.Categories = await _artworkRepository.GetCategories();
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật tranh";
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            
            // In thông tin để gỡ lỗi
            _logger.LogInformation($"Delete request: ArtworkID={id}, UserID={currentUserId}, IsAdmin={isAdmin}");
            
            var result = await _artworkRepository.DeleteArtwork(id, currentUserId);

            return Json(new { 
                success = result.success, 
                message = result.message,
                redirectUrl = result.redirectUrl 
            });
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Admin(string sortOrder, string searchString, string categoryFilter)
        {
            try
            {
                // Nếu có categoryFilter thì ưu tiên lọc theo thể loại
                if (!string.IsNullOrEmpty(categoryFilter))
                {
                    var artworks = await _context.Tranhs
                        .Include(t => t.MaNguoiDungNavigation)
                        .Include(t => t.MaTheLoais)
                        .Include(t => t.MaTags)
                        .Where(t => t.MaTheLoais.Any(c => c.TenTheLoai.Contains(categoryFilter)))
                        .ToListAsync();
                        
                    if (sortOrder != null)
                    {
                        // Áp dụng sắp xếp nếu có
                        artworks = _artworkRepository.ApplySorting(artworks, sortOrder);
                    }
                        
                    ViewBag.CategoryFilter = categoryFilter;
                    return View(artworks);
                }
                
                var filteredArtworks = await _artworkRepository.GetFilteredArtworks(searchString, sortOrder);
                return View(filteredArtworks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách tranh cho Admin");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách tranh";
                return RedirectToAction("Index", "Home");
            }
        }

        // Xử lý khi người dùng không có quyền truy cập
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLike(int artworkId)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để thích tác phẩm" });
                }

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _artworkRepository.ToggleLike(artworkId, currentUserId);

                return Json(new { 
                    success = result.success, 
                    liked = result.liked,
                    message = result.message 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thay đổi trạng thái yêu thích");
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> FilterByCategory(string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName))
            {
                return RedirectToAction("Products");
            }
            
            try
            {
                var artworks = await _context.Tranhs
                    .Include(t => t.MaNguoiDungNavigation)
                    .Include(t => t.MaTheLoais)
                    .Include(t => t.MaTags)
                    .Where(t => t.MaTheLoais.Any(c => c.TenTheLoai.Contains(categoryName)))
                    .ToListAsync();
                    
                ViewBag.CategoryName = categoryName;
                return View("Products", artworks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lọc tranh theo thể loại '{categoryName}'");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi lọc tranh";
                return RedirectToAction("Products");
            }
        }

        public async Task<IActionResult> Products(string sortOrder, string searchString, string categoryFilter)
        {
            try
            {
                // Nếu có categoryFilter thì ưu tiên lọc theo thể loại
                if (!string.IsNullOrEmpty(categoryFilter))
                {
                    return await FilterByCategory(categoryFilter);
                }
                
                var artworks = await _artworkRepository.GetFilteredArtworks(searchString, sortOrder);
                return View(artworks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách tranh");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách tranh";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}