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
        public async Task<IActionResult> Admin(string sortOrder, string searchString)
        {
            try
            {
                // Lấy tất cả tranh
                var artworks = await _artworkRepository.GetAllArtworks();
                
                // Tìm kiếm nếu có chuỗi tìm kiếm
                if (!string.IsNullOrEmpty(searchString))
                {
                    searchString = searchString.ToLower();
                    artworks = artworks.Where(a => 
                        a.TieuDe.ToLower().Contains(searchString) ||
                        a.MaNguoiDungNavigation.TenNguoiDung.ToLower().Contains(searchString) ||
                        a.Gia.ToString().Contains(searchString) ||
                        a.SoLuongTon.ToString().Contains(searchString)
                    ).ToList();
                }
                
                // Mặc định sắp xếp theo ID giảm dần
                if (string.IsNullOrEmpty(sortOrder))
                {
                    sortOrder = "id_desc"; 
                }
                
                // Sắp xếp danh sách artworks theo sortOrder
                switch (sortOrder)
                {
                    case "id_asc":
                        artworks = artworks.OrderBy(a => a.MaTranh).ToList();
                        break;
                    case "id_desc":
                        artworks = artworks.OrderByDescending(a => a.MaTranh).ToList();
                        break;
                    case "title_asc":
                        artworks = artworks.OrderBy(a => a.TieuDe).ToList();
                        break;
                    case "title_desc":
                        artworks = artworks.OrderByDescending(a => a.TieuDe).ToList();
                        break;
                    case "artist_asc":
                        artworks = artworks.OrderBy(a => a.MaNguoiDungNavigation.TenNguoiDung).ToList();
                        break;
                    case "artist_desc":
                        artworks = artworks.OrderByDescending(a => a.MaNguoiDungNavigation.TenNguoiDung).ToList();
                        break;
                    case "price_asc":
                        artworks = artworks.OrderBy(a => a.Gia).ToList();
                        break;
                    case "price_desc":
                        artworks = artworks.OrderByDescending(a => a.Gia).ToList();
                        break;
                    case "quantity_asc":
                        artworks = artworks.OrderBy(a => a.SoLuongTon).ToList();
                        break;
                    case "quantity_desc":
                        artworks = artworks.OrderByDescending(a => a.SoLuongTon).ToList();
                        break;
                    case "date_asc":
                        artworks = artworks.OrderBy(a => a.NgayDang).ToList();
                        break;
                    case "date_desc":
                        artworks = artworks.OrderByDescending(a => a.NgayDang).ToList();
                        break;
                    default:
                        artworks = artworks.OrderByDescending(a => a.MaTranh).ToList();
                        break;
                }
                
                return View(artworks);
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
                var existingLike = await _context.LuotThiches
                    .FirstOrDefaultAsync(lt => lt.MaTranh == artworkId && lt.MaNguoiDung == currentUserId);

                if (existingLike != null)
                {
                    // Nếu đã thích, xóa lượt thích
                    _context.LuotThiches.Remove(existingLike);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, liked = false });
                }
                else
                {
                    // Nếu chưa thích, thêm lượt thích mới
                    var luotThich = new LuotThich
                    {
                        MaTranh = artworkId,
                        MaNguoiDung = currentUserId,
                        NgayThich = DateTime.Now
                    };
                    _context.LuotThiches.Add(luotThich);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, liked = true });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thay đổi trạng thái yêu thích");
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }
    }
}