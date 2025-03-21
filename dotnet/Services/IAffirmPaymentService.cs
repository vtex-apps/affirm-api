namespace Affirm.Services
{
    using Affirm.Models;
    using System.Threading.Tasks;

    public interface IAffirmPaymentService
    {
        Task<CreatePaymentResponse> CreatePayment(CreatePaymentRequest createPaymentRequest, string publicKey);

        Task<CancelPaymentResponse> CancelPayment(CancelPaymentRequest cancelPaymentRequest, string publicKey, string privateKey);

        Task<CapturePaymentResponse> CapturePayment(CapturePaymentRequest capturePaymentRequest, string publicKey, string privateKey);

        Task<AffirmVoidResponse> VoidPayment(CapturePaymentRequest capturePaymentRequest, string publicKey, string privateKey, int voidAmount);

        Task<RefundPaymentResponse> RefundPayment(RefundPaymentRequest refundPaymentRequest, string publicKey, string privateKey);

        Task<CreatePaymentRequest> GetCreatePaymentRequest(string paymentIdentifier);

        Task<CreatePaymentResponse> Authorize(string paymentIdentifier, string token, string publicKey, string privateKey, string callbackUrl, int amount, string orderId, bool sandboxMode, string transactionId);

        Task<CreatePaymentResponse> ReadCharge(string paymentId, string publicKey, string privateKey, bool sandboxMode);

        Task<VtexSettings> GetSettings();
    }
}
