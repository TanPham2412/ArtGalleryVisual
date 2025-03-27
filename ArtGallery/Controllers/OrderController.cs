using Microsoft.AspNetCore.Mvc;
using ArtGallery.Models;
using ArtGallery.Repositories.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ArtGallery.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrderController> _logger;
        private readonly UserManager<NguoiDung> _userManager;
        private readonly ArtGalleryContext _context;

        public OrderController(IOrderRepository orderRepository, ILogger<OrderController> logger, UserManager<NguoiDung> userManager, ArtGalleryContext context)
        {
            _orderRepository = orderRepository;
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Display(int id)
        {
            // Lấy thông tin tranh từ cơ sở dữ liệu
            var artwork = await _context.Tranhs
                .Include(t => t.MaNguoiDungNavigation)
                .FirstOrDefaultAsync(t => t.MaTranh == id);
            
            if (artwork == null)
            {
                return NotFound();
            }
            
            // Gán giá trị cho ViewBag
            ViewBag.Artwork = artwork;
            
            return View(artwork);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int artworkId, int quantity, decimal totalAmount)
        {
            if (quantity <= 0)
            {
                TempData["ErrorMessage"] = "Số lượng phải lớn hơn 0";
                return RedirectToAction("Display", new { id = artworkId });
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var artwork = await _orderRepository.GetArtworkById(artworkId);

            if (artwork == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tranh";
                return RedirectToAction("Index", "Home");
            }

            // Tính lại tổng tiền để kiểm tra
            var calculatedTotal = artwork.Gia * quantity;

            var giaoDich = new GiaoDich
            {
                MaNguoiMua = currentUserId,
                MaTranh = artworkId,
                SoLuong = quantity,
                SoTien = calculatedTotal // Sử dụng giá trị tính toán từ server để đảm bảo tính chính xác
            };

            var result = await _orderRepository.CreateOrder(giaoDich);

            if (result.success)
            {
                TempData["SuccessMessage"] = result.message;
                return RedirectToAction("OrderSuccess", new { id = giaoDich.MaGiaoDich });
            }
            else
            {
                TempData["ErrorMessage"] = result.message;
                return RedirectToAction("Display", new { id = artworkId });
            }
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(int maTranh, int soLuong, decimal tongTien, string trangThai, string phuongThucThanhToan)
        {
            try
            {
                // Lấy thông tin người dùng hiện tại
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "Bạn cần đăng nhập để đặt hàng" });
                }

                // Tạo giao dịch mới
                var giaoDich = new GiaoDich
                {
                    MaNguoiMua = userId,
                    MaTranh = maTranh,
                    SoLuong = soLuong,
                    SoTien = tongTien,
                    NgayMua = DateTime.Now,
                    TrangThai = trangThai,
                    PhuongThucThanhToan = phuongThucThanhToan
                };

                // Thêm vào cơ sở dữ liệu
                _context.GiaoDiches.Add(giaoDich);

                // Cập nhật số lượng tranh còn lại
                var artwork = await _context.Tranhs.FindAsync(maTranh);
                if (artwork != null)
                {
                    artwork.SoLuongTon -= soLuong;
                    _context.Tranhs.Update(artwork);
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // Action hiển thị trang đặt hàng thành công
        public IActionResult OrderSuccess()
        {
            return View();
        }

        public async Task<IActionResult> History()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orders = await _orderRepository.GetOrdersByUserId(currentUserId);
            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int maTranh, int soLuong, decimal tongTien, string trangThai, string phuongThucThanhToan)
        {
            try
            {
                // Lấy thông tin người dùng hiện tại
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "Bạn cần đăng nhập để thêm vào giỏ hàng" });
                }
                
                // Tạo giao dịch mới với trạng thái "Chờ xác nhận"
                var giaoDich = new GiaoDich
                {
                    MaNguoiMua = userId,
                    MaTranh = maTranh,
                    SoLuong = soLuong,
                    SoTien = tongTien,
                    NgayMua = DateTime.Now,
                    TrangThai = trangThai,
                    PhuongThucThanhToan = phuongThucThanhToan
                };
                
                // Thêm vào cơ sở dữ liệu
                _context.GiaoDiches.Add(giaoDich);
                await _context.SaveChangesAsync();
                
                return Json(new { success = true, message = "Đã thêm vào giỏ hàng thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}