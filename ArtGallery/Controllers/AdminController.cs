using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using ArtGallery.Models;
using Microsoft.AspNetCore.Authorization;
using ArtGallery.Repositories.Interfaces;
using ArtGallery.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ArtGallery.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<NguoiDung> _userManager;
        private readonly ArtGalleryContext _context;
        private readonly INotificationRepository _notificationRepository;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            UserManager<NguoiDung> userManager,
            ArtGalleryContext context,
            INotificationRepository notificationRepository,
            ILogger<AdminController> logger)
        {
            _userManager = userManager;
            _context = context;
            _notificationRepository = notificationRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _context.NguoiDungs.ToListAsync();
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> ArtistApproval(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null || !user.DangKyNgheSi)
                return NotFound();

            var viewModel = new ArtistApprovalViewModel
            {
                UserId = user.Id,
                ArtistName = user.TenNguoiDung,
                Address = user.DiaChi,
                Description = user.MoTa,
                PhoneNumber = user.PhoneNumber,
                AvatarPath = user.GetAvatarPath()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveArtist(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            // Thêm người dùng vào vai trò Artists
            await _userManager.AddToRoleAsync(user, "Artists");
            
            // Cập nhật trạng thái đăng ký
            user.DangKyNgheSi = false;
            await _userManager.UpdateAsync(user);
            
            // Gửi thông báo cho người dùng
            await _notificationRepository.CreateSystemNotification(
                userId,
                "Đăng ký nghệ sĩ được chấp nhận",
                "Chúc mừng! Bạn đã được chấp nhận trở thành nghệ sĩ trên PiaoYue.",
                "/User/Gallery/" + userId,
                "system"
            );
            
            TempData["SuccessMessage"] = "Đã phê duyệt đăng ký nghệ sĩ thành công!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectArtist(string userId, string reason)
        {
            // Thêm log để kiểm tra dữ liệu đầu vào
            _logger.LogInformation($"Reject Artist - UserId: {userId}, Reason: {reason}");

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("UserId is null or empty");
                return NotFound();
            }
        
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User with ID {userId} not found");
                return NotFound();
            }
        
            try
            {
                // Cập nhật trạng thái đăng ký
                user.DangKyNgheSi = false;
                await _userManager.UpdateAsync(user);
            
                // Gửi thông báo từ chối
                string reasonText = string.IsNullOrEmpty(reason) 
                    ? "Đăng ký của bạn không đáp ứng đủ yêu cầu của chúng tôi."
                    : reason;
            
                await _notificationRepository.CreateSystemNotification(
                    userId,
                    "Đăng ký nghệ sĩ bị từ chối",
                    $"Rất tiếc, đăng ký nghệ sĩ của bạn đã bị từ chối với lý do: {reasonText}",
                    "/User/Profile/" + userId,
                    "system"
                );
            
                TempData["SuccessMessage"] = "Đã từ chối đăng ký nghệ sĩ";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting artist application");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi từ chối đăng ký: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }
    }
}