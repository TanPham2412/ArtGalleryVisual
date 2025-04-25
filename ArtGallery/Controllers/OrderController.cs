using Microsoft.AspNetCore.Mvc;
using ArtGallery.Models;
using ArtGallery.Repositories.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ArtGallery.Models.VNPAY;

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

        public async Task<IActionResult> Display(int id, int? orderId = null)
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

            // Nếu có orderId, lấy thông tin đơn hàng
            if (orderId.HasValue)
            {
                var userId = _userManager.GetUserId(User);
                var existingOrder = await _context.GiaoDiches
                    .FirstOrDefaultAsync(g => g.MaGiaoDich == orderId.Value && g.MaNguoiMua == userId);

                if (existingOrder != null)
                {
                    ViewBag.ExistingOrder = existingOrder;
                }
            }

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
        public async Task<IActionResult> PlaceOrder(int maTranh, int soLuong, decimal tongTien, string trangThai, string phuongThucThanhToan, int? orderId = null)
        {
            try
            {
                // Lấy thông tin người dùng hiện tại
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "Bạn cần đăng nhập để đặt hàng" });
                }

                // Nếu có orderId (đang xác nhận đơn hàng từ giỏ hàng), cập nhật giao dịch hiện có
                if (orderId.HasValue)
                {
                    var existingOrder = await _context.GiaoDiches.FindAsync(orderId.Value);
                    if (existingOrder != null && existingOrder.MaNguoiMua == userId)
                    {
                        // Cập nhật trạng thái và phương thức thanh toán
                        existingOrder.TrangThai = trangThai;
                        existingOrder.PhuongThucThanhToan = phuongThucThanhToan;
                        
                        _context.GiaoDiches.Update(existingOrder);
                        await _context.SaveChangesAsync();
                        
                        return Json(new { success = true });
                    }
                    
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng cần cập nhật" });
                }
                else
                {
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
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // Action hiển thị trang đặt hàng thành công
        [HttpGet]
        public IActionResult OrderSuccess()
        {
            // Kiểm tra các tham số từ VNPay
            var vnp_TxnRef = Request.Query["vnp_TxnRef"];
            var vnp_TransactionNo = Request.Query["vnp_TransactionNo"];
            var vnp_OrderInfo = Request.Query["vnp_OrderInfo"];
            var vnp_ResponseCode = Request.Query["vnp_ResponseCode"];

            // Nếu không có tham số VNPay, có thể là thanh toán COD
            if (string.IsNullOrEmpty(vnp_TxnRef) ||
                string.IsNullOrEmpty(vnp_TransactionNo) ||
                string.IsNullOrEmpty(vnp_OrderInfo) ||
                string.IsNullOrEmpty(vnp_ResponseCode))
            {
                // Hiển thị view OrderSuccess mà không có model
                return View();
            }

            var paymentResponse = new PaymentResponseModel
            {
                OrderId = vnp_TxnRef,
                TransactionId = vnp_TransactionNo,
                OrderDescription = vnp_OrderInfo,
                VnPayResponseCode = vnp_ResponseCode,
                PaymentMethod = "VNPAY"
            };

            return View(paymentResponse);
        }

        // Thêm action PaymentError nếu cần
        [HttpGet]
        public IActionResult PaymentError()
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

        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int orderId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var order = await _context.GiaoDiches
                    .FirstOrDefaultAsync(g => g.MaGiaoDich == orderId && g.MaNguoiMua == userId);

                if (order == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
                }

                if (order.TrangThai != "Chờ xác nhận")
                {
                    return Json(new { success = false, message = "Chỉ có thể xóa đơn hàng đang chờ xác nhận" });
                }

                // Xóa đơn hàng
                _context.GiaoDiches.Remove(order);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã xóa sản phẩm khỏi giỏ hàng" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}