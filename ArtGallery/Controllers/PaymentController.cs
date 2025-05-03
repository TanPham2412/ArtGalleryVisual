using ArtGallery.Models.VNPAY;
using ArtGallery.Services.VNPAY;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArtGallery.Models;
using System.Linq;

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
        public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model, string DiaChi, string PhoneNumber, string UserId)
        {
            var user = _context.NguoiDungs.FirstOrDefault(u => u.Id == UserId);
            if (user != null)
            {
                user.DiaChi = DiaChi;
                user.PhoneNumber = PhoneNumber;
                _context.SaveChanges();
            }

            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);
            return Redirect(url);
        }




        public IActionResult Index()
        {
            return View();
        }
    }
}
