namespace Affirm.Data
{
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using Affirm.Models;

    public class InMemoryPaymentRequestRepository : IPaymentRequestRepository
    {
        private readonly IDictionary<string, CreatePaymentRequest> _inMemoryDataStore = new Dictionary<string, CreatePaymentRequest>();
        private readonly IDictionary<string, CreatePaymentResponse> _inMemoryDataStoreResponse = new Dictionary<string, CreatePaymentResponse>();
        private readonly IDictionary<string, MerchantSettings> _inMemorySettings = new Dictionary<string, MerchantSettings>();
        private readonly IDictionary<string, AffirmVoidResponse> _inMemoryVoidDataStore = new Dictionary<string, AffirmVoidResponse>();
        private readonly IDictionary<string, AffirmVoidResponse> _inMemoryVoidDataStoreResponse = new Dictionary<string, AffirmVoidResponse>();
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

        public Task SavePaymentResponseAsync(CreatePaymentResponse createPaymentResponse)
        {
            this._inMemoryDataStoreResponse.Remove(createPaymentResponse.paymentId);
            this._inMemoryDataStoreResponse.Add(createPaymentResponse.paymentId, createPaymentResponse);
            return Task.CompletedTask;
        }

        public Task<CreatePaymentResponse> GetPaymentResponseAsync(string paymentId)
        {
            if (!this._inMemoryDataStoreResponse.TryGetValue(paymentId, out var paymentResponse))
            {
                return Task.FromResult((CreatePaymentResponse)null);
            }

            return Task.FromResult(paymentResponse);
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

        public Task PostCallbackResponse(string callbackUrl, CreatePaymentResponse createPaymentResponse)
        {
            return Task.CompletedTask;
        }

        public Task<VtexSettings> GetAppSettings()
        {
            throw new System.NotImplementedException();
        }

        Task IPaymentRequestRepository.SaveVoidResponseAsync(AffirmVoidResponse affirmVoidResponse)
        {
            this._inMemoryVoidDataStoreResponse.Remove(affirmVoidResponse.transactionId);
            this._inMemoryVoidDataStoreResponse.Add(affirmVoidResponse.transactionId, affirmVoidResponse);
            return Task.CompletedTask;
        }

        Task<AffirmVoidResponse> IPaymentRequestRepository.GetVoidResponseAsync(string transactionId)
        {
            if (!this._inMemoryVoidDataStore.TryGetValue(transactionId, out var affirmVoidResponse))
            {
                return Task.FromResult((AffirmVoidResponse)null);
            }
            return Task.FromResult(affirmVoidResponse);
        }
    }
}
