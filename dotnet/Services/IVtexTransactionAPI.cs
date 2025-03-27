namespace Affirm.Services
{
    using Affirm.Models;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// https://developers.vtex.com/docs/api-reference/payment-provider-protocol
    /// </summary>
    public interface IVtexTransactionAPI
    {
        /// <summary>
        /// VTEX Payment API to get the Cancellation activities on the transaction that was previously approved, but not settled. It is possible to cancel partially
        /// or complete value of the transaction.
        /// </summary>
        /// <returns>Task representing the asynchronous operation for getting cancellation done on transaction</returns>
        Task<JObject> GetPaymentCancellationsAsync(string transactionId);

        /// <summary>
        /// Adds void response data to a VTEX transaction's additional data.
        /// </summary>
        /// <param name="transactionId">The transaction ID to which the void response data is added.</param>
        /// <param name="transactionData">The serialized void response data to be stored.</param>
        /// <returns>A Task representing the asynchronous operation for adding data to transaction</returns>
        Task AddTransactionDataAsync(string transactionId, string transactionData);
    }
}
