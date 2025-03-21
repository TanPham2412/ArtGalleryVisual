using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ArtGallery.Models;
using ArtGallery.Repositories.Interfaces;

namespace ArtGallery.Repositories
{
    public class ArtworkRepository : IArtworkRepository
    {
        private readonly ArtGalleryContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<ArtworkRepository> _logger;

        public ArtworkRepository(
            ArtGalleryContext context,
            IWebHostEnvironment webHostEnvironment,
            ILogger<ArtworkRepository> logger)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public async Task<(Tranh artwork, List<Tranh> otherWorks)> GetArtworkForDisplay(int id)
        {
            var artwork = await _context.Tranhs
                .Include(t => t.MaNguoiDungNavigation)
                .Include(t => t.MaTheLoais)
                .Include(t => t.MaTags)
                .FirstOrDefaultAsync(t => t.MaTranh == id);

            if (artwork == null)
                return (null, null);

            var otherWorks = await _context.Tranhs
                .Where(t => t.MaNguoiDung == artwork.MaNguoiDung && t.MaTranh != id)
                .OrderBy(r => Guid.NewGuid())
                .Take(3)
                .ToListAsync();

            return (artwork, otherWorks);
        }

        public async Task<(TheLoai category, List<Tranh> artworks)> GetArtworksByCategory(int categoryId)
        {
            var category = await _context.TheLoais.FindAsync(categoryId);
            if (category == null)
                return (null, null);

            var artworks = await _context.Tranhs
                .Include(t => t.MaNguoiDungNavigation)
                .Where(t => t.MaTheLoais.Any(c => c.MaTheLoai == categoryId))
                .ToListAsync();

            return (category, artworks);
        }

        public async Task<(TheTag tag, List<Tranh> artworks)> GetArtworksByTag(int tagId)
        {
            var tag = await _context.TheTags.FindAsync(tagId);
            if (tag == null)
                return (null, null);

            var artworks = await _context.Tranhs
                .Include(t => t.MaNguoiDungNavigation)
                .Where(t => t.MaTags.Any(t => t.MaTag == tagId))
                .ToListAsync();

            return (tag, artworks);
        }

        public async Task<Tranh> GetArtworkForEdit(int id, string currentUserId)
        {
            var artwork = await _context.Tranhs
                .Include(t => t.MaTheLoais)
                .Include(t => t.MaTags)
                .FirstOrDefaultAsync(t => t.MaTranh == id);

            if (artwork == null || artwork.MaNguoiDung != currentUserId)
                return null;

            return artwork;
        }

        public async Task<List<SelectListItem>> GetCategories()
        {
            return await _context.TheLoais
                .Select(c => new SelectListItem { Value = c.MaTheLoai.ToString(), Text = c.TenTheLoai })
                .ToListAsync();
        }

        public async Task<(bool success, string message)> UpdateArtwork(
            Tranh model, IFormFile imageFile, string tagsInput, List<int> selectedCategories, string currentUserId)
        {
            try
            {
                var artwork = await _context.Tranhs
                    .Include(t => t.MaTheLoais)
                    .Include(t => t.MaTags)
                    .Include(t => t.MaNguoiDungNavigation)
                    .FirstOrDefaultAsync(t => t.MaTranh == model.MaTranh);

                if (artwork == null || artwork.MaNguoiDung != currentUserId)
                    return (false, "Không tìm thấy tranh hoặc bạn không có quyền sửa");

                // Cập nhật thông tin cơ bản
                artwork.TieuDe = model.TieuDe;
                artwork.MoTa = model.MoTa;
                artwork.Gia = model.Gia;
                artwork.SoLuongTon = model.SoLuongTon;

                if (imageFile != null && imageFile.Length > 0)
                {
                    var (success, message, imagePath) = await UpdateArtworkImage(artwork, imageFile);
                    if (!success) return (false, message);
                    artwork.DuongDanAnh = imagePath;
                }

                await UpdateCategories(artwork, selectedCategories);
                await UpdateTags(artwork, tagsInput);

                await _context.SaveChangesAsync();
                return (true, "Cập nhật thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật tranh");
                return (false, "Có lỗi xảy ra khi cập nhật tranh");
            }
        }

        public async Task<(bool success, string message, string redirectUrl)> DeleteArtwork(int id, string currentUserId)
        {
            try
            {
                var artwork = await _context.Tranhs
                    .Include(t => t.MaNguoiDungNavigation)
                    .FirstOrDefaultAsync(t => t.MaTranh == id);

                if (artwork == null)
                    return (false, "Không tìm thấy tranh", null);

                if (artwork.MaNguoiDung != currentUserId)
                    return (false, "Bạn không có quyền xóa tranh này", null);

                if (!string.IsNullOrEmpty(artwork.DuongDanAnh))
                {
                    var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, artwork.DuongDanAnh.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Tranhs.Remove(artwork);
                await _context.SaveChangesAsync();

                var redirectUrl = $"/User/Profile/{currentUserId}";
                return (true, "Xóa thành công!", redirectUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa tranh");
                return (false, "Có lỗi xảy ra khi xóa tranh", null);
            }
        }

        private async Task<(bool success, string message, string imagePath)> UpdateArtworkImage(Tranh artwork, IFormFile imageFile)
        {
            try
            {
                var userFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products", artwork.MaNguoiDungNavigation.TenNguoiDung);
                Directory.CreateDirectory(userFolder);

                if (!string.IsNullOrEmpty(artwork.DuongDanAnh))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, artwork.DuongDanAnh.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                var originalFileName = Path.GetFileNameWithoutExtension(imageFile.FileName).Replace("_", "");
                var fileExtension = Path.GetExtension(imageFile.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}{originalFileName}{fileExtension}";
                var filePath = Path.Combine(userFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                return (true, "Cập nhật ảnh thành công", $"/images/products/{artwork.MaNguoiDungNavigation.TenNguoiDung}/{uniqueFileName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật ảnh");
                return (false, "Có lỗi xảy ra khi cập nhật ảnh", null);
            }
        }

        private async Task UpdateCategories(Tranh artwork, List<int> selectedCategories)
        {
            artwork.MaTheLoais.Clear();
            if (selectedCategories != null && selectedCategories.Any())
            {
                var categories = await _context.TheLoais
                    .Where(c => selectedCategories.Contains(c.MaTheLoai))
                    .ToListAsync();
                foreach (var category in categories)
                {
                    artwork.MaTheLoais.Add(category);
                }
            }
        }

        private async Task UpdateTags(Tranh artwork, string tagsInput)
        {
            artwork.MaTags.Clear();
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
                        _context.TheTags.Add(tag);
                        await _context.SaveChangesAsync();
                    }

                    artwork.MaTags.Add(tag);
                }
            }
        }
    }
}
