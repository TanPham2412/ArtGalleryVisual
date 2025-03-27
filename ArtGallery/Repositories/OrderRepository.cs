using Microsoft.EntityFrameworkCore;
using ArtGallery.Models;
using ArtGallery.Repositories.Interfaces;

namespace ArtGallery.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ArtGalleryContext _context;
        private readonly ILogger<OrderRepository> _logger;

        public OrderRepository(ArtGalleryContext context, ILogger<OrderRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Tranh> GetArtworkById(int id)
        {
            return await _context.Tranhs
                .Include(t => t.MaNguoiDungNavigation)
                .FirstOrDefaultAsync(t => t.MaTranh == id);
        }

        public async Task<(bool success, string message)> CreateOrder(GiaoDich giaoDich)
        {
            try
            {
                // Lấy thông tin tranh
                var artwork = await _context.Tranhs.FindAsync(giaoDich.MaTranh);
                if (artwork == null)
                {
                    return (false, "Không tìm thấy tranh");
                }

                // Kiểm tra số lượng tồn
                if (artwork.SoLuongTon < giaoDich.SoLuong)
                {
                    return (false, "Số lượng tranh trong kho không đủ");
                }

                // Cập nhật số lượng tồn và số lượng đã bán
                artwork.SoLuongTon -= giaoDich.SoLuong;
                artwork.DaBan += giaoDich.SoLuong;

                // Thêm giao dịch mới
                giaoDich.NgayMua = DateTime.Now;
                await _context.GiaoDiches.AddAsync(giaoDich);
                await _context.SaveChangesAsync();

                return (true, "Đặt hàng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo giao dịch mới");
                return (false, "Có lỗi xảy ra khi đặt hàng");
            }
        }

        public async Task<List<GiaoDich>> GetOrdersByUserId(string userId)
        {
            return await _context.GiaoDiches
                .Include(g => g.MaTranhNavigation)
                .Include(g => g.MaNguoiMuaNavigation)
                .Where(g => g.MaNguoiMua == userId)
                .OrderByDescending(g => g.NgayMua)
                .ToListAsync();
        }
    }
}