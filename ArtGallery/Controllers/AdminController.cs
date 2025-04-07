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
            if (user == null)
                return NotFound();
            
            // Kiểm tra nếu đơn đã được xử lý
            if (!user.DangKyNgheSi)
            {
                // Kiểm tra xem người này đã là nghệ sĩ chưa
                bool isArtist = await _userManager.IsInRoleAsync(user, "Artists");
                
                // Chuyển đến view đã xử lý
                return View("ArtistApprovalProcessed", new ArtistApprovalProcessedViewModel
                {
                    UserId = user.Id,
                    ArtistName = user.TenNguoiDung,
                    AvatarPath = user.GetAvatarPath(),
                    IsApproved = isArtist
                });
            }

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
            _logger.LogInformation($"Approve Artist - UserId: {userId}");

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
                // Kiểm tra xem role Artists có tồn tại chưa
                var roleExists = await _context.Roles.AnyAsync(r => r.Name == "Artists");
                if (!roleExists)
                {
                    // Nếu chưa có role, tạo mới
                    _logger.LogInformation("Creating Artists role as it doesn't exist");
                    await _context.Roles.AddAsync(new Microsoft.AspNetCore.Identity.IdentityRole { Name = "Artists", NormalizedName = "ARTISTS" });
                    await _context.SaveChangesAsync();
                }

                // Thêm người dùng vào vai trò Artists
                var result = await _userManager.AddToRoleAsync(user, "Artists");
                
                if (!result.Succeeded)
                {
                    string errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError($"Failed to add user to Artists role: {errors}");
                    
                    // Thử tìm lỗi cụ thể
                    var userRoles = await _userManager.GetRolesAsync(user);
                    _logger.LogInformation($"Current user roles: {string.Join(", ", userRoles)}");
                    
                    TempData["ErrorMessage"] = "Không thể thêm người dùng vào vai trò nghệ sĩ: " + errors;
                    return RedirectToAction("Index", "Home");
                }
                
                // Cập nhật trạng thái đăng ký
                user.DangKyNgheSi = false;
                var updateResult = await _userManager.UpdateAsync(user);
                
                if (!updateResult.Succeeded)
                {
                    string errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                    _logger.LogError($"Failed to update user: {errors}");
                }
                
                // Gửi thông báo cho người dùng
                await _notificationRepository.CreateSystemNotification(
                    userId,
                    "Đăng ký nghệ sĩ được chấp nhận",
                    "Chúc mừng! Bạn đã được chấp nhận trở thành nghệ sĩ trên PiaoYue.",
                    "/User/Gallery/" + userId,
                    "system"
                );
                
                TempData["SuccessMessage"] = "Đã phê duyệt đăng ký nghệ sĩ thành công!";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving artist application");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi chấp nhận đăng ký: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
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