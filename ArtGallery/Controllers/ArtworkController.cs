using Microsoft.AspNetCore.Mvc;
using ArtGallery.Models;
using ArtGallery.Repositories.Interfaces;

namespace ArtGallery.Controllers
{
    public class ArtworkController : Controller
    {
        private readonly IArtworkRepository _artworkRepository;
        private readonly ILogger<ArtworkController> _logger;

        public ArtworkController(IArtworkRepository artworkRepository, ILogger<ArtworkController> logger)
        {
            _artworkRepository = artworkRepository;
            _logger = logger;
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
            var currentUserId = User.FindFirst("UserId").Value;
            var artwork = await _artworkRepository.GetArtworkForEdit(id, currentUserId);
            
            if (artwork == null)
                return NotFound();

            ViewBag.Categories = await _artworkRepository.GetCategories();
            return View(artwork);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Tranh model, IFormFile? ImageFile, string TagsInput, List<int> SelectedCategories)
        {
            try 
            {
                ModelState.Remove("MaNguoiDungNavigation");
                
                if (!ModelState.IsValid)
                {
                    ViewBag.Categories = await _artworkRepository.GetCategories();
                    TempData["ErrorMessage"] = "Dữ liệu không hợp lệ";
                    return View(model);
                }

                var currentUserId = User.FindFirst("UserId")?.Value;

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

                var result = await _artworkRepository.UpdateArtwork(model, ImageFile, TagsInput, SelectedCategories, currentUserId);

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
            var currentUserId = User.FindFirst("UserId").Value;
            var result = await _artworkRepository.DeleteArtwork(id, currentUserId);

            return Json(new { 
                success = result.success, 
                message = result.message,
                redirectUrl = result.redirectUrl 
            });
        }
    }
}
