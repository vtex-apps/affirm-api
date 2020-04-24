namespace Affirm.Models
{
    public class CapturePaymentResponse
    {
        public string paymentId { get; set; }
        public string settleId { get; set; }
        public decimal value { get; set; }
        public string code { get; set; }
        public string message { get; set; }
        public string requestId { get; set; }
    }
}
