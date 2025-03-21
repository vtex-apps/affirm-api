namespace Affirm.Data
{
    using Affirm.Models;
    using System.Threading.Tasks;

    public interface IPaymentRequestRepository
    {
        Task<CreatePaymentRequest> GetPaymentRequestAsync(string paymentIdentifier);

        Task SavePaymentResponseAsync(CreatePaymentResponse createPaymentResponse);

        Task<CreatePaymentResponse> GetPaymentResponseAsync(string paymentId);

        Task SavePaymentRequestAsync(string paymentIdentifier, CreatePaymentRequest createPaymentRequest);

        Task SetMerchantSettings(MerchantSettings merchantSettings);

        Task<MerchantSettings> GetMerchantSettings();

        Task PostCallbackResponse(string callbackUrl, CreatePaymentResponse createPaymentResponse);

        Task<VtexSettings> GetAppSettings();

        Task SaveVoidResponseAsync(AffirmVoidResponse affirmVoidResponse);
        
        Task<AffirmVoidResponse> GetVoidResponseAsync(string paymentIdentifier);
    }
}
