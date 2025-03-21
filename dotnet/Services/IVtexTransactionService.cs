namespace Affirm.Services
{
    using Affirm.Models;
    using System.Threading.Tasks;

    public interface IVtexTransactionService
    {
        /// <summary>
        /// Retrieves the payment cancellation details for a given transaction.
        /// </summary>
        /// <param name="transactionId">The unique identifier of the transaction.</param>
        /// <returns>
        /// A task that returns a <see cref="GetPaymentCancellationResponse"/> object containing 
        /// the list of cancellation requests and actions performed during the cancellation process.
        /// If the request fails, it may return an error response.
        /// </returns>
        Task<GetPaymentCancellationResponse> GetPaymentCancellations(string vtexAppKey, string vtexAppToken, string transactionId);

        /// <summary>
        /// Retrieves the total amount of partially canceled payments for a given transaction.
        /// </summary>
        /// <param name="transactionId">The unique identifier of the transaction.</param>
        /// <returns>
        /// A task that returns the total partially canceled amount in minor units (e.g., cents).
        /// Returns 0 if there are no partially canceled payments.
        /// </returns>
        Task<int> GetPartialCancelledAmount(string transactionId);
        Task<bool> isPartialCancellationEnabled();
        Task AddTransactionVoidData(string transactionId, string voidResponse);
        Task<bool> isPartialVoidDoneForTransaction(string transactionId);
    }
}