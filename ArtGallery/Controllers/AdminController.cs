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
            return View("AdminHomePage");
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

        [HttpGet]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                // Đếm tổng số người dùng
                var totalUsers = await _context.NguoiDungs.CountAsync();
                
                // Đếm tổng số nghệ sĩ
                var totalArtists = await _userManager.GetUsersInRoleAsync("Artists");
                
                // Đếm tổng số tác phẩm
                var totalArtworks = await _context.Tranhs.CountAsync();
                
                // Đếm tổng số đơn hàng
                var totalOrders = await _context.GiaoDiches.CountAsync();
                
                // Lấy danh sách đơn đăng ký nghệ sĩ mới nhất
                var recentArtistApplications = await _context.NguoiDungs
                    .Where(n => n.DangKyNgheSi)
                    .OrderByDescending(n => n.NgayDangKy)
                    .Take(5)
                    .Select(n => new
                    {
                        userId = n.Id,
                        artistName = n.TenNguoiDung,
                        applicationDate = n.NgayDangKy
                    })
                    .ToListAsync();
                
                // Lấy danh sách tác phẩm mới nhất
                var recentArtworks = await _context.Tranhs
                    .OrderByDescending(t => t.NgayDang)
                    .Take(5)
                    .Select(t => new
                    {
                        id = t.MaTranh,
                        title = t.TieuDe,
                        thumbnailUrl = t.DuongDanAnh,
                        artistName = t.MaNguoiDungNavigation.TenNguoiDung,
                        createdDate = t.NgayDang
                    })
                    .ToListAsync();
                
                return Json(new
                {
                    totalUsers,
                    totalArtists = totalArtists.Count,
                    totalArtworks,
                    totalOrders,
                    recentArtistApplications,
                    recentArtworks
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard stats");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _context.NguoiDungs
                    .Select(u => new
                    {
                        id = u.Id,
                        userName = u.TenNguoiDung,
                        email = u.Email,
                        avatarPath = u.GetAvatarPath(),
                        isAdmin = _context.UserRoles.Any(ur => ur.UserId == u.Id && _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Admin")),
                        isArtist = _context.UserRoles.Any(ur => ur.UserId == u.Id && _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Artists")),
                        isActive = !u.LockoutEnabled || (u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.Now)
                    })
                    .ToListAsync();
                
                return Json(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetArtistApplications()
        {
            try
            {
                var applications = await _context.NguoiDungs
                    .Where(n => n.DangKyNgheSi)
                    .OrderByDescending(n => n.NgayDangKy)
                    .Select(n => new
                    {
                        userId = n.Id,
                        artistName = n.TenNguoiDung,
                        email = n.Email,
                        avatarPath = n.GetAvatarPath(),
                        applicationDate = n.NgayDangKy
                    })
                    .ToListAsync();
                
                return Json(applications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting artist applications");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetArtworks()
        {
            try
            {
                var artworks = await _context.Tranhs
                    .OrderByDescending(t => t.NgayDang)
                    .Select(t => new
                    {
                        id = t.MaTranh,
                        title = t.TieuDe,
                        thumbnailUrl = t.DuongDanAnh,
                        artistName = t.MaNguoiDungNavigation.TenNguoiDung,
                        category = t.MaTheLoais.FirstOrDefault() != null ? t.MaTheLoais.FirstOrDefault().TenTheLoai : "Chưa phân loại",
                        price = t.Gia,
                        createdDate = t.NgayDang,
                        isActive = t.TrangThai != "Đã ẩn" // Hoặc sử dụng trạng thái phù hợp từ t.TrangThai
                    })
                    .ToListAsync();

                return Json(artworks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting artworks");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            try
            {
                var orders = await _context.GiaoDiches
                    .OrderByDescending(g => g.NgayMua)
                    .Select(g => new
                    {
                        orderId = g.MaGiaoDich,
                        customerName = g.MaNguoiMuaNavigation.TenNguoiDung,
                        artworkTitle = g.MaTranhNavigation.TieuDe,
                        artworkThumbnail = g.MaTranhNavigation.DuongDanAnh,
                        totalAmount = g.SoTien,
                        orderDate = g.NgayMua,
                        status = g.TrangThai
                    })
                    .ToListAsync();
                
                return Json(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> LockUser([FromBody] LockUserViewModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.UserId))
                    return BadRequest(new { success = false, message = "ID người dùng không hợp lệ" });

                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user == null)
                    return NotFound(new { success = false, message = "Không tìm thấy người dùng" });

                // Tính toán thời gian khóa
                var lockoutEnd = DateTimeOffset.UtcNow.AddDays(model.Days)
                                                    .AddHours(model.Hours)
                                                    .AddMinutes(model.Minutes)
                                                    .AddSeconds(model.Seconds);

                // Lưu lý do khóa
                user.LockoutReason = model.Reason;
                
                // Kích hoạt khóa tài khoản
                await _userManager.SetLockoutEnabledAsync(user, true);
                await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);
                
                // Cập nhật thông tin người dùng
                await _userManager.UpdateAsync(user);
                
                // Gửi thông báo cho người dùng
                await _notificationRepository.CreateSystemNotification(
                    model.UserId,
                    "Tài khoản của bạn đã bị khóa",
                    $"Tài khoản của bạn đã bị khóa đến {lockoutEnd.LocalDateTime:dd/MM/yyyy HH:mm:ss} với lý do: {model.Reason}",
                    "/User/Profile/" + model.UserId,
                    "system"
                );
                
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error locking user");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UnlockUser([FromBody] string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                    return BadRequest(new { success = false, message = "ID người dùng không hợp lệ" });

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return NotFound(new { success = false, message = "Không tìm thấy người dùng" });

                // Xóa lý do khóa
                user.LockoutReason = null;
                
                // Mở khóa tài khoản
                await _userManager.SetLockoutEndDateAsync(user, null);
                
                // Cập nhật thông tin người dùng
                await _userManager.UpdateAsync(user);
                
                // Gửi thông báo cho người dùng
                await _notificationRepository.CreateSystemNotification(
                    userId,
                    "Tài khoản của bạn đã được mở khóa",
                    "Tài khoản của bạn đã được mở khóa và có thể sử dụng bình thường.",
                    "/User/Profile/" + userId,
                    "system"
                );
                
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlocking user");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}