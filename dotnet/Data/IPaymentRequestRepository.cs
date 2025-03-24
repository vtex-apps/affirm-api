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

        /// <summary>
        /// Saves the void response for a transaction asynchronously in VTEX VBase.
        /// </summary>
        /// <param name="affirmVoidResponse">The AffirmVoidResponse object containing transaction details.</param>
        /// <returns>Returns a Task that represents the asynchronous operation.</returns>
        Task SaveVoidResponseAsync(AffirmVoidResponse affirmVoidResponse);

        /// <summary>
        /// Retrieves the void response for a given payment ID from the VTEX VBase storage.
        /// </summary>
        /// <param name="paymentId">The unique identifier of the payment transaction.</param>
        /// <returns>
        /// An <see cref="AffirmVoidResponse"/> object containing the void response details if found;
        /// otherwise, returns <c>null</c> if the response is not found.
        /// </returns>
        Task<AffirmVoidResponse> GetVoidResponseAsync(string paymentIdentifier);
    }
}
