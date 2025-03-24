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
        /// Retrieves the total amount of partially cancelled items payments for a given transaction.
        /// </summary>
        /// <param name="transactionId">The unique identifier of the transaction.</param>
        /// <returns>
        /// A task that returns the total partially canceled amount in minor units (e.g., cents).
        /// Returns 0 if there are no partially cancelled payments.
        /// </returns>
        Task<int> GetPartialCancelledAmount(string transactionId);

        /// <summary>
        /// Checks if the partial cancellation feature is enabled in VTEX settings.
        /// </summary>
        /// <returns>
        /// A boolean indicating whether partial cancellation is enabled.
        /// Returns <c>false</c> if the setting is <c>null</c> or in case of any exception.
        /// </returns>
        /// <exception cref="Exception">
        /// Catches any exception that occurs while retrieving settings and logs the error.
        /// </exception>
        Task<bool> isPartialCancellationEnabled();

        /// <summary>
        /// Adds void transaction data to VTEX if valid credentials are available.
        /// </summary>
        /// <param name="transactionId">The transaction ID for which the void data is being added.</param>
        /// <param name="voidResponse">The void response data in JSON format.</param>
        /// <returns>A task representing the asynchronous operation for Adding Transaction data</returns>
        Task AddTransactionVoidData(string transactionId, string voidResponse);

        /// <summary>
        /// Checks whether a partial void has been processed for a given transaction.
        /// </summary>
        /// <param name="transactionId">The unique identifier of the transaction.</param>
        /// <returns>
        /// A boolean indicating whether a partial void has been processed for the transaction.
        /// Returns <c>true</c> if a void response exists; otherwise, returns <c>false</c>.
        /// </returns>
        Task<bool> isPartialVoidDoneForTransaction(string transactionId);
    }
}