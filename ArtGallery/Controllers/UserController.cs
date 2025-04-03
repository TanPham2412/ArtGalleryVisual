using Microsoft.AspNetCore.Mvc;
using ArtGallery.Repositories.Interfaces;
using ArtGallery.Models;
using ArtGallery.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace ArtGallery.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserController> _logger;
        private readonly UserManager<NguoiDung> _userManager;
        private readonly ArtGalleryContext _context;

        public UserController(
            IUserRepository userRepository, 
            ILogger<UserController> logger,
            UserManager<NguoiDung> userManager,
            ArtGalleryContext context)
        {
            _userRepository = userRepository;
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Profile(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    var currentUser = await _userManager.GetUserAsync(User);
                    if (currentUser != null)
                    {
                        id = currentUser.Id;
                    }
                    else
                    {
                        return RedirectToAction("Login", "Account");
                    }
                }

                var result = await _userRepository.GetUserProfile(id);

                if (result == null)
                {
                    return NotFound("Không tìm thấy người dùng");
                }

                var (user, followersCount, followingCount) = result.Value;

                ViewBag.FollowersCount = followersCount;
                ViewBag.FollowingCount = followingCount;
                
                ViewBag.IsOwnProfile = User.Identity.IsAuthenticated && 
                                      (await _userManager.GetUserAsync(User))?.Id == id;

                if (User.Identity.IsAuthenticated)
                {
                    var currentUserId = _userManager.GetUserId(User);
                    ViewBag.CurrentUserId = currentUserId;
                    ViewBag.IsFollowing = await _context.TheoDois
                        .AnyAsync(t => t.MaNguoiTheoDoi == currentUserId && 
                                       t.MaNguoiDuocTheoDoi == id);
                }

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
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromForm] NguoiDung model, IFormFile profileImage, IFormFile coverImage, List<string> LoaiMedia, List<string> DuongDan)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "Bạn cần đăng nhập để thực hiện thao tác này" });
                }

                if (currentUser.Id != model.Id)
                {
                    return Json(new { success = false, message = "Bạn không có quyền chỉnh sửa hồ sơ này" });
                }

                model.Id = currentUser.Id;
                
                if (coverImage != null && coverImage.Length > 0)
                {
                    _logger.LogInformation($"Đã nhận được file ảnh bìa: {coverImage.FileName}, Size: {coverImage.Length} bytes");
                }

                var result = await _userRepository.UpdateProfile(model, profileImage, coverImage, LoaiMedia, DuongDan);

                return Json(new
                {
                    success = result.success,
                    message = result.message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật profile người dùng {UserId}", model.Id);
                return Json(new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi cập nhật profile"
                });
            }
        }

        public async Task<IActionResult> Gallery(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    var currentUser = await _userManager.GetUserAsync(User);
                    if (currentUser != null)
                    {
                        id = currentUser.Id;
                    }
                    else
                    {
                        return RedirectToAction("Login", "Account");
                    }
                }

                var result = await _userRepository.GetUserProfile(id);

                if (result == null)
                {
                    return NotFound("Không tìm thấy người dùng");
                }

                var (user, followersCount, followingCount) = result.Value;

                ViewBag.FollowersCount = followersCount;
                ViewBag.FollowingCount = followingCount;
                ViewBag.IsOwnProfile = User.Identity.IsAuthenticated && 
                                     (await _userManager.GetUserAsync(User))?.Id == id;

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi hiển thị tác phẩm của người dùng {UserId}", id);
                return RedirectToAction("Error", "Home");
            }
        }
    }
}