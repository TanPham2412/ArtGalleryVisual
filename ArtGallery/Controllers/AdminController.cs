using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using ArtGallery.Models;
using Microsoft.AspNetCore.Authorization;
using ArtGallery.Repositories.Interfaces;
using ArtGallery.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.IO;

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

        [HttpPost]
        public async Task<IActionResult> DeleteUser([FromBody] DeleteUserViewModel model)
        {
            try
            {
                if (model == null || string.IsNullOrEmpty(model.UserId))
                    return BadRequest(new { success = false, message = "ID người dùng không hợp lệ" });

                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user == null)
                    return NotFound(new { success = false, message = "Không tìm thấy người dùng" });

                if (User.FindFirstValue(ClaimTypes.NameIdentifier) == model.UserId)
                    return BadRequest(new { success = false, message = "Không thể tự xóa tài khoản của bạn" });

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 1. Xóa dữ liệu phản hồi bình luận
                    var userCommentIds = await _context.BinhLuans
                        .Where(b => b.MaNguoiDung == model.UserId)
                        .Select(b => b.MaBinhLuan)
                        .ToListAsync();
                    
                    var replies = await _context.PhanHoiBinhLuans
                        .Where(r => userCommentIds.Contains(r.MaBinhLuan) || r.MaNguoiDung == model.UserId)
                        .ToListAsync();
                    _context.PhanHoiBinhLuans.RemoveRange(replies);
                    
                    // 2. Xóa bình luận
                    var comments = await _context.BinhLuans
                        .Where(b => b.MaNguoiDung == model.UserId)
                        .ToListAsync();
                    _context.BinhLuans.RemoveRange(comments);
                    
                    // 3. Xóa doanh thu
                    var revenue = await _context.DoanhThus
                        .FirstOrDefaultAsync(d => d.MaNguoiDung == model.UserId);
                    if (revenue != null)
                        _context.DoanhThus.Remove(revenue);
                    
                    // 4. Xóa lượt thích
                    var likes = await _context.LuotThiches
                        .Where(l => l.MaNguoiDung == model.UserId)
                        .ToListAsync();
                    _context.LuotThiches.RemoveRange(likes);
                    
                    // 5. Xóa lưu tranh
                    var bookmarks = await _context.LuuTranhs
                        .Where(l => l.MaNguoiDung == model.UserId)
                        .ToListAsync();
                    _context.LuuTranhs.RemoveRange(bookmarks);
                    
                    // 6. Xóa media
                    var media = await _context.Media
                        .Where(m => m.MaNguoiDung == model.UserId)
                        .ToListAsync();
                    _context.Media.RemoveRange(media);
                    
                    // 7. Xóa nổi bật
                    var features = await _context.NoiBats
                        .Where(n => n.MaNguoiDung == model.UserId)
                        .ToListAsync();
                    _context.NoiBats.RemoveRange(features);
                    
                    // 8. Xóa theo dõi
                    var follows = await _context.TheoDois
                        .Where(t => t.MaNguoiTheoDoi == model.UserId || t.MaNguoiDuocTheoDoi == model.UserId)
                        .ToListAsync();
                    _context.TheoDois.RemoveRange(follows);
                    
                    // 9. Xóa thông báo
                    var notifications = await _context.ThongBaos
                        .Where(t => t.MaNguoiNhan == model.UserId || t.MaNguoiGui == model.UserId)
                        .ToListAsync();
                    _context.ThongBaos.RemoveRange(notifications);
                    
                    // 10. Xóa tin nhắn
                    var messages = await _context.TinNhans
                        .Where(t => t.MaNguoiGui == model.UserId || t.MaNguoiNhan == model.UserId)
                        .ToListAsync();
                    _context.TinNhans.RemoveRange(messages);
                    
                    // 11. Xử lý tác phẩm nghệ thuật
                    var artworks = await _context.Tranhs
                        .Include(t => t.BinhLuans)
                        .ThenInclude(b => b.MaNguoiDungNavigation)
                        .Include(t => t.GiaoDiches)
                        .Include(t => t.LuotThiches)
                        .Include(t => t.LuuTranhs)
                        .Include(t => t.NoiBats)
                        .Include(t => t.MaTags)
                        .Include(t => t.MaTheLoais)
                        .Where(t => t.MaNguoiDung == model.UserId)
                        .ToListAsync();
                    
                    foreach (var artwork in artworks)
                    {
                        // 11.1 Lấy tất cả mã bình luận để xóa phản hồi
                        var commentIds = artwork.BinhLuans.Select(b => b.MaBinhLuan).ToList();
                        
                        // 11.2 Xóa phản hồi bình luận
                        var artworkReplies = await _context.PhanHoiBinhLuans
                            .Where(r => commentIds.Contains(r.MaBinhLuan))
                            .ToListAsync();
                        _context.PhanHoiBinhLuans.RemoveRange(artworkReplies);
                        
                        // 11.3 Xóa bình luận
                        _context.BinhLuans.RemoveRange(artwork.BinhLuans);
                        
                        // 11.4 Xóa lượt thích
                        _context.LuotThiches.RemoveRange(artwork.LuotThiches);
                        
                        // 11.5 Xóa lưu tranh
                        _context.LuuTranhs.RemoveRange(artwork.LuuTranhs);
                        
                        // 11.6 Xóa nổi bật
                        _context.NoiBats.RemoveRange(artwork.NoiBats);
                        
                        // 11.7 Xóa liên kết với thẻ tag (không xóa tag)
                        artwork.MaTags.Clear();
                        
                        // 11.8 Xóa liên kết với thể loại (không xóa thể loại)
                        artwork.MaTheLoais.Clear();
                        
                        // 11.9 Xóa giao dịch
                        var transactions = artwork.GiaoDiches.ToList();
                        _context.GiaoDiches.RemoveRange(transactions);
                        
                        // 11.10 Xóa file ảnh gốc trên server
                        if (!string.IsNullOrEmpty(artwork.DuongDanAnh))
                        {
                            var imagePath = Path.Combine(
                                Directory.GetCurrentDirectory(), 
                                "wwwroot", 
                                artwork.DuongDanAnh.TrimStart('/')
                            );
                            
                            if (System.IO.File.Exists(imagePath))
                            {
                                try {
                                    System.IO.File.Delete(imagePath);
                                } catch (Exception ex) {
                                    _logger.LogWarning($"Không thể xóa file ảnh: {ex.Message}");
                                }
                            }
                        }
                    }
                    
                    // 11.11 Xóa tranh
                    _context.Tranhs.RemoveRange(artworks);
                    
                    // 12. Xóa giao dịch mua của người dùng
                    var purchases = await _context.GiaoDiches
                        .Where(g => g.MaNguoiMua == model.UserId)
                        .ToListAsync();
                    _context.GiaoDiches.RemoveRange(purchases);
                    
                    // 13. Xóa người dùng từ tất cả vai trò
                    var roles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, roles);
                    
                    // 14. Xóa avatar và cover image nếu có
                    if (!string.IsNullOrEmpty(user.AnhDaiDien))
                    {
                        var avatarPath = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot",
                            "images",
                            "authors",
                            "avatars",
                            user.UserName,
                            user.AnhDaiDien
                        );
                        
                        if (System.IO.File.Exists(avatarPath))
                        {
                            try {
                                System.IO.File.Delete(avatarPath);
                            } catch (Exception ex) {
                                _logger.LogWarning($"Không thể xóa avatar: {ex.Message}");
                            }
                        }
                    }
                    
                    if (!string.IsNullOrEmpty(user.CoverImage))
                    {
                        var coverPath = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot",
                            "images",
                            "authors",
                            "covers",
                            user.UserName,
                            user.CoverImage
                        );
                        
                        if (System.IO.File.Exists(coverPath))
                        {
                            try {
                                System.IO.File.Delete(coverPath);
                            } catch (Exception ex) {
                                _logger.LogWarning($"Không thể xóa cover image: {ex.Message}");
                            }
                        }
                    }
                    
                    // 15. Cuối cùng xóa người dùng
                    await _userManager.DeleteAsync(user);
                    
                    // Xác nhận giao dịch
                    await transaction.CommitAsync();
                    
                    return Ok(new { success = true, message = "Đã xóa người dùng và tất cả dữ liệu liên quan thành công" });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error deleting user data");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }

    public class DeleteUserViewModel
    {
        public string UserId { get; set; }
    }
}