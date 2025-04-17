namespace ArtGallery.Models.VNPAY
{
    public class PaymentInformationModel
    {
        public string OrderType { get; set; }
        public int Amount { get; set; }
        public string OrderDescription { get; set; }
        public string Name { get; set; }

    }
}
