using ArtGallery.Data;
using ArtGallery.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using ArtGallery.Hubs;

namespace ArtGallery.Controllers
{
    [Authorize]
    public class ReplyController : Controller
    {
        private readonly ArtGalleryContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly ILogger<ReplyController> _logger;
        private readonly IHubContext<CommentHub> _commentHubContext;

        public ReplyController(ArtGalleryContext context, IWebHostEnvironment hostEnvironment, ILogger<ReplyController> logger, IHubContext<CommentHub> commentHubContext)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
            _logger = logger;
            _commentHubContext = commentHubContext;
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int replyId, string content, IFormFile image, string sticker, string originalImage)
        {
            try
            {
                var reply = await _context.PhanHoiBinhLuans.FindAsync(replyId);
                
                if (reply == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy phản hồi" });
                }

                // Kiểm tra người dùng có quyền sửa không
                if (reply.MaNguoiDung != User.Identity.Name)
                {
                    return Json(new { success = false, message = "Bạn không có quyền sửa phản hồi này" });
                }

                // Cập nhật nội dung
                reply.NoiDung = content;
                reply.DaChinhSua = true;

                // Xử lý ảnh mới nếu có
                if (image != null)
                {
                    // Xóa ảnh cũ nếu có
                    if (!string.IsNullOrEmpty(reply.DuongDanAnh))
                    {
                        var oldPath = Path.Combine(_hostEnvironment.WebRootPath, reply.DuongDanAnh.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    // Lưu ảnh mới
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "replies");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(fileStream);
                    }

                    reply.DuongDanAnh = "/uploads/replies/" + uniqueFileName;
                }
                else
                {
                    // Giữ nguyên ảnh cũ nếu không có ảnh mới
                    reply.DuongDanAnh = originalImage;
                }

                // Cập nhật sticker
                reply.Sticker = sticker;

                // Lưu thay đổi
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetReplyInfo(int replyId)
        {
            try
            {
                var reply = await _context.PhanHoiBinhLuans.FindAsync(replyId);
                if (reply == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy phản hồi" });
                }
                
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin");
                
                // Chỉ admin hoặc người viết phản hồi mới có quyền xem thông tin chi tiết
                if (isAdmin || reply.MaNguoiDung == currentUserId)
                {
                    return Json(new
                    {
                        success = true,
                        content = reply.NoiDung,
                        imagePath = reply.DuongDanAnh,
                        sticker = reply.Sticker
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Bạn không có quyền xem thông tin này" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin phản hồi");
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditReply(int replyId, int commentId, int artworkId, string editedContent, 
            IFormFile replyImage, string sticker, bool keepOriginalImage = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(editedContent) && replyImage == null && string.IsNullOrEmpty(sticker))
                {
                    return Json(new { success = false, message = "Vui lòng nhập nội dung, chọn ảnh hoặc sticker" });
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
                    
                    // Cập nhật sticker nếu có
                    reply.Sticker = sticker;
                    
                    // Xử lý upload ảnh mới nếu có
                    if (replyImage != null && replyImage.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "comments");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);
                            
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + replyImage.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await replyImage.CopyToAsync(fileStream);
                        }
                        
                        reply.DuongDanAnh = "/images/comments/" + uniqueFileName;
                    }
                    else if (!keepOriginalImage)
                    {
                        // Xóa ảnh nếu người dùng đã xóa và không upload ảnh mới
                        reply.DuongDanAnh = null;
                    }
                    
                    await _context.SaveChangesAsync();
                    
                    // Lấy thông tin người dùng để gửi qua SignalR
                    var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Id == reply.MaNguoiDung);
                    var comment = await _context.BinhLuans.FirstOrDefaultAsync(c => c.MaBinhLuan == reply.MaBinhLuan);
                    
                    if (comment == null)
                    {
                        return Json(new { success = false, message = "Không tìm thấy bình luận gốc" });
                    }
                    
                    // Tạo đối tượng phản hồi đã cập nhật để gửi qua SignalR
                    var updatedReply = new {
                        id = reply.MaPhanHoi,
                        commentId = reply.MaBinhLuan,
                        content = reply.NoiDung,
                        userId = reply.MaNguoiDung,
                        userName = user?.TenNguoiDung ?? "Người dùng",
                        userAvatar = user?.AnhDaiDien ?? "/images/default-avatar.png",
                        date = reply.NgayPhanHoi,
                        imagePath = reply.DuongDanAnh,
                        sticker = reply.Sticker,
                        isEdited = reply.DaChinhSua,
                        artworkId = artworkId  // Thêm artworkId vào dữ liệu phản hồi
                    };
                    
                    // Gửi thông tin phản hồi đã cập nhật qua SignalR
                    await _commentHubContext.Clients.Group($"artwork_{artworkId}").SendAsync("ReplyEdited", replyId, updatedReply);
                    
                    return Json(new { 
                        success = true, 
                        message = "Đã cập nhật phản hồi thành công",
                        replyId = replyId,
                        editedContent = editedContent,
                        sticker = reply.Sticker,
                        imagePath = reply.DuongDanAnh
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
                    // Xóa file ảnh nếu có
                    if (!string.IsNullOrEmpty(reply.DuongDanAnh))
                    {
                        var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", 
                            reply.DuongDanAnh.TrimStart('/'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }
                    
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