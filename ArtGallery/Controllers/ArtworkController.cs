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
            var currentUserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
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
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _artworkRepository.GetCategories();
                return View(model);
            }

            var currentUserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var result = await _artworkRepository.UpdateArtwork(model, ImageFile, TagsInput, SelectedCategories, currentUserId);

            if (result.success)
            {
                TempData["SuccessMessage"] = result.message;
                return RedirectToAction(nameof(Display), new { id = model.MaTranh });
            }

            TempData["ErrorMessage"] = result.message;
            ViewBag.Categories = await _artworkRepository.GetCategories();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var result = await _artworkRepository.DeleteArtwork(id, currentUserId);

            return Json(new { 
                success = result.success, 
                message = result.message,
                redirectUrl = result.redirectUrl 
            });
        }
    }
}
