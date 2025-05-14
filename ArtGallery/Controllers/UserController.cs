using Microsoft.AspNetCore.Mvc;
using ArtGallery.Repositories.Interfaces;
using ArtGallery.Models;
using ArtGallery.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ArtGallery.ViewModels;

namespace ArtGallery.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserController> _logger;
        private readonly UserManager<NguoiDung> _userManager;
        private readonly ArtGalleryContext _context;
        private readonly INotificationRepository _notificationRepository;

        public UserController(
            IUserRepository userRepository, 
            ILogger<UserController> logger,
            UserManager<NguoiDung> userManager,
            ArtGalleryContext context,
            INotificationRepository notificationRepository)
        {
            _userRepository = userRepository;
            _logger = logger;
            _userManager = userManager;
            _context = context;
            _notificationRepository = notificationRepository;
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

                // Lấy danh sách tác phẩm nổi bật
                var featuredArtworks = await _context.NoiBats
                    .Where(n => n.MaNguoiDung == id)
                    .Include(n => n.MaTranhNavigation)
                    .Select(n => n.MaTranhNavigation)
                    .ToListAsync();

                ViewBag.FeaturedArtworks = featuredArtworks;

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

        public async Task<IActionResult> RegisterArtist()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");
        
            var user = await _userManager.GetUserAsync(User);
            if (await _userManager.IsInRoleAsync(user, "Artists"))
                return RedirectToAction("Gallery", new { id = user.Id });
        
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterArtist(ArtistRegistrationViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            // Cập nhật thông tin nghệ sĩ
            user.TenNguoiDung = model.TenNgheSi;
            user.DiaChi = model.DiaChi;
            user.MoTa = model.MoTa;
            user.PhoneNumber = model.SoDienThoai;
            user.DangKyNgheSi = true; // Đánh dấu đã đăng ký, chờ xét duyệt
            
            await _userManager.UpdateAsync(user);

            // Gửi thông báo cho Admin
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            foreach (var admin in admins)
            {
                await _notificationRepository.CreateNotification(
                    admin.Id, 
                    user.Id,
                    "Đăng ký nghệ sĩ mới", 
                    $"{user.TenNguoiDung} đã đăng ký trở thành nghệ sĩ",
                    $"/Admin/ArtistApproval/{user.Id}",
                    "system",
                    user.GetAvatarPath()
                );
            }

            TempData["SuccessMessage"] = "Đăng ký nghệ sĩ thành công! Vui lòng chờ quản trị viên phê duyệt.";
            return RedirectToAction("Profile", new { id = user.Id });
        }

        public IActionResult PendingArtistApproval()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ToggleFollow(string followedUserId)
        {
            try
            {
                if (string.IsNullOrEmpty(followedUserId))
                {
                    return Json(new { success = false, message = "ID người dùng không hợp lệ" });
                }

                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "Bạn cần đăng nhập để thực hiện thao tác này" });
                }

                // Không thể tự theo dõi chính mình
                if (currentUser.Id == followedUserId)
                {
                    return Json(new { success = false, message = "Bạn không thể theo dõi chính mình" });
                }

                // Kiểm tra xem đã theo dõi hay chưa
                var existingFollow = await _context.TheoDois
                    .FirstOrDefaultAsync(t => t.MaNguoiTheoDoi == currentUser.Id && t.MaNguoiDuocTheoDoi == followedUserId);

                bool isFollowing = false;

                if (existingFollow != null)
                {
                    // Nếu đã theo dõi, hủy theo dõi
                    _context.TheoDois.Remove(existingFollow);
                    isFollowing = false;
                }
                else
                {
                    // Nếu chưa theo dõi, tạo mới
                    var newFollow = new TheoDoi
                    {
                        MaNguoiTheoDoi = currentUser.Id,
                        MaNguoiDuocTheoDoi = followedUserId,
                        NgayTheoDoi = DateTime.Now
                    };

                    await _context.TheoDois.AddAsync(newFollow);
                    isFollowing = true;

                    // Gửi thông báo cho người được theo dõi
                    await _notificationRepository.CreateNotification(
                        followedUserId,
                        currentUser.Id,
                        "Có người theo dõi mới",
                        $"{currentUser.TenNguoiDung} đã bắt đầu theo dõi bạn",
                        $"/User/Profile/{currentUser.Id}",
                        "follow",
                        currentUser.GetAvatarPath()
                    );
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, isFollowing });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi theo dõi/hủy theo dõi người dùng {UserId}", followedUserId);
                return Json(new { success = false, message = "Có lỗi xảy ra khi thực hiện thao tác" });
            }
        }

        [Authorize]
        public async Task<IActionResult> ProductStatistics(string id = null)
        {
            try
            {
                // Xác định ID người dùng cần xem
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

                // Kiểm tra quyền truy cập (chỉ admin hoặc chủ sở hữu)
                var loggedInUser = await _userManager.GetUserAsync(User);
                var isAdmin = await _userManager.IsInRoleAsync(loggedInUser, "Admin");
                var isOwner = loggedInUser?.Id == id;

                if (!isAdmin && !isOwner)
                {
                    return Forbid();
                }

                // Lấy thông tin người dùng và danh sách tranh
                var user = await _context.NguoiDungs
                    .Include(u => u.DoanhThu)
                    .Include(u => u.Tranhs)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return NotFound("Không tìm thấy người dùng");
                }
                
                // Lấy thông tin giao dịch của các tranh
                var artworkIds = user.Tranhs.Select(t => t.MaTranh).ToList();
                var transactions = await _context.GiaoDiches
                    .Where(g => artworkIds.Contains(g.MaTranh) && g.TrangThai == "Đã hoàn thành")
                    .ToListAsync();

                // Tính toán thống kê chi tiết cho từng tranh
                var artworkStatistics = user.Tranhs.Select(tranh => new ArtworkStatisticsViewModel
                {
                    Artwork = tranh,
                    TotalSales = transactions.Where(t => t.MaTranh == tranh.MaTranh)
                                  .Sum(t => t.SoTien)
                }).ToList();

                ViewBag.User = user;
                ViewBag.TotalRevenue = user.DoanhThu?.TongDoanhThu ?? 0;
                ViewBag.TotalSoldArtworks = user.DoanhThu?.SoTranhBanDuoc ?? 0;

                return View(artworkStatistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi hiển thị thống kê sản phẩm của người dùng {UserId}", id);
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserArtworks(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "ID người dùng không hợp lệ" });
            }

            try
            {
                var user = await _context.NguoiDungs
                    .Include(u => u.Tranhs)
                    .Include(u => u.NoiBats)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy người dùng" });
                }

                // Lấy danh sách ID tranh đã thêm vào nổi bật
                var featuredArtworkIds = user.NoiBats
                    .Select(nb => nb.MaTranh)
                    .ToList();

                // Lấy thông tin tranh và đánh dấu những tranh đã được thêm vào nổi bật
                var artworks = user.Tranhs
                    .Select(t => new
                    {
                        maTranh = t.MaTranh,
                        tieuDe = t.TieuDe,
                        duongDanAnh = t.DuongDanAnh,
                        isFeatured = featuredArtworkIds.Contains(t.MaTranh)
                    })
                    .ToList();

                return Json(artworks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách tác phẩm của người dùng {UserId}", userId);
                return Json(new { success = false, message = "Có lỗi xảy ra khi lấy danh sách tác phẩm" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ToggleFeaturedArtwork([FromBody] FeaturedArtworkViewModel model)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "Bạn cần đăng nhập để thực hiện thao tác này" });
                }

                var artwork = await _context.Tranhs
                    .FirstOrDefaultAsync(t => t.MaTranh == model.ArtworkId);

                if (artwork == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy tác phẩm" });
                }

                // Chỉ cho phép thêm tác phẩm của chính mình
                if (artwork.MaNguoiDung != currentUser.Id)
                {
                    return Json(new { success = false, message = "Bạn chỉ có thể thêm tác phẩm của chính mình vào danh sách nổi bật" });
                }

                // Kiểm tra xem tác phẩm đã có trong danh sách nổi bật chưa
                var existingFeatured = await _context.NoiBats
                    .FirstOrDefaultAsync(n => n.MaNguoiDung == currentUser.Id && n.MaTranh == model.ArtworkId);

                if (model.IsFeatured)
                {
                    // Thêm vào danh sách nổi bật nếu chưa có
                    if (existingFeatured == null)
                    {
                        var newFeatured = new NoiBat
                        {
                            MaNguoiDung = currentUser.Id,
                            MaTranh = model.ArtworkId,
                     
                        };

                        await _context.NoiBats.AddAsync(newFeatured);
                        await _context.SaveChangesAsync();
                    }

                    return Json(new
                    {
                        success = true,
                        message = "Đã thêm tác phẩm vào danh sách nổi bật",
                        artwork = new
                        {
                            maTranh = artwork.MaTranh,
                            tieuDe = artwork.TieuDe,
                            duongDanAnh = artwork.DuongDanAnh
                        }
                    });
                }
                else
                {
                    // Xóa khỏi danh sách nổi bật nếu đã có
                    if (existingFeatured != null)
                    {
                        _context.NoiBats.Remove(existingFeatured);
                        await _context.SaveChangesAsync();
                    }

                    return Json(new
                    {
                        success = true,
                        message = "Đã xóa tác phẩm khỏi danh sách nổi bật"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật tác phẩm nổi bật");
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật tác phẩm nổi bật" });
            }
        }
    }
}