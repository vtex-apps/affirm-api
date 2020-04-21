namespace Affirm.Services
{
    using Affirm.Models;
    using System.Threading.Tasks;

    public interface IAffirmPaymentService
    {
        Task<CreatePaymentResponse> CreatePaymentAsync(CreatePaymentRequest createPaymentRequest, string publicKey);

        Task<CancelPaymentResponse> CancelPaymentAsync(CancelPaymentRequest cancelPaymentRequest, string publicKey, string privateKey);

        Task<CapturePaymentResponse> CapturePaymentAsync(CapturePaymentRequest capturePaymentRequest, string publicKey, string privateKey);

        Task<RefundPaymentResponse> RefundPaymentAsync(RefundPaymentRequest refundPaymentRequest, string publicKey, string privateKey);

        Task<CreatePaymentRequest> GetCreatePaymentRequestAsync(string paymentIdentifier);

        Task<CreatePaymentResponse> AuthorizeAsync(string paymentIdentifier, string token, string publicKey, string privateKey, string callbackUrl, int amount, string orderId, bool sandboxMode);

        Task<CreatePaymentResponse> ReadChargeAsync(string paymentId, string publicKey, string privateKey, bool sandboxMode);

        Task<VtexSettings> GetSettingsAsync();
    }
}
