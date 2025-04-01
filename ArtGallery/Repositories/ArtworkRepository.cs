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

        public async Task<Tranh> GetArtworkForEdit(int id, string currentUserId, bool isAdmin = false)
        {
            var artwork = await _context.Tranhs
                .Include(t => t.MaTheLoais)
                .Include(t => t.MaTags)
                .Include(t => t.MaNguoiDungNavigation)
                .FirstOrDefaultAsync(t => t.MaTranh == id);

            // Cho phép admin hoặc chủ sở hữu sửa tranh
            if (artwork == null || (artwork.MaNguoiDung != currentUserId && !isAdmin))
                return null;

            _logger.LogInformation($"Artwork Owner ID: {artwork.MaNguoiDung}, Current User ID: {currentUserId}, IsAdmin: {isAdmin}");

            return artwork;
        }

        public async Task<List<SelectListItem>> GetCategories()
        {
            return await _context.TheLoais
                .Select(c => new SelectListItem { Value = c.MaTheLoai.ToString(), Text = c.TenTheLoai })
                .ToListAsync();
        }

        public async Task<(bool success, string message)> UpdateArtwork(
            Tranh model, IFormFile imageFile, string tagsInput, List<int> selectedCategories, string currentUserId, bool isAdmin = false)
        {
            try
            {
                var artwork = await _context.Tranhs
                    .Include(t => t.MaTheLoais)
                    .Include(t => t.MaTags)
                    .Include(t => t.MaNguoiDungNavigation)
                    .FirstOrDefaultAsync(t => t.MaTranh == model.MaTranh);

                // Cho phép admin hoặc chủ sở hữu sửa tranh
                if (artwork == null || (artwork.MaNguoiDung != currentUserId && !isAdmin))
                    return (false, "Không tìm thấy tranh hoặc bạn không có quyền sửa");

                _logger.LogInformation($"Artwork Owner ID: {artwork.MaNguoiDung}, Current User ID: {currentUserId}, IsAdmin: {isAdmin}");

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
                // Kiểm tra quyền admin
                var isAdmin = await _context.UserRoles
                    .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur, r })
                    .AnyAsync(x => x.ur.UserId == currentUserId && x.r.Name == "Admin");
                
                _logger.LogInformation($"DeleteArtwork - ArtworkID: {id}, UserID: {currentUserId}, IsAdmin: {isAdmin}");
                
                var artwork = await _context.Tranhs
                    .Include(t => t.MaTags)
                    .Include(t => t.MaTheLoais)
                    .FirstOrDefaultAsync(t => t.MaTranh == id);

                if (artwork == null)
                {
                    _logger.LogWarning($"Artwork not found - ID: {id}");
                    return (false, "Không tìm thấy tranh", null);
                }

                // Cho phép admin hoặc chủ sở hữu xóa tranh
                if (artwork.MaNguoiDung != currentUserId && !isAdmin)
                {
                    _logger.LogWarning($"Permission denied - ArtworkID: {id}, OwnerID: {artwork.MaNguoiDung}, CurrentUserID: {currentUserId}");
                    return (false, "Bạn không có quyền xóa tranh này", null);
                }

                // Xóa file ảnh
                if (!string.IsNullOrEmpty(artwork.DuongDanAnh))
                {
                    var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, artwork.DuongDanAnh.TrimStart('/'));
                    _logger.LogInformation($"Image path: {imagePath}");
                    
                    if (System.IO.File.Exists(imagePath))
                    {
                        try
                        {
                            System.IO.File.Delete(imagePath);
                            _logger.LogInformation($"Deleted image file: {imagePath}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error deleting image file: {imagePath}");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Image file not found: {imagePath}");
                    }
                }

                // Xóa liên kết với tags và thể loại
                artwork.MaTags.Clear();
                artwork.MaTheLoais.Clear();
                
                // Xóa tranh
                _context.Tranhs.Remove(artwork);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Artwork deleted successfully - ID: {id}");

                // Chuyển hướng về trang admin nếu là admin
                var redirectUrl = isAdmin ? "/Artwork/Admin" : $"/User/Profile/{currentUserId}";
                return (true, "Xóa thành công!", redirectUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in DeleteArtwork - ArtworkID: {id}");
                return (false, "Có lỗi xảy ra khi xóa tranh: " + ex.Message, null);
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

        public async Task<List<Tranh>> GetAllArtworks()
        {
            try
            {
                return await _context.Tranhs
                    .Include(t => t.MaNguoiDungNavigation)
                    .Include(t => t.MaTheLoais)
                    .Include(t => t.MaTags)
                    .OrderByDescending(t => t.NgayDang)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy tất cả tranh");
                return new List<Tranh>();
            }
        }

        public async Task<List<Tranh>> GetFilteredArtworks(string searchString, string sortOrder)
        {
            try
            {
                // Lấy tất cả tranh
                var query = _context.Tranhs
                    .Include(t => t.MaNguoiDungNavigation)
                    .Include(t => t.MaTheLoais)
                    .Include(t => t.MaTags)
                    .AsQueryable();
                
                // Tìm kiếm nếu có chuỗi tìm kiếm
                if (!string.IsNullOrEmpty(searchString))
                {
                    searchString = searchString.ToLower();
                    query = query.Where(a => 
                        a.TieuDe.ToLower().Contains(searchString) ||
                        a.MaNguoiDungNavigation.TenNguoiDung.ToLower().Contains(searchString) ||
                        a.MaTheLoais.Any(tl => tl.TenTheLoai.ToLower().Contains(searchString)) || // Tìm theo tên thể loại
                        a.Gia.ToString().Contains(searchString) ||
                        a.SoLuongTon.ToString().Contains(searchString)
                    );
                }
                
                // Sắp xếp danh sách artworks theo sortOrder
                var artworks = await query.ToListAsync();
                
                // Mặc định sắp xếp theo ID giảm dần nếu không có sắp xếp
                if (string.IsNullOrEmpty(sortOrder))
                {
                    sortOrder = "id_desc"; 
                }
                
                switch (sortOrder)
                {
                    case "id_asc":
                        artworks = artworks.OrderBy(a => a.MaTranh).ToList();
                        break;
                    case "id_desc":
                        artworks = artworks.OrderByDescending(a => a.MaTranh).ToList();
                        break;
                    case "title_asc":
                        artworks = artworks.OrderBy(a => a.TieuDe).ToList();
                        break;
                    case "title_desc":
                        artworks = artworks.OrderByDescending(a => a.TieuDe).ToList();
                        break;
                    case "artist_asc":
                        artworks = artworks.OrderBy(a => a.MaNguoiDungNavigation.TenNguoiDung).ToList();
                        break;
                    case "artist_desc":
                        artworks = artworks.OrderByDescending(a => a.MaNguoiDungNavigation.TenNguoiDung).ToList();
                        break;
                    case "category_asc":
                        // Sắp xếp theo tên thể loại đầu tiên (nếu có)
                        artworks = artworks.OrderBy(a => a.MaTheLoais.Any() ? a.MaTheLoais.First().TenTheLoai : "").ToList();
                        break;
                    case "category_desc":
                        // Sắp xếp theo tên thể loại đầu tiên (nếu có) giảm dần
                        artworks = artworks.OrderByDescending(a => a.MaTheLoais.Any() ? a.MaTheLoais.First().TenTheLoai : "").ToList();
                        break;
                    case "price_asc":
                        artworks = artworks.OrderBy(a => a.Gia).ToList();
                        break;
                    case "price_desc":
                        artworks = artworks.OrderByDescending(a => a.Gia).ToList();
                        break;
                    case "quantity_asc":
                        artworks = artworks.OrderBy(a => a.SoLuongTon).ToList();
                        break;
                    case "quantity_desc":
                        artworks = artworks.OrderByDescending(a => a.SoLuongTon).ToList();
                        break;
                    case "date_asc":
                        artworks = artworks.OrderBy(a => a.NgayDang).ToList();
                        break;
                    case "date_desc":
                        artworks = artworks.OrderByDescending(a => a.NgayDang).ToList();
                        break;
                    default:
                        artworks = artworks.OrderByDescending(a => a.MaTranh).ToList();
                        break;
                }
                
                return artworks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lọc và sắp xếp tranh");
                return new List<Tranh>();
            }
        }

        public async Task<(bool success, bool liked, string message)> ToggleLike(int artworkId, string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return (false, false, "Vui lòng đăng nhập để thích tác phẩm");
                }

                var existingLike = await _context.LuotThiches
                    .FirstOrDefaultAsync(lt => lt.MaTranh == artworkId && lt.MaNguoiDung == userId);

                if (existingLike != null)
                {
                    // Nếu đã thích, xóa lượt thích
                    _context.LuotThiches.Remove(existingLike);
                    await _context.SaveChangesAsync();
                    return (true, false, "Đã hủy yêu thích");
                }
                else
                {
                    // Nếu chưa thích, thêm lượt thích mới
                    var luotThich = new LuotThich
                    {
                        MaTranh = artworkId,
                        MaNguoiDung = userId,
                        NgayThich = DateTime.Now
                    };
                    _context.LuotThiches.Add(luotThich);
                    await _context.SaveChangesAsync();
                    return (true, true, "Đã thêm vào yêu thích");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thay đổi trạng thái yêu thích");
                return (false, false, "Có lỗi xảy ra");
            }
        }

        public List<Tranh> ApplySorting(List<Tranh> artworks, string sortOrder)
        {
            // Mặc định sắp xếp theo ID giảm dần
            if (string.IsNullOrEmpty(sortOrder))
            {
                sortOrder = "id_desc"; 
            }
            
            switch (sortOrder)
            {
                case "id_asc":
                    return artworks.OrderBy(a => a.MaTranh).ToList();
                case "id_desc":
                    return artworks.OrderByDescending(a => a.MaTranh).ToList();
                case "title_asc":
                    return artworks.OrderBy(a => a.TieuDe).ToList();
                case "title_desc":
                    return artworks.OrderByDescending(a => a.TieuDe).ToList();
                case "artist_asc":
                    return artworks.OrderBy(a => a.MaNguoiDungNavigation.TenNguoiDung).ToList();
                case "artist_desc":
                    return artworks.OrderByDescending(a => a.MaNguoiDungNavigation.TenNguoiDung).ToList();
                case "category_asc":
                    return artworks.OrderBy(a => a.MaTheLoais.Any() ? a.MaTheLoais.First().TenTheLoai : "").ToList();
                case "category_desc":
                    return artworks.OrderByDescending(a => a.MaTheLoais.Any() ? a.MaTheLoais.First().TenTheLoai : "").ToList();
                case "price_asc":
                    return artworks.OrderBy(a => a.Gia).ToList();
                case "price_desc":
                    return artworks.OrderByDescending(a => a.Gia).ToList();
                case "quantity_asc":
                    return artworks.OrderBy(a => a.SoLuongTon).ToList();
                case "quantity_desc":
                    return artworks.OrderByDescending(a => a.SoLuongTon).ToList();
                case "date_asc":
                    return artworks.OrderBy(a => a.NgayDang).ToList();
                case "date_desc":
                    return artworks.OrderByDescending(a => a.NgayDang).ToList();
                default:
                    return artworks.OrderByDescending(a => a.MaTranh).ToList();
            }
        }
    }
}
