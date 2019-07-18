namespace UnitTests.Fakes
{
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using Affirm.Data;
    using Affirm.Models;

    public class FakePaymentRepository : IPaymentRequestRepository
    {
        public Task<MerchantSettings> GetMerchantSettings()
        {
            throw new System.NotImplementedException();
        }

        public Task<CreatePaymentRequest> GetPaymentRequestAsync(string paymentIdentifier)
        {
            throw new System.NotImplementedException();
        }

        public Task PostCallbackResponse(CreatePaymentRequest createPaymentRequest, CreatePaymentResponse createPaymentResponse)
        {
            throw new System.NotImplementedException();
        }

        public Task SavePaymentRequestAsync(string paymentIdentifier, CreatePaymentRequest createPaymentRequest)
        {
            throw new System.NotImplementedException();
        }

        public Task SetMerchantSettings(MerchantSettings merchantSettings)
        {
            throw new System.NotImplementedException();
        }
    }
}
