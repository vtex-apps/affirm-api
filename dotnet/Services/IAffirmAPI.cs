namespace Affirm.Services
{
    using Affirm.Models;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// https://docs.affirm.com/Integrate_Affirm/Direct_API/Affirm_API_Reference
    /// </summary>
    public interface IAffirmAPI
    {
        /// <summary>
        /// Charge authorization occurs after a user completes the Affirm checkout flow and returns to the merchant site.
        /// Authorizing the charge generates a charge ID that will be used to reference it moving forward.
        /// You must authorize a charge to fully create it.
        /// A charge is not visible in the Read charge response, nor in the merchant dashboard until you authorize it.
        /// The response to your request will contain the full charge response object, which includes all of the information
        /// submitted when you initialized checkout in addition to the charge and transaction-level identifiers.
        /// The only two pieces of information you need to parse from this response object are amount and the id.
        /// </summary>
        /// <returns></returns>
        Task<JObject> AuthorizeAsync(string publicApiKey, string privateApiKey, string checkoutToken, string orderId);

        /// <summary>
        /// Read the checkout data and current checkout status for a specific checkout attempt.
        /// This is also useful for updating your phone or in-store agent's UI.
        /// </summary>
        /// <returns></returns>
        Task<JObject> ReadAsync(string publicApiKey, string privateApiKey, string checkoutId);

        /// <summary>
        /// Read the charge information, current charge status, and checkout data for one or more charges.
        /// This is useful for updating your records or order management system with current charge states
        /// before performing actions on them. It also allows you to keep your system in sync with Affirm
        /// if your staff manually manages loans in the merchant dashboard.
        /// </summary>
        /// <returns></returns>
        Task<JObject> ReadChargeAsync(string publicApiKey, string privateApiKey, string chargeId);

        /// <summary>
        /// Capture the funds of an authorized charge, similar to capturing a credit card transaction. After capturing a loan:
        /// The customer receives a notice that the charge is captured
        /// The customer's first payment is due in 30 days
        /// Affirm pays the merchants within 2-3 business days
        /// Full or partial refunds can be processed within 120 days
        /// </summary>
        /// <returns></returns>
        Task<JObject> CaptureAsync(string publicApiKey, string privateApiKey, string chargeId, string orderId, decimal amount);

        /// <summary>
        /// Cancel an authorized charge. After voiding a loan:
        /// It is permanently canceled and cannot be re-authorized
        /// The customer receives a notice that the transaction is canceled
        /// </summary>
        /// <returns></returns>
        Task<JObject> VoidAsync(string publicApiKey, string privateApiKey, string chargeId);

        /// <summary>
        /// Refund a charge. (add link to refund info)
        /// </summary>
        /// <returns></returns>
        Task<JObject> RefundAsync(string publicApiKey, string privateApiKey, string chargeId, int amount);

        /// <summary>
        /// Update a charge with new fulfillment or order information, such as shipment tracking number, shipping carrier, or order ID.
        /// </summary>
        /// <returns></returns>
        Task<JObject> UpdateAsync(string publicApiKey, string privateApiKey, string chargeId, string orderId, string customerContactInfo, string shippingCarrier, string trackingNumber);

        Task<JObject> KatapultFundingAsync(string publicApiKey, string privateApiKey, string orderId);
    }
}
