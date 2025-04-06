using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ArtGallery.Models;
using System.Security.Claims;

namespace ArtGallery.Controllers
{
    public class NotificationController : Controller
    {
        private readonly ArtGalleryContext _context;
        private readonly UserManager<NguoiDung> _userManager;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(
            ArtGalleryContext context,
            UserManager<NguoiDung> userManager,
            ILogger<NotificationController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
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
    }
} 