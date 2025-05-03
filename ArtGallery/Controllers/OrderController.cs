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
        private readonly INotificationRepository _notificationRepository;

        public OrderController(
            IOrderRepository orderRepository, 
            ILogger<OrderController> logger, 
            UserManager<NguoiDung> userManager, 
            ArtGalleryContext context,
            INotificationRepository notificationRepository)
        {
            _orderRepository = orderRepository;
            _logger = logger;
            _userManager = userManager;
            _context = context;
            _notificationRepository = notificationRepository;
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

                // Lấy thông tin tranh và người bán
                var artwork = await _context.Tranhs
                    .Include(t => t.MaNguoiDungNavigation)
                    .FirstOrDefaultAsync(t => t.MaTranh == maTranh);
                
                if (artwork == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy tác phẩm" });
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
                        
                        // Gửi thông báo cho người bán
                        // Đối với đơn hàng mới (Đã đặt hàng), cần thông báo để người bán xác nhận
                            var buyer = await _userManager.FindByIdAsync(userId);
                            var sellerId = artwork.MaNguoiDung;
                            
                            // Tạo thông báo cho người bán
                            await _notificationRepository.CreateNotification(
                                receiverId: sellerId,
                                senderId: userId,
                                title: "Đơn hàng mới",
                                content: $"{buyer.TenNguoiDung} đã đặt mua tác phẩm {artwork.TieuDe} với số lượng {soLuong}",
                                url: "/Order/History",
                                notificationType: "order",
                                imageUrl: artwork.DuongDanAnh
                            );
                        
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
                        TrangThai = trangThai, // Trạng thái sẽ là "Đã đặt hàng"
                        PhuongThucThanhToan = phuongThucThanhToan
                    };
                    
                    // Thêm vào cơ sở dữ liệu
                    _context.GiaoDiches.Add(giaoDich);
                    
                    // Cập nhật số lượng tranh còn lại
                    if (artwork != null)
                    {
                        artwork.SoLuongTon -= soLuong;
                        _context.Tranhs.Update(artwork);
                    }
                    
                    await _context.SaveChangesAsync();
                    
                    // Gửi thông báo cho người bán khi đặt hàng mới
                        var buyer = await _userManager.FindByIdAsync(userId);
                        var sellerId = artwork.MaNguoiDung;
                        
                        // Tạo thông báo cho người bán
                        await _notificationRepository.CreateNotification(
                            receiverId: sellerId,
                            senderId: userId,
                            title: "Đơn hàng mới",
                            content: $"{buyer.TenNguoiDung} đã đặt mua tác phẩm {artwork.TieuDe} với số lượng {soLuong}",
                            url: "/Order/History",
                            notificationType: "order",
                            imageUrl: artwork.DuongDanAnh
                        );
                    
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

            // Lấy đơn hàng mua của người dùng (không bao gồm những đơn hàng đã ẩn)
            var buyOrders = await _context.GiaoDiches
                .Include(g => g.MaTranhNavigation)
                .Include(g => g.MaNguoiMuaNavigation)
                .Where(g => g.MaNguoiMua == currentUserId && (g.IsHiddenByBuyer == false || g.IsHiddenByBuyer == null))
                .ToListAsync();

            // Lấy đơn hàng bán (người dùng là người tạo ra tranh)
            var sellOrders = await _context.GiaoDiches
                .Include(g => g.MaTranhNavigation)
                .Include(g => g.MaNguoiMuaNavigation)
                .Where(g => g.MaTranhNavigation.MaNguoiDung == currentUserId && 
                       g.MaNguoiMua != currentUserId && 
                       (g.IsHiddenBySeller == false || g.IsHiddenBySeller == null))
                .ToListAsync();

            // Kết hợp danh sách
            var allOrders = buyOrders.Concat(sellOrders).ToList();

            return View(allOrders);
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

                // Lấy thông tin tranh và người bán
                var artwork = await _context.Tranhs
                    .Include(t => t.MaNguoiDungNavigation)
                    .FirstOrDefaultAsync(t => t.MaTranh == maTranh);
                
                if (artwork == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy tác phẩm" });
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

                // Gửi thông báo cho người bán khi có người thêm sản phẩm vào giỏ hàng
                var buyer = await _userManager.FindByIdAsync(userId);
                var sellerId = artwork.MaNguoiDung;
                
                await _notificationRepository.CreateNotification(
                    receiverId: sellerId,
                    senderId: userId,
                    title: "Sản phẩm được thêm vào giỏ hàng",
                    content: $"{buyer.TenNguoiDung} đã thêm tác phẩm {artwork.TieuDe} vào giỏ hàng",
                    url: "/Order/History",
                    notificationType: "cart",
                    imageUrl: artwork.DuongDanAnh
                );

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

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var order = await _context.GiaoDiches
                    .Include(g => g.MaTranhNavigation)
                    .FirstOrDefaultAsync(g => g.MaGiaoDich == orderId);

                if (order == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
                }

                // Kiểm tra quyền cập nhật (cho phép cả người bán và người mua hủy đơn)
                bool hasPermission = false;
                
                // Người bán có quyền thực hiện mọi hành động
                if (order.MaTranhNavigation.MaNguoiDung == userId)
                {
                    hasPermission = true;
                }
                // Người mua chỉ có quyền hủy đơn hàng của mình và đơn đó phải ở trạng thái "Đã đặt hàng"
                else if (order.MaNguoiMua == userId && status == "Đã hủy" && order.TrangThai == "Đã đặt hàng")
                {
                    hasPermission = true;
                }
                // Thêm điều kiện cho người mua xác nhận đã nhận hàng
                else if (order.MaNguoiMua == userId && status == "Đã hoàn thành" && order.TrangThai == "Đã xác nhận")
                {
                    hasPermission = true;
                }

                if (!hasPermission)
                {
                    return Json(new { success = false, message = "Bạn không có quyền cập nhật đơn hàng này" });
                }

                // Cập nhật trạng thái
                order.TrangThai = status;
                _context.GiaoDiches.Update(order);
                await _context.SaveChangesAsync();

                // Gửi thông báo cho người liên quan
                if (status == "Đã hủy")
                {
                    // Nếu người mua hủy, thông báo cho người bán
                    if (order.MaNguoiMua == userId)
                    {
                        var buyer = await _userManager.FindByIdAsync(userId);
                        await _notificationRepository.CreateNotification(
                            receiverId: order.MaTranhNavigation.MaNguoiDung,
                            senderId: userId,
                            title: "Đơn hàng đã bị hủy",
                            content: $"Người mua {buyer.TenNguoiDung} đã hủy đơn hàng #{order.MaGiaoDich}",
                            url: "/Order/History",
                            notificationType: "order",
                            imageUrl: order.MaTranhNavigation.DuongDanAnh
                        );
                    }
                    // Nếu người bán hủy, thông báo cho người mua
                    else
                    {
                        var seller = await _userManager.FindByIdAsync(userId);
                        await _notificationRepository.CreateNotification(
                            receiverId: order.MaNguoiMua,
                            senderId: userId,
                            title: "Đơn hàng đã bị hủy",
                            content: $"Người bán {seller.TenNguoiDung} đã hủy đơn hàng #{order.MaGiaoDich}",
                            url: "/Order/History",
                            notificationType: "order",
                            imageUrl: order.MaTranhNavigation.DuongDanAnh
                        );
                    }
                }
                else
                {
                    // Các thông báo khác giữ nguyên như cũ
                var seller = await _userManager.FindByIdAsync(userId);
                await _notificationRepository.CreateNotification(
                    receiverId: order.MaNguoiMua,
                    senderId: userId,
                    title: "Cập nhật đơn hàng",
                    content: $"Đơn hàng #{order.MaGiaoDich} đã được cập nhật sang trạng thái {status}",
                    url: "/Order/History",
                    notificationType: "order",
                    imageUrl: order.MaTranhNavigation.DuongDanAnh
                );
                }

                return Json(new { success = true, message = $"Đã cập nhật trạng thái đơn hàng thành {status}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteHistory(int orderId)
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

                // Chỉ cho phép ẩn lịch sử đơn hàng đã hoàn thành hoặc đã hủy
                if (order.TrangThai != "Đã hoàn thành" && order.TrangThai != "Đã hủy")
                {
                    return Json(new { success = false, message = "Chỉ có thể ẩn đơn hàng đã hoàn thành hoặc đã hủy" });
                }

                // Thay vì xóa đơn hàng, chỉ đánh dấu là đã ẩn
                order.IsHiddenByBuyer = true;
                _context.GiaoDiches.Update(order);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã ẩn đơn hàng khỏi lịch sử" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> HideSellerHistory(int orderId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                // Tìm đơn hàng mà người dùng hiện tại là người bán
                var order = await _context.GiaoDiches
                    .Include(g => g.MaTranhNavigation)
                    .FirstOrDefaultAsync(g => g.MaGiaoDich == orderId && g.MaTranhNavigation.MaNguoiDung == userId);

                if (order == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
                }

                // Chỉ cho phép ẩn lịch sử đơn hàng đã hoàn thành hoặc đã hủy
                if (order.TrangThai != "Đã hoàn thành" && order.TrangThai != "Đã hủy")
                {
                    return Json(new { success = false, message = "Chỉ có thể ẩn đơn hàng đã hoàn thành hoặc đã hủy" });
                }

                // Đánh dấu là đã ẩn bởi người bán
                order.IsHiddenBySeller = true;
                _context.GiaoDiches.Update(order);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã ẩn đơn hàng khỏi lịch sử" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAllHistory()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                
                // Tìm tất cả đơn hàng đã hoàn thành hoặc đã hủy mà người dùng là người mua
                var completedOrCancelledOrders = await _context.GiaoDiches
                    .Where(g => g.MaNguoiMua == userId && 
                          (g.TrangThai == "Đã hoàn thành" || g.TrangThai == "Đã hủy") &&
                          (g.IsHiddenByBuyer == false || g.IsHiddenByBuyer == null))
                    .ToListAsync();

                if (!completedOrCancelledOrders.Any())
                {
                    return Json(new { success = false, message = "Không có đơn hàng nào để xóa" });
                }

                // Đánh dấu tất cả là đã ẩn
                foreach (var order in completedOrCancelledOrders)
                {
                    order.IsHiddenByBuyer = true;
                }

                _context.GiaoDiches.UpdateRange(completedOrCancelledOrders);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã xóa tất cả lịch sử đơn hàng", count = completedOrCancelledOrders.Count });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAllSellerHistory()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                
                // Tìm tất cả đơn hàng đã hoàn thành hoặc đã hủy mà người dùng là người bán
                var completedOrCancelledOrders = await _context.GiaoDiches
                    .Include(g => g.MaTranhNavigation)
                    .Where(g => g.MaTranhNavigation.MaNguoiDung == userId &&
                          g.MaNguoiMua != userId &&
                          (g.TrangThai == "Đã hoàn thành" || g.TrangThai == "Đã hủy") &&
                          (g.IsHiddenBySeller == false || g.IsHiddenBySeller == null))
                    .ToListAsync();

                if (!completedOrCancelledOrders.Any())
                {
                    return Json(new { success = false, message = "Không có đơn hàng nào để xóa" });
                }

                // Đánh dấu tất cả là đã ẩn
                foreach (var order in completedOrCancelledOrders)
                {
                    order.IsHiddenBySeller = true;
                }

                _context.GiaoDiches.UpdateRange(completedOrCancelledOrders);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã xóa tất cả lịch sử bán hàng", count = completedOrCancelledOrders.Count });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
        [HttpPost]
        public IActionResult OrderCOD(string DiaChi, string PhoneNumber, string UserId)
        {
            var user = _context.NguoiDungs.FirstOrDefault(u => u.Id == UserId);
            if (user != null)
            {
                user.DiaChi = DiaChi;
                user.PhoneNumber = PhoneNumber;
                _context.SaveChanges();
            }

            // Truyền thông tin qua TempData để dùng sau khi redirect
            TempData["DiaChi"] = DiaChi;
            TempData["PhoneNumber"] = PhoneNumber;
            TempData["SuccessMessage"] = "Đơn hàng của bạn đã được xác nhận!";

            return RedirectToAction("OrderCOD"); // Gọi GET /Order/OrderCOD
        }
        [HttpGet]
        public IActionResult OrderCOD()
        {
            var diaChi = TempData["DiaChi"]?.ToString();
            var phone = TempData["PhoneNumber"]?.ToString();
            var message = TempData["SuccessMessage"]?.ToString();

            ViewBag.DiaChi = diaChi;
            ViewBag.PhoneNumber = phone;
            ViewBag.Message = message;

            return View(); // sẽ render ra Views/Order/OrderCOD.cshtml
        }

    }
}