using Microsoft.AspNetCore.Mvc;
using ArtGallery.Models;
using ArtGallery.Repositories.Interfaces;
using ArtGallery.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.AspNetCore.SignalR;
using ArtGallery.Hubs;

namespace ArtGallery.Controllers
{
    public class ArtworkController : Controller
    {
        private readonly IArtworkRepository _artworkRepository;
        private readonly ILogger<ArtworkController> _logger;
        private readonly ArtGalleryContext _context;
        private readonly IHubContext<CommentHub> _commentHubContext;
        private readonly IContentModerationService _contentModerationService;

        public ArtworkController(IArtworkRepository artworkRepository, ILogger<ArtworkController> logger, ArtGalleryContext context, IHubContext<CommentHub> commentHubContext, IContentModerationService contentModerationService)
        {
            _artworkRepository = artworkRepository;
            _logger = logger;
            _context = context;
            _commentHubContext = commentHubContext;
            _contentModerationService = contentModerationService;
        }

        public async Task<IActionResult> Display(int id)
        {
            var result = await _artworkRepository.GetArtworkForDisplay(id);
            if (result.artwork == null)
                return NotFound();

            ViewBag.OtherWorks = result.otherWorks;
            
            // Lấy danh sách bình luận và sắp xếp theo thời gian mới nhất
            var comments = await _context.BinhLuans
                .Include(b => b.MaNguoiDungNavigation)
                .Where(b => b.MaTranh == id)
                .OrderByDescending(b => b.NgayBinhLuan)
                .ToListAsync();
            
            ViewBag.Comments = comments;
            
            // Lấy tất cả phản hồi cho bình luận
            if (comments.Any())
            {
                var commentIds = comments.Select(c => c.MaBinhLuan).ToList();
                var replies = await _context.PhanHoiBinhLuans
                    .Include(r => r.MaNguoiDungNavigation)
                    .Where(r => commentIds.Contains(r.MaBinhLuan))
                    .OrderBy(r => r.NgayPhanHoi)
                    .ToListAsync();
                    
                // Nhóm phản hồi theo MaBinhLuan
                var repliesByCommentId = replies.GroupBy(r => r.MaBinhLuan)
                    .ToDictionary(g => g.Key, g => g.ToList());
                    
                ViewBag.Replies = repliesByCommentId;
            }
            
            // Kiểm tra tài khoản người dùng đang đăng nhập
            var currentUser = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (currentUser != null)
            {
                ViewBag.IsCurrentUserLocked = currentUser.LockoutEnabled && currentUser.LockoutEnd != null && currentUser.LockoutEnd > DateTimeOffset.UtcNow;
                ViewBag.CurrentUserLockoutEnd = currentUser.LockoutEnd;
                ViewBag.CurrentUserLockoutReason = currentUser.LockoutReason;
            }

            // Kiểm tra tài khoản của người bán (tác giả)
            var artist = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Id == result.artwork.MaNguoiDung);
            if (artist != null)
            {
                ViewBag.IsArtistLocked = artist.LockoutEnabled && artist.LockoutEnd != null && artist.LockoutEnd > DateTimeOffset.UtcNow;
                ViewBag.ArtistLockoutEnd = artist.LockoutEnd;
                ViewBag.ArtistLockoutReason = artist.LockoutReason;
            }
            
            return View(result.artwork);
        }

        public async Task<IActionResult> ByCategory(int id)
        {
            var result = await _artworkRepository.GetArtworksByCategory(id);
            if (result.category == null)
                return NotFound();

            ViewBag.CategoryName = result.category.TenTheLoai;
            return View(result.artworks);
        }

        public async Task<IActionResult> ByTag(int id)
        {
            var result = await _artworkRepository.GetArtworksByTag(id);
            if (result.tag == null)
                return NotFound();

            ViewBag.TagName = result.tag.TenTag;
            return View(result.artworks);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            
            var artwork = await _artworkRepository.GetArtworkForEdit(id, currentUserId, isAdmin);
            
            if (artwork == null)
                return NotFound();

            ViewBag.Categories = await _artworkRepository.GetCategories();
            return View(artwork);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Tranh model, IFormFile? ImageFile, string TagsInput, List<int> SelectedCategories)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            
            _logger.LogInformation($"Current User ID: {currentUserId}, IsAdmin: {isAdmin}");

            try 
            {
                ModelState.Remove("MaNguoiDungNavigation");
                ModelState.Remove("TagsInput");

                if (!ModelState.IsValid)
                {
                    ViewBag.Categories = await _artworkRepository.GetCategories();
                    TempData["ErrorMessage"] = "Dữ liệu không hợp lệ";
                    return View(model);
                }

                if (model.Gia < 0)
                {
                    ViewBag.Categories = await _artworkRepository.GetCategories();
                    TempData["ErrorMessage"] = "Giá không thể là số âm";
                    return View(model);
                }
                
                if (model.SoLuongTon < 0)
                {
                    ViewBag.Categories = await _artworkRepository.GetCategories();
                    TempData["ErrorMessage"] = "Số lượng không thể là số âm";
                    return View(model);
                }

                var result = await _artworkRepository.UpdateArtwork(model, ImageFile, TagsInput, SelectedCategories, currentUserId, isAdmin);

                if (result.success)
                {
                    TempData["SuccessMessage"] = "Cập nhật thành công!";
                    return RedirectToAction("Display", new { id = model.MaTranh });
                }
                else
                {
                    ViewBag.Categories = await _artworkRepository.GetCategories();
                    TempData["ErrorMessage"] = result.message;
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật tranh");
                ViewBag.Categories = await _artworkRepository.GetCategories();
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật tranh";
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            
            _logger.LogInformation($"Delete request: ArtworkID={id}, UserID={currentUserId}, IsAdmin={isAdmin}");
            
            try 
            {
                // Kiểm tra quyền sở hữu
                var artwork = await _context.Tranhs
                    .Include(t => t.BinhLuans)
                    .Include(t => t.GiaoDiches)
                    .Include(t => t.LuotThiches)
                    .Include(t => t.LuuTranhs)
                    .Include(t => t.NoiBats)
                    .Include(t => t.MaTags)
                    .Include(t => t.MaTheLoais)
                    .FirstOrDefaultAsync(t => t.MaTranh == id);
                    
                if (artwork == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy tác phẩm" });
                }
                
                if (artwork.MaNguoiDung != currentUserId && !isAdmin)
                {
                    return Json(new { success = false, message = "Bạn không có quyền xóa tác phẩm này" });
                }
                
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
                
                // 8. Kiểm tra giao dịch (nếu có giao dịch hoàn thành, không xóa tranh)
                var completedTransactions = artwork.GiaoDiches
                    .Where(g => g.TrangThai == "Đã hoàn thành" || g.TrangThai == "Đang giao hàng")
                    .ToList();
                    
                if (completedTransactions.Any())
                {
                    return Json(new { 
                        success = false, 
                        message = "Không thể xóa tác phẩm đã bán. Hãy cập nhật trạng thái để ẩn thay vì xóa." 
                    });
                }
                
                // 9. Xóa giao dịch chưa hoàn thành
                var pendingTransactions = artwork.GiaoDiches
                    .Where(g => g.TrangThai != "Đã hoàn thành" && g.TrangThai != "Đang giao hàng")
                    .ToList();
                _context.GiaoDiches.RemoveRange(pendingTransactions);
                
                // 10. Xóa file ảnh gốc trên server (nếu cần)
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
                
                // 11. Cuối cùng xóa tác phẩm
                _context.Tranhs.Remove(artwork);
                await _context.SaveChangesAsync();
                
                return Json(new { 
                    success = true, 
                    message = "Xóa tác phẩm thành công",
                    redirectUrl = "/Artwork/Products" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa tác phẩm");
                return Json(new { 
                    success = false, 
                    message = "Có lỗi xảy ra khi xóa tác phẩm" 
                });
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Admin(string sortOrder, string searchString, string categoryFilter)
        {
            try
            {
                // Nếu có categoryFilter thì ưu tiên lọc theo thể loại
                if (!string.IsNullOrEmpty(categoryFilter))
                {
                    var artworks = await _context.Tranhs
                        .Include(t => t.MaNguoiDungNavigation)
                        .Include(t => t.MaTheLoais)
                        .Include(t => t.MaTags)
                        .Where(t => t.MaTheLoais.Any(c => c.TenTheLoai.Contains(categoryFilter)))
                        .ToListAsync();
                        
                    if (sortOrder != null)
                    {
                        // Áp dụng sắp xếp nếu có
                        artworks = _artworkRepository.ApplySorting(artworks, sortOrder);
                    }
                        
                    ViewBag.CategoryFilter = categoryFilter;
                    return View(artworks);
                }
                
                var filteredArtworks = await _artworkRepository.GetFilteredArtworks(searchString, sortOrder);
                return View(filteredArtworks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách tranh cho Admin");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách tranh";
                return RedirectToAction("Index", "Home");
            }
        }

        // Xử lý khi người dùng không có quyền truy cập
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLike(int artworkId)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để thích tác phẩm" });
                }

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _artworkRepository.ToggleLike(artworkId, currentUserId);

                return Json(new { 
                    success = result.success, 
                    liked = result.liked,
                    message = result.message 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thay đổi trạng thái yêu thích");
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> FilterByCategory(string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName))
            {
                return RedirectToAction("Products");
            }
            
            try
            {
                var artworks = await _context.Tranhs
                    .Include(t => t.MaNguoiDungNavigation)
                    .Include(t => t.MaTheLoais)
                    .Include(t => t.MaTags)
                    .Where(t => t.MaTheLoais.Any(c => c.TenTheLoai.Contains(categoryName)))
                    .ToListAsync();
                    
                ViewBag.CategoryName = categoryName;
                return View("Products", artworks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lọc tranh theo thể loại '{categoryName}'");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi lọc tranh";
                return RedirectToAction("Products");
            }
        }

        public async Task<IActionResult> Products(string sortOrder, string searchString, string categoryFilter)
        {
            try
            {
                // Nếu có categoryFilter thì ưu tiên lọc theo thể loại
                if (!string.IsNullOrEmpty(categoryFilter))
                {
                    return await FilterByCategory(categoryFilter);
                }
                
                var artworks = await _artworkRepository.GetFilteredArtworks(searchString, sortOrder);
                return View(artworks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách tranh");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách tranh";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public IActionResult GetStickers()
        {
            try {
                var basePath = "/images/stickers/";
                
                var daisuhuynhPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "stickers", "daisuhuynh");
                var nhisuhuynhPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "stickers", "nhisuhuynh");
                var tamsuhuynhPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "stickers", "tamsuhuynh");
                var tusuhuynhPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "stickers", "tusuhuynh");
                var longtuongPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "stickers", "longtuong");
                var ngutieumaiPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "stickers", "ngutieumai");
                var thuyhanhPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "stickers", "thuyhanh");
                var vanthuongPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "stickers", "vanthuong");

                var daisuhuynhFiles = new List<string>();
                var nhisuhuynhFiles = new List<string>();
                var tamsuhuynhFiles = new List<string>();
                var tusuhuynhFiles = new List<string>();
                var longtuongFiles = new List<string>();
                var ngutieumaiFiles = new List<string>();
                var thuyhanhFiles = new List<string>();
                var vanthuongFiles = new List<string>();
                
                
                if (Directory.Exists(daisuhuynhPath)) {
                    daisuhuynhFiles = Directory.GetFiles(daisuhuynhPath, "*.png")
                        .Select(f => basePath + "daisuhuynh/" + Path.GetFileName(f))
                        .ToList();
                }

                if (Directory.Exists(nhisuhuynhPath))
                {
                    nhisuhuynhFiles = Directory.GetFiles(nhisuhuynhPath, "*.png")
                        .Select(f => basePath + "nhisuhuynh/" + Path.GetFileName(f))
                        .ToList();
                }

                if (Directory.Exists(tamsuhuynhPath))
                {
                    tamsuhuynhFiles = Directory.GetFiles(tamsuhuynhPath, "*.png")
                        .Select(f => basePath + "tamsuhuynh/" + Path.GetFileName(f))
                        .ToList();
                }

                if (Directory.Exists(tusuhuynhPath))
                {
                    tusuhuynhFiles = Directory.GetFiles(tusuhuynhPath, "*.png")
                        .Select(f => basePath + "tusuhuynh/" + Path.GetFileName(f))
                        .ToList();
                }

                if (Directory.Exists(longtuongPath))
                {
                    longtuongFiles = Directory.GetFiles(longtuongPath, "*.png")
                        .Select(f => basePath + "longtuong/" + Path.GetFileName(f))
                        .ToList();
                }

                if (Directory.Exists(ngutieumaiPath))
                {
                    ngutieumaiFiles = Directory.GetFiles(ngutieumaiPath, "*.png")
                        .Select(f => basePath + "ngutieumai/" + Path.GetFileName(f))
                        .ToList();
                }

                if (Directory.Exists(thuyhanhPath))
                {
                    thuyhanhFiles = Directory.GetFiles(thuyhanhPath, "*.png")
                        .Select(f => basePath + "thuyhanh/" + Path.GetFileName(f))
                        .ToList();
                }

                if (Directory.Exists(vanthuongPath))
                {
                    vanthuongFiles = Directory.GetFiles(vanthuongPath, "*.png")
                        .Select(f => basePath + "vanthuong/" + Path.GetFileName(f))
                        .ToList();
                }

                return Json(new { 
                    success = true,
                    daisuhuynh = daisuhuynhFiles,
                    nhisuhuynh = nhisuhuynhFiles,
                    tamsuhuynh = tamsuhuynhFiles,
                    tusuhuynh = tusuhuynhFiles,
                    longtuong = longtuongFiles,
                    ngutieumai = ngutieumaiFiles,
                    thuyhanh = thuyhanhFiles,
                    vanthuong = vanthuongFiles        
                });
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Lỗi khi lấy danh sách stickers");
                return Json(new { success = false, message = "Không thể tải stickers" });
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> AddComment(BinhLuan model, int Rating, IFormFile CommentImage, string Sticker)
        {
            // Thêm log để kiểm tra
            _logger.LogInformation($"AddComment: Ảnh đã upload: {CommentImage?.FileName}, Kích thước: {CommentImage?.Length}");
            
            if (model == null || (string.IsNullOrEmpty(model.NoiDung) && CommentImage == null && string.IsNullOrEmpty(Sticker)))
            {
                return new JsonResult(new { success = false, message = "Vui lòng nhập nội dung, chọn ảnh hoặc sticker" });
            }
            
            // Lấy ID người dùng hiện tại
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Kiểm duyệt nội dung và kiểm tra spam
            if (!string.IsNullOrEmpty(model.NoiDung))
            {
                var validationResult = await _contentModerationService.ValidateCommentSpamAsync(model.NoiDung, model.MaTranh, currentUserId);
                if (!validationResult.isValid)
                {
                    return new JsonResult(new { success = false, message = validationResult.errorMessage });
                }
            }
            
            try
            {
                
                var comment = new BinhLuan
                {
                    MaTranh = model.MaTranh,
                    MaNguoiDung = currentUserId,
                    NoiDung = model.NoiDung ?? "",
                    NgayBinhLuan = DateTime.Now,
                    Rating = Rating,
                    Sticker = Sticker
                };
                
                // Xử lý upload ảnh nếu có
                if (CommentImage != null && CommentImage.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "comments");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);
                        
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + CommentImage.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await CommentImage.CopyToAsync(fileStream);
                    }
                    
                    comment.DuongDanAnh = "/images/comments/" + uniqueFileName;
                    _logger.LogInformation($"Đã lưu ảnh tại: {comment.DuongDanAnh}");
                }
                
                _context.BinhLuans.Add(comment);
                await _context.SaveChangesAsync();
                
                // Lấy thông tin người dùng để gửi qua SignalR
                var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Id == comment.MaNguoiDung);
                
                // Tạo đối tượng bình luận để gửi qua SignalR
                var commentData = new {
                    id = comment.MaBinhLuan,
                    content = comment.NoiDung,
                    userId = comment.MaNguoiDung,
                    userName = user?.TenNguoiDung ?? "Người dùng",
                    userAvatar = user != null ? user.GetAvatarPath() : "/images/default-avatar.png",
                    date = comment.NgayBinhLuan,
                    rating = comment.Rating,
                    imagePath = comment.DuongDanAnh,
                    sticker = comment.Sticker,
                    isHidden = comment.IsHidden,
                    isEdited = comment.DaChinhSua
                };
                
                // Gửi bình luận mới qua SignalR
                await _commentHubContext.Clients.Group($"artwork_{model.MaTranh}").SendAsync("ReceiveComment", commentData);
                
                return new JsonResult(new { success = true, message = "Bình luận của bạn đã được gửi thành công!", data = commentData });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm bình luận");
                return new JsonResult(new { success = false, message = "Có lỗi xảy ra khi gửi bình luận" });
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> AddReply(int MaBinhLuan, int MaTranh, string NoiDung, IFormFile ReplyImage, string Sticker)
        {
            if (string.IsNullOrEmpty(NoiDung) && ReplyImage == null && string.IsNullOrEmpty(Sticker))
            {
                return new JsonResult(new { success = false, message = "Vui lòng nhập nội dung, chọn ảnh hoặc sticker" });
            }
            
            // Kiểm duyệt nội dung
            if (!string.IsNullOrEmpty(NoiDung))
            {
                var validationResult = _contentModerationService.ValidateContent(NoiDung);
                if (!validationResult.isValid)
                {
                    return new JsonResult(new { success = false, message = validationResult.errorMessage });
                }
            }
            
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                var reply = new PhanHoiBinhLuan
                {
                    MaBinhLuan = MaBinhLuan,
                    MaNguoiDung = currentUserId,
                    NoiDung = NoiDung ?? "",
                    NgayPhanHoi = DateTime.Now,
                    Sticker = Sticker
                };
                
                // Xử lý upload ảnh nếu có
                if (ReplyImage != null && ReplyImage.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "comments");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);
                        
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + ReplyImage.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ReplyImage.CopyToAsync(fileStream);
                    }
                    
                    reply.DuongDanAnh = "/images/comments/" + uniqueFileName;
                }
                
                _context.PhanHoiBinhLuans.Add(reply);
                await _context.SaveChangesAsync();
                
                // Lấy thông tin người dùng để gửi qua SignalR
                var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Id == currentUserId);
                
                // Tạo đối tượng phản hồi để gửi qua SignalR
                var replyData = new {
                    id = reply.MaPhanHoi,
                    commentId = reply.MaBinhLuan,
                    content = reply.NoiDung,
                    userId = reply.MaNguoiDung,
                    userName = user?.TenNguoiDung ?? "Người dùng",
                    userAvatar = user != null ? user.GetAvatarPath() : "/images/default-avatar.png",
                    date = reply.NgayPhanHoi,
                    imagePath = reply.DuongDanAnh,
                    sticker = reply.Sticker,
                    isEdited = reply.DaChinhSua,
                    artworkId = MaTranh  // Thêm artworkId vào dữ liệu phản hồi
                };
                
                // Gửi thông tin phản hồi mới qua SignalR
                await _commentHubContext.Clients.Group($"artwork_{MaTranh}").SendAsync("ReceiveReply", MaBinhLuan, replyData);
                
                return new JsonResult(new { success = true, message = "Phản hồi của bạn đã được gửi thành công!", data = replyData });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm phản hồi bình luận");
                return new JsonResult(new { success = false, message = "Có lỗi xảy ra khi gửi phản hồi" });
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int commentId, int artworkId)
        {
            try
            {
                var comment = await _context.BinhLuans.FindAsync(commentId);
                if (comment == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bình luận này" });
                }
                
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin");
                
                // Chỉ admin hoặc người viết bình luận mới có quyền xóa
                if (isAdmin || comment.MaNguoiDung == currentUserId)
                {
                    // Trước khi xóa bình luận, xóa tất cả các phản hồi liên quan
                    var replies = await _context.PhanHoiBinhLuans
                        .Where(r => r.MaBinhLuan == commentId)
                        .ToListAsync();
                        
                    _context.PhanHoiBinhLuans.RemoveRange(replies);
                    _context.BinhLuans.Remove(comment);
                    await _context.SaveChangesAsync();
                    
                    // Thông báo xóa bình luận qua SignalR
                    await _commentHubContext.Clients.Group($"artwork_{artworkId}").SendAsync("CommentDeleted", commentId);
                    
                    return Json(new { success = true, message = "Đã xóa bình luận thành công" });
                }
                else
                {
                    return Json(new { success = false, message = "Bạn không có quyền xóa bình luận này" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa bình luận");
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa bình luận" });
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleHideComment(int commentId, int artworkId)
        {
            try
            {
                var comment = await _context.BinhLuans.FindAsync(commentId);
                if (comment == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bình luận này" });
                }
                
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin");
                
                // Chỉ admin hoặc người viết bình luận mới có quyền ẩn/hiện
                if (isAdmin || comment.MaNguoiDung == currentUserId)
                {
                    // Thêm cột IsHidden vào model BinhLuan nếu chưa có
                    comment.IsHidden = !comment.IsHidden;
                    await _context.SaveChangesAsync();
                    
                    string message = comment.IsHidden ? "Đã ẩn bình luận" : "Đã hiện bình luận";
                    return Json(new { success = true, message = message, isHidden = comment.IsHidden });
                }
                else
                {
                    return Json(new { success = false, message = "Bạn không có quyền ẩn/hiện bình luận này" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi ẩn/hiện bình luận");
                return Json(new { success = false, message = "Có lỗi xảy ra khi ẩn/hiện bình luận" });
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditComment(int commentId, int artworkId, string editedContent, 
            IFormFile commentImage, string sticker, bool keepOriginalImage = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(editedContent) && commentImage == null && string.IsNullOrEmpty(sticker))
                {
                    return Json(new { success = false, message = "Vui lòng nhập nội dung, chọn ảnh hoặc sticker" });
                }
                
                var comment = await _context.BinhLuans.FindAsync(commentId);
                if (comment == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bình luận này" });
                }
                
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin");
                
                // Kiểm duyệt nội dung và kiểm tra spam
                if (!string.IsNullOrWhiteSpace(editedContent))
                {
                    var validationResult = await _contentModerationService.ValidateCommentSpamAsync(editedContent, artworkId, currentUserId);
                    if (!validationResult.isValid)
                    {
                        return Json(new { success = false, message = validationResult.errorMessage });
                    }
                }
                
                // Chỉ admin hoặc người viết bình luận mới có quyền sửa
                if (isAdmin || comment.MaNguoiDung == currentUserId)
                {
                    // Cập nhật nội dung bình luận
                    comment.NoiDung = editedContent;
                    comment.DaChinhSua = true; // Đánh dấu đã chỉnh sửa
                    
                    // Cập nhật sticker nếu có
                    comment.Sticker = sticker;
                    
                    // Xử lý upload ảnh mới nếu có
                    if (commentImage != null && commentImage.Length > 0)
                    {
                        // Xóa ảnh cũ nếu có
                        if (!string.IsNullOrEmpty(comment.DuongDanAnh))
                        {
                            // Có thể thêm code xóa file ảnh cũ ở đây
                        }
                        
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "comments");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);
                            
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + commentImage.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await commentImage.CopyToAsync(fileStream);
                        }
                        
                        comment.DuongDanAnh = "/images/comments/" + uniqueFileName;
                        _logger.LogInformation($"Đã lưu ảnh mới tại: {comment.DuongDanAnh}");
                    }
                    else if (!keepOriginalImage)
                    {
                        // Xóa ảnh nếu người dùng đã xóa và không upload ảnh mới
                        comment.DuongDanAnh = null;
                    }
                    
                    await _context.SaveChangesAsync();
                    
                    // Lấy thông tin người dùng để gửi qua SignalR
                    var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Id == comment.MaNguoiDung);
                    
                    // Tạo đối tượng bình luận đã cập nhật để gửi qua SignalR
                    var updatedComment = new {
                        id = comment.MaBinhLuan,
                        content = comment.NoiDung,
                        userId = comment.MaNguoiDung,
                        userName = user?.TenNguoiDung ?? "Người dùng",
                        userAvatar = user?.AnhDaiDien ?? "/images/default-avatar.png",
                        date = comment.NgayBinhLuan,
                        rating = comment.Rating,
                        imagePath = comment.DuongDanAnh,
                        sticker = comment.Sticker,
                        isHidden = comment.IsHidden,
                        isEdited = comment.DaChinhSua
                    };
                    
                    // Gửi thông tin bình luận đã cập nhật qua SignalR
                    await _commentHubContext.Clients.Group($"artwork_{artworkId}").SendAsync("CommentEdited", commentId, updatedComment);
                    
                    return Json(new { 
                        success = true, 
                        message = "Đã cập nhật bình luận thành công",
                        commentId = commentId,
                        editedContent = editedContent,
                        sticker = comment.Sticker,
                        imagePath = comment.DuongDanAnh
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Bạn không có quyền sửa bình luận này" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi sửa bình luận");
                return Json(new { success = false, message = "Có lỗi xảy ra khi sửa bình luận" });
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditReply(int replyId, int commentId, int artworkId, string editedContent)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(editedContent))
                {
                    return Json(new { success = false, message = "Nội dung phản hồi không được để trống" });
                }
                
                var reply = await _context.PhanHoiBinhLuans.FindAsync(replyId);
                if (reply == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy phản hồi này" });
                }
                
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin");
                
                // Chỉ admin hoặc người viết phản hồi mới có quyền sửa
                if (isAdmin || reply.MaNguoiDung == currentUserId)
                {
                    // Cập nhật nội dung phản hồi
                    reply.NoiDung = editedContent;
                    reply.DaChinhSua = true; // Đánh dấu đã chỉnh sửa
                    await _context.SaveChangesAsync();
                    
                    return Json(new { 
                        success = true, 
                        message = "Đã cập nhật phản hồi thành công",
                        replyId = replyId,
                        editedContent = editedContent
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Bạn không có quyền sửa phản hồi này" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi sửa phản hồi");
                return Json(new { success = false, message = "Có lỗi xảy ra khi sửa phản hồi" });
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReply(int replyId, int artworkId)
        {
            try
            {
                var reply = await _context.PhanHoiBinhLuans.FindAsync(replyId);
                if (reply == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy phản hồi này" });
                }
                
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin");
                
                // Chỉ admin hoặc người viết phản hồi mới có quyền xóa
                if (isAdmin || reply.MaNguoiDung == currentUserId)
                {
                    var commentId = reply.MaBinhLuan;
                    _context.PhanHoiBinhLuans.Remove(reply);
                    await _context.SaveChangesAsync();
                    
                    // Thông báo xóa phản hồi qua SignalR
                    await _commentHubContext.Clients.Group($"artwork_{artworkId}").SendAsync("ReplyDeleted", replyId, commentId);
                    
                    return Json(new { success = true, message = "Đã xóa phản hồi thành công" });
                }
                else
                {
                    return Json(new { success = false, message = "Bạn không có quyền xóa phản hồi này" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa phản hồi");
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa phản hồi" });
            }
        }
    }
}