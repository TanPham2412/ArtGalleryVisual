using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ArtGallery.Models;
using System.Security.Claims;
using ArtGallery.Repositories.Interfaces;

namespace ArtGallery.Controllers
{
    public class NotificationController : Controller
    {
        private readonly ArtGalleryContext _context;
        private readonly UserManager<NguoiDung> _userManager;
        private readonly ILogger<NotificationController> _logger;
        private readonly INotificationRepository _notificationRepository;

        public NotificationController(
            ArtGalleryContext context,
            UserManager<NguoiDung> userManager,
            ILogger<NotificationController> logger,
            INotificationRepository notificationRepository)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _notificationRepository = notificationRepository;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return Json(new { success = false, message = "Bạn cần đăng nhập để thực hiện chức năng này" });
                }

                var userId = _userManager.GetUserId(User);
                var thongBao = await _context.ThongBaos
                    .FirstOrDefaultAsync(t => t.MaThongBao == id && t.MaNguoiNhan == userId);

                if (thongBao == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông báo" });
                }

                thongBao.DaDoc = true;
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đánh dấu thông báo đã đọc");
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return Json(new { success = false, message = "Bạn cần đăng nhập để thực hiện chức năng này" });
                }

                var userId = _userManager.GetUserId(User);
                var thongBaoList = await _context.ThongBaos
                    .Where(t => t.MaNguoiNhan == userId && t.DaDoc != true)
                    .ToListAsync();

                foreach (var thongBao in thongBaoList)
                {
                    thongBao.DaDoc = true;
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đánh dấu tất cả thông báo đã đọc");
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsViewed()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Ok();
            }

            // Chỉ cập nhật view state trong session hoặc cookie, không thay đổi database
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetNotificationCount()
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return Json(new { count = 0 });
                }

                var userId = _userManager.GetUserId(User);
                var count = await _context.ThongBaos
                    .CountAsync(t => t.MaNguoiNhan == userId && t.DaDoc != true);

                return Json(new { count = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy số lượng thông báo");
                return Json(new { count = 0 });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveArtist(string userId)
        {
            if (!User.IsInRole("Admin"))
                return Forbid();
        
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
        
            return RedirectToAction("Index", "Admin");
        }
    }
} 