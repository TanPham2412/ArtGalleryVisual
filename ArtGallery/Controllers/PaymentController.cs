using ArtGallery.Models.VNPAY;
using ArtGallery.Services.VNPAY;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArtGallery.Models;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace ArtGallery.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IVnPayService _vnPayService;
        private readonly ArtGalleryContext _context;

        public PaymentController(IVnPayService vnPayService, ArtGalleryContext context)
        {
            _vnPayService = vnPayService;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePaymentUrlVnpay(string OrderType, decimal amount, string OrderDescription, int MaTranh, int SoLuong, int? OrderId = null)
        {
            // Lưu thông tin form vào session để sử dụng sau
            var formData = new Dictionary<string, string>
            {
                { "MaTranh", MaTranh.ToString() },
                { "SoLuong", SoLuong.ToString() },
                { "Amount", amount.ToString() }
            };
            
            if (OrderId.HasValue)
        {
                formData.Add("OrderId", OrderId.Value.ToString());
            }
            
            // Lưu vào session
            HttpContext.Session.SetString("VNPayFormData", JsonSerializer.Serialize(formData));
            
            var orderInfo = "VNPAY";
            
            // Thêm thông tin quan trọng vào OrderInfo để sử dụng khi callback
            if (OrderId.HasValue)
            {
                orderInfo = $"VNPAY+{OrderId}+{MaTranh}+{SoLuong}";
            }
            else
            {
                orderInfo = $"VNPAY+0+{MaTranh}+{SoLuong}";
            }
            
            // Tạo model thay vì truyền nhiều tham số
            var paymentModel = new PaymentInformationModel
            {
                Amount = (int)(amount), // Chuyển đổi sang số nguyên, nhân 100 để giữ chính xác
                OrderDescription = orderInfo,
                OrderType = OrderType
            };
            
            // Gọi phương thức với đúng tham số
            var vnpUrl = _vnPayService.CreatePaymentUrl(paymentModel, HttpContext);
            
            // Chuyển hướng đến trang thanh toán VNPay
            return Redirect(vnpUrl);
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
