using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArtGallery.Models;

namespace ArtGallery.Controllers
{
    public class SearchController : Controller
    {
        private readonly ArtGalleryContext _context;
        private readonly ILogger<SearchController> _logger;

        public SearchController(ArtGalleryContext context, ILogger<SearchController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> SearchAll(string query)
        {
            if (string.IsNullOrEmpty(query) || query.Length < 2)
            {
                return Json(new { users = new object[0], artworks = new object[0] });
            }

            try
            {
                query = query.Trim().ToLower();

                // Tìm tranh trước - ưu tiên tìm theo tên tranh
                var artworks = await _context.Tranhs
                    .Where(t => t.TieuDe.ToLower().Contains(query))
                    .Include(t => t.MaNguoiDungNavigation)
                    .Take(5)
                    .Select(t => new 
                    {
                        maTranh = t.MaTranh,
                        tieuDe = t.TieuDe,
                        gia = t.Gia,
                        giaGoc = t.Gia * 1.1m,
                        duongDanAnh = t.DuongDanAnh,
                        soLuongTon = t.SoLuongTon,
                        nguoiDung = t.MaNguoiDungNavigation.TenNguoiDung
                    })
                    .ToListAsync();

                // Tìm người dùng
                var users = await _context.Users
                    .Where(u => (u.TenNguoiDung != null && u.TenNguoiDung.ToLower().Contains(query)) || 
                                (u.UserName != null && u.UserName.ToLower().Contains(query)))
                    .Take(3)
                    .Select(u => new
                    {
                        id = u.Id,
                        tenNguoiDung = u.TenNguoiDung,
                        tenDangNhap = u.UserName,
                        anhDaiDien = u.AnhDaiDien
                    })
                    .ToListAsync();

                _logger.LogInformation("Tìm kiếm với từ khóa: {Query}, kết quả: {ArtworksCount} tranh, {UsersCount} người dùng", 
                    query, artworks.Count, users.Count);

                return Json(new { users, artworks });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm với từ khóa: {Query}", query);
                return Json(new { users = new object[0], artworks = new object[0] });
            }
        }
    }
} 