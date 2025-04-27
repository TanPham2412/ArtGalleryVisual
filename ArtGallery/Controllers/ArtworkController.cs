using Microsoft.AspNetCore.Mvc;
using ArtGallery.Models;
using ArtGallery.Repositories.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace ArtGallery.Controllers
{
    public class ArtworkController : Controller
    {
        private readonly IArtworkRepository _artworkRepository;
        private readonly ILogger<ArtworkController> _logger;
        private readonly ArtGalleryContext _context;

        public ArtworkController(IArtworkRepository artworkRepository, ILogger<ArtworkController> logger, ArtGalleryContext context)
        {
            _artworkRepository = artworkRepository;
            _logger = logger;
            _context = context;
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
            
            // In thông tin để gỡ lỗi
            _logger.LogInformation($"Delete request: ArtworkID={id}, UserID={currentUserId}, IsAdmin={isAdmin}");
            
            var result = await _artworkRepository.DeleteArtwork(id, currentUserId);

            return Json(new { 
                success = result.success, 
                message = result.message,
                redirectUrl = result.redirectUrl 
            });
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
                var vanthuongPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "stickers", "vanthuong");
                var daisuhuynhPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "stickers", "daisuhuynh");
                
                var vanthuongFiles = new List<string>();
                var daisuhuynhFiles = new List<string>();
                
                if (Directory.Exists(vanthuongPath)) {
                    vanthuongFiles = Directory.GetFiles(vanthuongPath, "*.png")
                        .Select(f => basePath + "vanthuong/" + Path.GetFileName(f))
                        .ToList();
                }
                
                if (Directory.Exists(daisuhuynhPath)) {
                    daisuhuynhFiles = Directory.GetFiles(daisuhuynhPath, "*.png")
                        .Select(f => basePath + "daisuhuynh/" + Path.GetFileName(f))
                        .ToList();
                }
                
                return Json(new { 
                    success = true, 
                    vanthuong = vanthuongFiles, 
                    daisuhuynh = daisuhuynhFiles 
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
        public async Task<IActionResult> AddComment(BinhLuan model, int Rating, IFormFile CommentImage, string Sticker)
        {
            // Thêm log để kiểm tra
            _logger.LogInformation($"AddComment: Ảnh đã upload: {CommentImage?.FileName}, Kích thước: {CommentImage?.Length}");
            
            if (model == null || (string.IsNullOrEmpty(model.NoiDung) && CommentImage == null && string.IsNullOrEmpty(Sticker)))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập nội dung, chọn ảnh hoặc sticker";
                return RedirectToAction("Display", new { id = model.MaTranh });
            }
            
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
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
                
                TempData["SuccessMessage"] = "Bình luận của bạn đã được gửi thành công!";
                return RedirectToAction("Display", new { id = model.MaTranh });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm bình luận");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi gửi bình luận";
                return RedirectToAction("Display", new { id = model.MaTranh });
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReply(int MaBinhLuan, int MaTranh, string NoiDung, IFormFile ReplyImage, string Sticker)
        {
            if (string.IsNullOrEmpty(NoiDung) && ReplyImage == null && string.IsNullOrEmpty(Sticker))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập nội dung, chọn ảnh hoặc sticker";
                return RedirectToAction("Display", new { id = MaTranh });
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
                
                TempData["SuccessMessage"] = "Phản hồi của bạn đã được gửi thành công!";
                return RedirectToAction("Display", new { id = MaTranh });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm phản hồi bình luận");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi gửi phản hồi";
                return RedirectToAction("Display", new { id = MaTranh });
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
        public async Task<IActionResult> EditComment(int commentId, int artworkId, string editedContent)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(editedContent))
                {
                    return Json(new { success = false, message = "Nội dung bình luận không được để trống" });
                }
                
                var comment = await _context.BinhLuans.FindAsync(commentId);
                if (comment == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bình luận này" });
                }
                
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin");
                
                // Chỉ admin hoặc người viết bình luận mới có quyền sửa
                if (isAdmin || comment.MaNguoiDung == currentUserId)
                {
                    // Lưu nội dung cũ trước khi cập nhật
                    var originalContent = comment.NoiDung;
                    
                    // Cập nhật nội dung bình luận
                    comment.NoiDung = editedContent;
                    comment.DaChinhSua = true; // Đánh dấu đã chỉnh sửa
                    await _context.SaveChangesAsync();
                    
                    return Json(new { 
                        success = true, 
                        message = "Đã cập nhật bình luận thành công",
                        commentId = commentId,
                        editedContent = editedContent
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
    }
}