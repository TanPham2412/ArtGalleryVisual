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
using System.Linq;

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

        public IActionResult AdminHomePage()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId != null)
                {
                    var admin = _userManager.FindByIdAsync(userId).Result;
                    if (admin != null)
                    {
                        // Sử dụng phương thức mở rộng GetAvatarPath() giống _NavigationPartial
                        ViewBag.AdminAvatar = admin.GetAvatarPath();
                        ViewBag.AdminName = admin.TenNguoiDung;
                    }
                }
                
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin user");
                ViewBag.AdminAvatar = "/images/default-avatar.png";
                return View();
            }
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
                        artistId = t.MaNguoiDung,
                        category = t.MaTheLoais.FirstOrDefault() != null ? t.MaTheLoais.FirstOrDefault().TenTheLoai : "Chưa phân loại",
                        price = t.Gia,
                        createdDate = t.NgayDang
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

        // Comment out phương thức DeleteUser trong AdminController.cs
        // [HttpPost]
        // public async Task<IActionResult> DeleteUser([FromBody] DeleteUserViewModel model)
        // {
        //     try
        //     {
        //         if (model == null || string.IsNullOrEmpty(model.UserId))
        //             return BadRequest(new { success = false, message = "ID người dùng không hợp lệ" });

        //         var user = await _userManager.FindByIdAsync(model.UserId);
        //         if (user == null)
        //             return NotFound(new { success = false, message = "Không tìm thấy người dùng" });

        //         if (User.FindFirstValue(ClaimTypes.NameIdentifier) == model.UserId)
        //             return BadRequest(new { success = false, message = "Không thể tự xóa tài khoản của bạn" });

        //         using var transaction = await _context.Database.BeginTransactionAsync();
        //         try
        //         {
        //             // 1. Xóa dữ liệu phản hồi bình luận
        //             var userCommentIds = await _context.BinhLuans
        //                 .Where(b => b.MaNguoiDung == model.UserId)
        //                 .Select(b => b.MaBinhLuan)
        //                 .ToListAsync();
            
        //             var replies = await _context.PhanHoiBinhLuans
        //                 .Where(r => userCommentIds.Contains(r.MaBinhLuan) || r.MaNguoiDung == model.UserId)
        //                 .ToListAsync();
        //             _context.PhanHoiBinhLuans.RemoveRange(replies);
            
        //             // 2. Xóa bình luận
        //             var comments = await _context.BinhLuans
        //                 .Where(b => b.MaNguoiDung == model.UserId)
        //                 .ToListAsync();
        //             _context.BinhLuans.RemoveRange(comments);
            
        //             // Các phần xóa dữ liệu liên quan khác...
            
        //             // 15. Cuối cùng xóa người dùng
        //             await _userManager.DeleteAsync(user);
            
        //             // Xác nhận giao dịch
        //             await transaction.CommitAsync();
            
        //             return Ok(new { success = true, message = "Đã xóa người dùng và tất cả dữ liệu liên quan thành công" });
        //         }
        //         catch (Exception ex)
        //         {
        //             await transaction.RollbackAsync();
        //             _logger.LogError(ex, "Error deleting user data");
        //             throw;
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error deleting user");
        //         return StatusCode(500, new { success = false, message = ex.Message });
        //     }
        // }

        [HttpPost]
        public async Task<IActionResult> DeleteArtwork([FromBody] DeleteArtworkViewModel model)
        {
            try
            {
                _logger.LogInformation($"DeleteArtwork called: ArtworkId={model?.ArtworkId}, ArtistId={model?.ArtistId}");
                
                if (model == null || model.ArtworkId <= 0)
                {
                    _logger.LogWarning("Invalid delete artwork data received");
                    return BadRequest(new { success = false, message = "Thông tin không hợp lệ" });
                }

                // Nếu ArtistId không được cung cấp, lấy từ database
                if (string.IsNullOrEmpty(model.ArtistId))
                {
                    var artworkInfo = await _context.Tranhs
                        .Where(t => t.MaTranh == model.ArtworkId)
                        .Select(t => new { t.MaNguoiDung })
                        .FirstOrDefaultAsync();
                        
                    if (artworkInfo != null)
                        model.ArtistId = artworkInfo.MaNguoiDung;
                }

                // Sửa lại để có lý do mặc định
                if (string.IsNullOrEmpty(model.Reason))
                    model.Reason = "Xóa bởi quản trị viên";

                var artwork = await _context.Tranhs
                    .Include(t => t.BinhLuans)
                    .Include(t => t.GiaoDiches)
                    .Include(t => t.LuotThiches)
                    .Include(t => t.LuuTranhs)
                    .Include(t => t.NoiBats)
                    .Include(t => t.MaTags)
                    .Include(t => t.MaTheLoais)
                    .FirstOrDefaultAsync(t => t.MaTranh == model.ArtworkId);
                    
                if (artwork == null)
                    return NotFound(new { success = false, message = "Không tìm thấy tác phẩm" });

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Lấy tất cả mã bình luận để xóa phản hồi
                    var commentIds = artwork.BinhLuans.Select(b => b.MaBinhLuan).ToList();
                    
                    // 1. Xóa phản hồi bình luận
                    var replies = await _context.PhanHoiBinhLuans
                        .Where(r => commentIds.Contains(r.MaBinhLuan))
                        .ToListAsync();
                    _context.PhanHoiBinhLuans.RemoveRange(replies);
                    
                    // 2. Xóa bình luận
                    _context.BinhLuans.RemoveRange(artwork.BinhLuans);
                    
                    // 3. Xóa lượt thích
                    _context.LuotThiches.RemoveRange(artwork.LuotThiches);
                    
                    // 4. Xóa lưu tranh
                    _context.LuuTranhs.RemoveRange(artwork.LuuTranhs);
                    
                    // 5. Xóa nổi bật
                    _context.NoiBats.RemoveRange(artwork.NoiBats);
                    
                    // 6. Xóa liên kết với thẻ tag (không xóa tag)
                    artwork.MaTags.Clear();
                    
                    // 7. Xóa liên kết với thể loại (không xóa thể loại)
                    artwork.MaTheLoais.Clear();
                    
                    // 8. Xóa tất cả giao dịch liên quan đến tác phẩm này
                    _context.GiaoDiches.RemoveRange(artwork.GiaoDiches);
                    
                    // 9. Xóa file ảnh gốc trên server
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
                    
                    // 10. Gửi thông báo cho nghệ sĩ
                    await _notificationRepository.CreateSystemNotification(
                        model.ArtistId, 
                        "Tác phẩm của bạn đã bị xóa",
                        $"Tác phẩm '{artwork.TieuDe}' đã bị quản trị viên xóa với lý do: {model.Reason}",
                        "/User/Gallery/" + model.ArtistId,
                        "system"
                    );
                    
                    // 11. Cuối cùng xóa tác phẩm
                    _context.Tranhs.Remove(artwork);
                    await _context.SaveChangesAsync();
                    
                    await transaction.CommitAsync();
                    
                    return Ok(new { success = true });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error deleting artwork data");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting artwork");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetOrderDetail(int id)
        {
            try
            {
                var order = await _context.GiaoDiches
                    .Include(g => g.MaNguoiMuaNavigation)
                    .Include(g => g.MaTranhNavigation)
                        .ThenInclude(t => t.MaNguoiDungNavigation)
                    .FirstOrDefaultAsync(g => g.MaGiaoDich == id);
                    
                if (order == null)
                    return NotFound(new { success = false, message = "Không tìm thấy đơn hàng" });
                    
                // Không cần lấy từ bảng Orders vì không tồn tại bảng này
                // Sử dụng thông tin địa chỉ và số điện thoại từ người mua
                    
                var result = new
                {
                    success = true,
                    orderId = order.MaGiaoDich,
                    orderDate = order.NgayMua,
                    status = order.TrangThai,
                    paymentMethod = order.PhuongThucThanhToan,
                    
                    // Thông tin khách hàng
                    customerName = order.MaNguoiMuaNavigation.TenNguoiDung,
                    customerEmail = order.MaNguoiMuaNavigation.Email,
                    customerPhone = order.MaNguoiMuaNavigation.PhoneNumber,
                    shippingAddress = order.MaNguoiMuaNavigation.DiaChi,
                    
                    // Thông tin sản phẩm
                    artworkId = order.MaTranh,
                    artworkTitle = order.MaTranhNavigation.TieuDe,
                    artworkImage = order.MaTranhNavigation.DuongDanAnh,
                    artistName = order.MaTranhNavigation.MaNguoiDungNavigation.TenNguoiDung,
                    artworkPrice = order.MaTranhNavigation.Gia,
                    quantity = order.SoLuong,
                    totalAmount = order.SoTien
                };
                    
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order details");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRevenueStats()
        {
            try
            {
                int currentYear = DateTime.Now.Year;
                var labels = new List<string>();
                var values = new List<decimal>();

                for (int i = 1; i <= 12; i++)
                {
                    labels.Add("Tháng " + i);
                    values.Add(0);
                }

                var completedTransactions = await _context.GiaoDiches
                    .Where(g => g.TrangThai == "Đã hoàn thành")
                    .ToListAsync();

                foreach (var transaction in completedTransactions)
                {
                    if (transaction.NgayMua?.Year == currentYear)
                    {
                        int monthIndex = transaction.NgayMua.Value.Month - 1;
                        values[monthIndex] += transaction.SoTien;
                    }
                }

                return Json(new { labels, values });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting revenue stats");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserStats()
        {
            try
            {
                int currentYear = DateTime.Now.Year;
                var labels = new List<string>();
                var values = new List<int>();

                // Tạo chuỗi nhãn cho 12 tháng
                for (int i = 1; i <= 12; i++)
                {
                    labels.Add("Tháng " + i);
                    values.Add(0); // Khởi tạo giá trị 0 cho mỗi tháng
                }

                // Lấy tất cả người dùng
                var users = await _context.NguoiDungs.ToListAsync();

                // Lọc và đếm theo tháng
                foreach (var user in users)
                {
                    if (user.NgayDangKy != null && user.NgayDangKy.Year == currentYear)
                    {
                        int monthIndex = user.NgayDangKy.Month - 1; // Chỉ số bắt đầu từ 0
                        values[monthIndex]++;
                    }
                }

                return Json(new { labels, values });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user stats");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoryStats()
        {
            try
            {
                var labels = new List<string>();
                var values = new List<int>();

                // Lấy tất cả tranh có thể loại
                var artworksWithCategories = await _context.Tranhs
                    .Include(t => t.MaTheLoais)
                    .ToListAsync();
                    
                // Lấy tất cả thể loại
                var allCategories = await _context.TheLoais.ToListAsync();
                
                // Đếm số lượng tranh cho mỗi thể loại
                var categoryCounts = new Dictionary<int, int>();
                
                foreach (var artwork in artworksWithCategories)
                {
                    if (artwork.MaTheLoais != null && artwork.MaTheLoais.Any())
                    {
                        foreach (var category in artwork.MaTheLoais)
                        {
                            if (!categoryCounts.ContainsKey(category.MaTheLoai))
                                categoryCounts[category.MaTheLoai] = 0;
                            
                            categoryCounts[category.MaTheLoai]++;
                        }
                    }
                }
                
                // Đếm tranh chưa phân loại
                int uncategorizedCount = artworksWithCategories.Count(a => a.MaTheLoais == null || !a.MaTheLoais.Any());
                
                // Thêm dữ liệu vào biểu đồ
                foreach (var category in allCategories)
                {
                    if (categoryCounts.ContainsKey(category.MaTheLoai))
                    {
                        labels.Add(category.TenTheLoai);
                        values.Add(categoryCounts[category.MaTheLoai]);
                    }
                }
                
                if (uncategorizedCount > 0)
                {
                    labels.Add("Chưa phân loại");
                    values.Add(uncategorizedCount);
                }

                return Json(new { labels, values });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category stats");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetArtworkStats()
        {
            try
            {
                int currentYear = DateTime.Now.Year;
                var labels = new List<string>();
                var values = new List<int>();

                // Tạo chuỗi nhãn cho 12 tháng
                for (int i = 1; i <= 12; i++)
                {
                    labels.Add("Tháng " + i);
                    values.Add(0);
                }

                // Lấy tất cả tranh
                var artworks = await _context.Tranhs.ToListAsync();

                // Lọc và đếm theo tháng
                foreach (var artwork in artworks)
                {
                    if (artwork.NgayDang?.Year == currentYear)
                    {
                        int monthIndex = artwork.NgayDang.Value.Month - 1;
                        values[monthIndex]++;
                    }
                }

                return Json(new { labels, values });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting artwork stats");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }

    public class DeleteUserViewModel
    {
        public string UserId { get; set; }
    }

    public class DeleteArtworkViewModel
    {
        public int ArtworkId { get; set; }
        public string ArtistId { get; set; }
        public string Reason { get; set; }
    }
}