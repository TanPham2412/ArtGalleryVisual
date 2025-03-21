using Microsoft.EntityFrameworkCore;
using ArtGallery.Models;
using ArtGallery.Repositories.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace ArtGallery.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ArtGalleryContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(
            ArtGalleryContext context,
            IWebHostEnvironment webHostEnvironment,
            ILogger<UserRepository> logger)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public async Task<(NguoiDung user, int followersCount, int followingCount)?> GetUserProfile(string userId)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Tranhs)
                    .Include(u => u.Media)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return null;
                }

                var followersCount = await _context.TheoDois
                    .CountAsync(t => t.MaNguoiDuocTheoDoi == userId);

                var followingCount = await _context.TheoDois
                    .CountAsync(t => t.MaNguoiTheoDoi == userId);

                return (user, followersCount, followingCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin profile người dùng {UserId}", userId);
                throw;
            }
        }

        public async Task<(bool success, string message)> UpdateProfile(NguoiDung model, IFormFile profileImage, IFormFile coverImage, List<string> LoaiMedia, List<string> DuongDan)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Media)
                    .FirstOrDefaultAsync(u => u.Id == model.Id);

                if (user == null)
                    return (false, "Không tìm thấy người dùng");

                // Xử lý cover image
                if (coverImage != null && coverImage.Length > 0)
                {
                    try
                    {
                        // Tạo thư mục nếu chưa tồn tại
                        var userFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "authors", "coverimages", user.UserName);
                        if (!Directory.Exists(userFolder))
                        {
                            Directory.CreateDirectory(userFolder);
                        }

                        // Xóa ảnh cũ
                        if (!string.IsNullOrEmpty(user.CoverImage))
                        {
                            var oldImagePath = Path.Combine(userFolder, user.CoverImage);
                            if (File.Exists(oldImagePath))
                            {
                                File.Delete(oldImagePath);
                            }
                        }

                        // Tạo tên file mới
                        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(coverImage.FileName)}";
                        var filePath = Path.Combine(userFolder, uniqueFileName);

                        // Lưu file
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await coverImage.CopyToAsync(stream);
                        }

                        // Cập nhật đường dẫn trong database
                        user.CoverImage = uniqueFileName;
                        _logger.LogInformation($"Đã lưu ảnh bìa: {uniqueFileName}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Lỗi khi lưu ảnh bìa: {ex.Message}");
                        return (false, "Có lỗi khi lưu ảnh bìa");
                    }
                }

                // Xử lý profile image
                if (profileImage != null && profileImage.Length > 0)
                {
                    try
                    {
                        // Tạo thư mục cho người dùng nếu chưa tồn tại
                        var userFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "authors", "avatars", user.UserName);
                        Directory.CreateDirectory(userFolder);

                        // Xóa ảnh cũ nếu có
                        if (!string.IsNullOrEmpty(user.AnhDaiDien))
                        {
                            var oldImagePath = Path.Combine(userFolder, user.AnhDaiDien);
                            if (File.Exists(oldImagePath))
                            {
                                File.Delete(oldImagePath);
                            }
                        }

                        // Tạo tên file mới và lưu
                        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(profileImage.FileName)}";
                        var filePath = Path.Combine(userFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await profileImage.CopyToAsync(stream);
                        }

                        user.AnhDaiDien = uniqueFileName;
                        _logger.LogInformation($"Đã lưu ảnh đại diện: {uniqueFileName}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Lỗi khi lưu ảnh đại diện: {ex.Message}");
                        return (false, "Có lỗi khi lưu ảnh đại diện");
                    }
                }

                // Cập nhật thông tin người dùng
                user.TenNguoiDung = model.TenNguoiDung;
                user.MoTa = model.MoTa;
                user.GioiTinh = model.GioiTinh;
                user.DiaChi = model.DiaChi;
                user.NgaySinh = model.NgaySinh;
                user.HienThiGioiTinh = model.HienThiGioiTinh;
                user.HienThiDiaChi = model.HienThiDiaChi;
                user.HienThiNgaySinh = model.HienThiNgaySinh;
                user.HienThiNamSinh = model.HienThiNamSinh;

                // Cập nhật social media
                // Xóa tất cả media cũ
                _context.Media.RemoveRange(user.Media);

                // Validate loại media trước khi thêm
                var validMediaTypes = new[] { "X", "Facebook", "Instagram", "Tiktok", "Website" };

                if (LoaiMedia != null && DuongDan != null)
                {
                    for (int i = 0; i < LoaiMedia.Count; i++)
                    {
                        if (i < DuongDan.Count && !string.IsNullOrWhiteSpace(DuongDan[i]))
                        {
                            // Kiểm tra loại media có hợp lệ không
                            if (!validMediaTypes.Contains(LoaiMedia[i]))
                            {
                                return (false, $"Loại media '{LoaiMedia[i]}' không hợp lệ");
                            }

                            var media = new Medium
                            {
                                MaNguoiDung = user.Id,
                                LoaiMedia = LoaiMedia[i],
                                DuongDan = DuongDan[i]
                            };

                            // Thêm prefix URL nếu chưa có
                            if (!media.DuongDan.StartsWith("http://") && !media.DuongDan.StartsWith("https://"))
                            {
                                switch (LoaiMedia[i])
                                {
                                    case "X":
                                        if (!media.DuongDan.Contains("x.com"))
                                            break;
                                        media.DuongDan = $"https://x.com/{media.DuongDan.TrimStart('@')}";
                                        break;
                                    case "Facebook":
                                        if (media.DuongDan.Contains("facebook.com"))
                                            break;
                                        media.DuongDan = $"https://facebook.com/{media.DuongDan}";
                                        break;
                                    case "Instagram":
                                        if (media.DuongDan.Contains("instagram.com"))
                                            break;
                                        media.DuongDan = $"https://instagram.com/{media.DuongDan.TrimStart('@')}";
                                        break;
                                    case "Tiktok":
                                        if (media.DuongDan.Contains("tiktok.com"))
                                            break;
                                        media.DuongDan = $"https://tiktok.com/@{media.DuongDan.TrimStart('@')}";
                                        break;
                                    default:
                                        if (!media.DuongDan.StartsWith("http"))
                                        {
                                            media.DuongDan = $"https://{media.DuongDan}";
                                        }
                                        break;
                                }
                            }

                            _context.Media.Add(media);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return (true, "Cập nhật thông tin thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật profile người dùng {UserId}", model.Id);
                return (false, $"Có lỗi xảy ra: {ex.Message}");
            }
        }
    }
}