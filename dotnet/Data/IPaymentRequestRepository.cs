namespace Affirm.Data
{
    using Affirm.Models;
    using System.Threading.Tasks;

    // An interface which defines what data should be retrieved. 
    // It doesnt make any assumption about how the data should be retrieved.
    public interface IPaymentRequestRepository
    {
        Task<CreatePaymentRequest> GetPaymentRequestAsync(string paymentIdentifier);

        Task SavePaymentRequestAsync(string paymentIdentifier, CreatePaymentRequest createPaymentRequest);

        Task SetMerchantSettings(MerchantSettings merchantSettings);

        Task<MerchantSettings> GetMerchantSettings();

        Task PostCallbackResponse(CreatePaymentRequest createPaymentRequest, CreatePaymentResponse createPaymentResponse);
    }
}
