namespace Affirm.Data
{
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using Affirm.Models;

    public class InMemoryPaymentRequestRepository : IPaymentRequestRepository
    {
        private readonly IDictionary<string, CreatePaymentRequest> _inMemoryDataStore = new Dictionary<string, CreatePaymentRequest>();
        private readonly IDictionary<string, MerchantSettings> _inMemorySettings = new Dictionary<string, MerchantSettings>();

        public InMemoryPaymentRequestRepository()
        {
            
        }

         public Task<CreatePaymentRequest> GetPaymentRequestAsync(string paymentIdentifier)
        {
            if (!this._inMemoryDataStore.TryGetValue(paymentIdentifier, out var paymentRequest))
            {
                return Task.FromResult((CreatePaymentRequest)null);
            }

            return Task.FromResult(paymentRequest);
        }

        public Task SavePaymentRequestAsync(string paymentIdentifier, CreatePaymentRequest createPaymentRequest)
        {
            this._inMemoryDataStore.Remove(paymentIdentifier);
            this._inMemoryDataStore.Add(paymentIdentifier, createPaymentRequest);
            return Task.CompletedTask;
        }

        public Task SetMerchantSettings(MerchantSettings merchantSettings)
        {
            this._inMemorySettings.Remove("settings");
            this._inMemorySettings.Add("settings", merchantSettings);

            return Task.CompletedTask;
        }

        public Task<MerchantSettings> GetMerchantSettings()
        {
            if (!this._inMemorySettings.TryGetValue("settings", out var merchantSettings))
            {
                return Task.FromResult((MerchantSettings)null);
            }

            return Task.FromResult(merchantSettings);
        }

        public Task PostCallbackResponse(CreatePaymentRequest createPaymentRequest, CreatePaymentResponse createPaymentResponse)
        {
            return Task.CompletedTask;
        }
    }
}
