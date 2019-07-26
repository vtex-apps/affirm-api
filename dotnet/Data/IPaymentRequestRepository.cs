namespace Affirm.Data
{
    using Affirm.Models;
    using System.Threading.Tasks;

    public interface IPaymentRequestRepository
    {
        Task<CreatePaymentRequest> GetPaymentRequestAsync(string paymentIdentifier);

        Task SavePaymentRequestAsync(string paymentIdentifier, CreatePaymentRequest createPaymentRequest);

        Task SetMerchantSettings(MerchantSettings merchantSettings);

        Task<MerchantSettings> GetMerchantSettings();

        Task PostCallbackResponse(string callbackUrl, CreatePaymentResponse createPaymentResponse);
    }
}
