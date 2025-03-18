using Microsoft.AspNetCore.Mvc;
using ArtGallery.Repositories.Interfaces;
using ArtGallery.Models;
using ArtGallery.Repositories;

namespace ArtGallery.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserRepository userRepository, ILogger<UserController> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Profile(int id)
        {
            try
            {
                var result = await _userRepository.GetUserProfile(id);

                if (result == null)
                {
                    return NotFound();
                }

                var (user, followersCount, followingCount) = result.Value;

                ViewBag.FollowersCount = followersCount;
                ViewBag.FollowingCount = followingCount;

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi hiển thị profile người dùng {UserId}", id);
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile([FromForm] NguoiDung model, IFormFile profileImage, IFormFile coverImage, List<string> LoaiMedia, List<string> DuongDan)
        {
            try
            {
                if (coverImage != null && coverImage.Length > 0)
                {
                    _logger.LogInformation($"Đã nhận được file ảnh bìa: {coverImage.FileName}, Size: {coverImage.Length} bytes");
                }

                var result = await _userRepository.UpdateProfile(model, profileImage, coverImage, LoaiMedia, DuongDan);
                
                return Json(new { 
                    success = result.success, 
                    message = result.message 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật profile người dùng {UserId}", model.MaNguoiDung);
                return Json(new { 
                    success = false, 
                    message = "Có lỗi xảy ra khi cập nhật profile" 
                });
            }
        }
    }
}