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
        /// <returns></returns>
        Task<JObject> GetPaymentCancellationsAsync(string vtexAppKey, string vtexAppToken, string transactionId);

        Task AddTransactionDataAsync(string vtexAppKey, string vtexAppToken, string transactionId, string transactionData);
    }
}
