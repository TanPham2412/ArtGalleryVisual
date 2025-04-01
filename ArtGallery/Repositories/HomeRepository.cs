using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ArtGallery.Models;
using ArtGallery.Repositories.Interfaces;

namespace ArtGallery.Repositories
{
    public class HomeRepository : IHomeRepository
    {
        private readonly ArtGalleryContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<HomeRepository> _logger;

        public HomeRepository(
            ArtGalleryContext context,
            IWebHostEnvironment webHostEnvironment,
            ILogger<HomeRepository> logger)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public async Task<List<Tranh>> GetRandomArtworksFromFollowing(string userId, int count)
        {
            var followingIds = await _context.TheoDois
                .Where(t => t.MaNguoiTheoDoi == userId)
                .Select(t => t.MaNguoiDuocTheoDoi)
                .ToListAsync();

            return await _context.Tranhs
                .Include(t => t.MaNguoiDungNavigation)
                .Include(t => t.MaTags)
                .Include(t => t.MaTheLoais)
                .Where(t => followingIds.Contains(t.MaNguoiDung))
                .OrderBy(r => Guid.NewGuid())
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<SelectListItem>> GetCategories()
        {
            return await _context.TheLoais
                .Select(c => new SelectListItem
                {
                    Value = c.MaTheLoai.ToString(),
                    Text = c.TenTheLoai
                })
                .ToListAsync();
        }

        public async Task<List<dynamic>> SearchUsers(string query, string currentUserId)
        {
            if (string.IsNullOrEmpty(query))
                return new List<dynamic>();

            return await _context.Users
                .Where(u => u.UserName.Contains(query) || u.TenNguoiDung.Contains(query))
                .Select(u => new
                {
                    u.Id,
                    u.TenNguoiDung,
                    u.UserName,
                    u.AnhDaiDien,
                    SoNguoiTheoDoi = u.TheoDoiMaNguoiDuocTheoDoiNavigations.Count,
                    DaTheoDoi = u.TheoDoiMaNguoiDuocTheoDoiNavigations.Any(t =>
                        t.MaNguoiTheoDoi == currentUserId)
                })
                .Take(5)
                .ToListAsync<dynamic>();
        }

        public async Task<(bool success, string message, int followerCount, bool isFollowing)> ToggleFollow(string currentUserId, string userId)
        {
            if (currentUserId == userId)
            {
                return (false, "Bạn không thể theo dõi chính mình", 0, false);
            }

            var existingFollow = await _context.TheoDois
                .FirstOrDefaultAsync(t => t.MaNguoiTheoDoi == currentUserId &&
                                        t.MaNguoiDuocTheoDoi == userId);

            if (existingFollow != null)
            {
                _context.TheoDois.Remove(existingFollow);
            }
            else
            {
                var newFollow = new TheoDoi
                {
                    MaNguoiTheoDoi = currentUserId,
                    MaNguoiDuocTheoDoi = userId,
                    NgayTheoDoi = DateTime.Now
                };
                await _context.TheoDois.AddAsync(newFollow);
            }

            await _context.SaveChangesAsync();

            var followerCount = await _context.TheoDois
                .CountAsync(t => t.MaNguoiDuocTheoDoi == userId);

            return (true, null, followerCount, existingFollow == null);
        }

        public async Task<(bool success, string message)> AddArtwork(Tranh tranh, IFormFile imageFile, string tagsInput, List<int> selectedCategories, string currentUserId)
        {
            try
            {
                if (string.IsNullOrEmpty(tranh.MaNguoiDung) || tranh.MaNguoiDung != currentUserId)
                {
                    tranh.MaNguoiDung = currentUserId;
                }

                var currentUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == currentUserId.ToString());

                if (currentUser == null)
                {
                    return (false, "Không tìm thấy thông tin người dùng");
                }

                if (imageFile == null || imageFile.Length == 0)
                {
                    return (false, "Vui lòng chọn file ảnh");
                }

                // Tạo thư mục nếu chưa tồn tại
                var userFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products", currentUser.TenNguoiDung);
                Directory.CreateDirectory(userFolder);

                // Lưu file ảnh
                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(imageFile.FileName)}";
                var filePath = Path.Combine(userFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                // Tạo tranh mới với đầy đủ thông tin
                var newTranh = new Tranh
                {
                    TieuDe = tranh.TieuDe,
                    MoTa = tranh.MoTa,
                    Gia = tranh.Gia,
                    SoLuongTon = tranh.SoLuongTon,
                    MaNguoiDung = currentUserId,
                    DuongDanAnh = $"/images/products/{currentUser.TenNguoiDung}/{uniqueFileName}",
                    NgayDang = DateTime.Now,
                    TrangThai = "Đang bán",
                    DaBan = 0
                };

                // Thêm tranh vào context
                await _context.Tranhs.AddAsync(newTranh);
                await _context.SaveChangesAsync();

                // Xử lý tags
                if (!string.IsNullOrEmpty(tagsInput))
                {
                    var tagNames = tagsInput.Split(' ')
                        .Where(t => t.StartsWith("#"))
                        .Select(t => t.TrimStart('#'))
                        .Distinct();

                    foreach (var tagName in tagNames)
                    {
                        var tag = await _context.TheTags
                            .FirstOrDefaultAsync(t => t.TenTag == tagName);

                        if (tag == null)
                        {
                            tag = new TheTag { TenTag = tagName };
                            await _context.TheTags.AddAsync(tag);
                            await _context.SaveChangesAsync();
                        }

                        newTranh.MaTags.Add(tag);
                    }
                }

                // Xử lý thể loại
                if (selectedCategories != null && selectedCategories.Any())
                {
                    var categories = await _context.TheLoais
                        .Where(c => selectedCategories.Contains(c.MaTheLoai))
                        .ToListAsync();

                    foreach (var category in categories)
                    {
                        newTranh.MaTheLoais.Add(category);
                    }
                }

                await _context.SaveChangesAsync();
                return (true, "Đăng tranh thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm tranh");
                return (false, "Có lỗi xảy ra khi thêm tranh");
            }
        }

        public async Task<List<Tranh>> GetRecommendedArtworks()
        {
            try
            {
                return await _context.Tranhs
                    .Include(t => t.MaNguoiDungNavigation)
                    .Include(t => t.MaTags)
                    .Include(t => t.MaTheLoais)
                    .OrderByDescending(t => t.NgayDang)
                    .Take(8)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách tranh đề xuất");
                return new List<Tranh>();
            }
        }

        public async Task<List<Tranh>> GetMostLikedArtworks(int count)
        {
            try
            {
                // Lấy danh sách tranh và đếm số lượt thích
                return await _context.Tranhs
                    .Include(t => t.MaNguoiDungNavigation)
                    .Include(t => t.LuotThiches)
                    .OrderByDescending(t => t.LuotThiches.Count) // Sắp xếp theo số lượt thích
                    .Take(count) // Lấy số lượng theo yêu cầu
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách tranh có nhiều lượt thích nhất");
                return new List<Tranh>();
            }
        }
    }
}
