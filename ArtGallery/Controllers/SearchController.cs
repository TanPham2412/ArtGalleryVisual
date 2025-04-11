using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArtGallery.Models;
using ArtGallery.ViewModels;
using System.Security.Claims;

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

        public async Task<IActionResult> Index(string q, string category = "Top", string sortBy = "newest")
        {
            try
            {
                // Xử lý trường hợp tìm kiếm toàn bộ
                bool isShowAll = string.IsNullOrEmpty(q) || sortBy == "all";
                string searchQuery = isShowAll ? string.Empty : q?.Trim().ToLower();
                
                var viewModel = new SearchViewModel
                {
                    Query = searchQuery,
                    Category = category,
                    SortBy = sortBy
                };

                // Lấy tất cả thể loại
                viewModel.Categories = await _context.TheLoais.ToListAsync();

                if (category == "Artists")
                {
                    var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var follows = await _context.TheoDois.Where(f => f.MaNguoiTheoDoi == currentUserId).ToListAsync();

                    viewModel = new SearchViewModel(currentUserId, follows)
                    {
                        Query = searchQuery,
                        Category = category,
                        SortBy = sortBy
                    };

                    var artistsQuery = _context.Users
                        .Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id &&
                                   ur.RoleId == _context.Roles.FirstOrDefault(r => r.Name == "Artists").Id));
                                
                    // Chỉ áp dụng tìm kiếm nếu không phải hiển thị tất cả
                    if (!isShowAll)
                    {
                        artistsQuery = artistsQuery.Where(u => u.TenNguoiDung.ToLower().Contains(searchQuery) ||
                                                              u.UserName.ToLower().Contains(searchQuery));
                    }
                    
                    viewModel.Artists = await artistsQuery
                        .Include(u => u.Tranhs)
                        .ThenInclude(t => t.LuotThiches)
                        .Include(u => u.TheoDoiMaNguoiDuocTheoDoiNavigations)
                        .ToListAsync();

                    viewModel.Artworks = new List<Tranh>(); // Khởi tạo list rỗng
                    return View("Index", viewModel); // Sử dụng view Index thay vì Artists
                }
                else
                {
                    // Tìm kiếm artwork như cũ
                    viewModel.Artists = new List<NguoiDung>();
                    var query = _context.Tranhs
                        .Include(t => t.MaNguoiDungNavigation)
                        .Include(t => t.LuotThiches)
                        .Include(t => t.MaTheLoais)
                        .AsQueryable();

                    // Chỉ áp dụng tìm kiếm nếu không phải hiển thị tất cả
                    if (!isShowAll)
                    {
                        query = query.Where(t => t.TieuDe.ToLower().Contains(searchQuery) ||
                                              (t.MoTa != null && t.MoTa.ToLower().Contains(searchQuery)));
                    }

                    // Lọc theo danh mục
                    if (category != "Top" && category != "All")
                    {
                        query = query.Where(t => t.MaTheLoais.Any(tl => tl.TenTheLoai == category));
                    }

                    // Sắp xếp
                    switch (sortBy)
                    {
                        case "newest":
                            query = query.OrderByDescending(t => t.NgayDang);
                            break;
                        case "oldest":
                            query = query.OrderBy(t => t.NgayDang);
                            break;
                        case "all":
                            // Không cần sắp xếp nếu là "Tất cả"
                            break;
                        default:
                            query = query.OrderByDescending(t => t.NgayDang);
                            break;
                    }

                    // Nếu chọn Top, ưu tiên sắp xếp theo lượt thích
                    if (category == "Top")
                    {
                        query = query.OrderByDescending(t => t.LuotThiches.Count);
                    }

                    viewModel.Artworks = await query.ToListAsync();
                    return View("Index", viewModel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm với từ khóa: {Query}", q);
                TempData["ErrorMessage"] = "Đã xảy ra lỗi trong quá trình tìm kiếm. Vui lòng thử lại sau.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFollow(string artistId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Log để debug
            _logger.LogInformation($"ToggleFollow được gọi với artistId={artistId}, currentUserId={currentUserId}");
            
            if (string.IsNullOrEmpty(currentUserId))
                return Json(new { success = false, message = "Vui lòng đăng nhập để thực hiện chức năng này" });

            try
            {
                var follow = await _context.TheoDois
                    .FirstOrDefaultAsync(f => f.MaNguoiTheoDoi == currentUserId && f.MaNguoiDuocTheoDoi == artistId);

                if (follow == null)
                {
                    // Thêm theo dõi mới
                    follow = new TheoDoi
                    {
                        MaNguoiTheoDoi = currentUserId,
                        MaNguoiDuocTheoDoi = artistId,
                        NgayTheoDoi = DateTime.Now
                    };
                    _context.TheoDois.Add(follow);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Đã thêm theo dõi: {currentUserId} -> {artistId}");
                    return Json(new { success = true, following = true });
                }
                else
                {
                    // Hủy theo dõi
                    _context.TheoDois.Remove(follow);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Đã hủy theo dõi: {currentUserId} -> {artistId}");
                    return Json(new { success = true, following = false });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thực hiện theo dõi");
                return Json(new { success = false, message = "Có lỗi xảy ra, vui lòng thử lại sau" });
            }
        }
    }
} 