using ArtGallery.Models.VNPAY;
using ArtGallery.Services.VNPAY;
using Microsoft.AspNetCore.Mvc;

namespace ArtGallery.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IVnPayService _vnPayService;
        public PaymentController(IVnPayService vnPayService)
        {

            _vnPayService = vnPayService;
        }

        public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model)
        {
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);

            return Redirect(url);
        }


        public IActionResult Index()
        {
            return View();
        }
    }
}
