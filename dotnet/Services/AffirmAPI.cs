namespace Affirm.Services
{
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml;
    using Affirm.Models;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class AffirmAPI : IAffirmAPI
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private string affirmBaseUrl;
        private const string APPLICATION_JSON = "application/json";
        private const string HEADER_VTEX_CREDENTIAL = "X-Vtex-Credential";

        public AffirmAPI(IHttpContextAccessor httpContextAccessor, HttpClient httpClient, bool isLive)
        {
            this._httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            this._httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            string prefix = isLive ? "api" : "sandbox";
            this.affirmBaseUrl = $"http://{prefix}.{AffirmConstants.AffirmUrlStub}/{AffirmConstants.AffirmApiVersion}";
        }

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
        public async Task<JObject> AuthorizeAsync(string publicApiKey, string privateApiKey, string checkoutToken, string orderId)
        {
            AffirmAuthorizeRequest authRequest = new AffirmAuthorizeRequest
            {
                transaction_id = checkoutToken,
                order_id = orderId
            };

            var jsonSerializedRequest = JsonConvert.SerializeObject(authRequest);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{affirmBaseUrl}/{AffirmConstants.Transactions}"),
                Content = new StringContent(jsonSerializedRequest, Encoding.UTF8, APPLICATION_JSON)
            };

            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{publicApiKey}:{privateApiKey}")));
            request.Headers.Add("X-Vtex-Use-Https", "true");
            request.Headers.Add("Proxy-Authorization", _httpContextAccessor.HttpContext.Request.Headers[HEADER_VTEX_CREDENTIAL].ToString());

            var response = await _httpClient.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();

            return ParseAffirmResponse(response, responseContent);
        }

        /// <summary>
        /// Capture the funds of an authorized charge, similar to capturing a credit card transaction. After capturing a loan:
        /// The customer receives a notice that the charge is captured
        /// The customer's first payment is due in 30 days
        /// Affirm pays the merchants within 2-3 business days
        /// Full or partial refunds can be processed within 120 days
        /// </summary>
        /// <returns></returns>
        public async Task<JObject> CaptureAsync(string publicApiKey, string privateApiKey, string chargeId, string orderId, decimal amount)
        {
            int amountInPennies = (int)(amount * 100);
            AffirmCaptureRequest captureRequest = new AffirmCaptureRequest
            {
                order_id = orderId,
                amount = amountInPennies
            };

            var jsonSerializedRequest = JsonConvert.SerializeObject(captureRequest);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{affirmBaseUrl}/{AffirmConstants.Transactions}/{chargeId}/{AffirmConstants.Capture}"),
                Content = new StringContent(jsonSerializedRequest, Encoding.UTF8, APPLICATION_JSON)
            };

            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{publicApiKey}:{privateApiKey}")));
            request.Headers.Add("X-Vtex-Use-Https", "true");
            request.Headers.Add("Proxy-Authorization", _httpContextAccessor.HttpContext.Request.Headers[HEADER_VTEX_CREDENTIAL].ToString());

            var response = await _httpClient.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();

            return ParseAffirmResponse(response, responseContent);
        }

        /// <summary>
        /// Read the checkout data and current checkout status for a specific checkout attempt.
        /// This is also useful for updating your phone or in-store agent's UI.
        /// </summary>
        /// <returns></returns>
        public async Task<JObject> ReadAsync(string publicApiKey, string privateApiKey, string checkoutId)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{affirmBaseUrl}/checkout/{checkoutId}")
            };

            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{publicApiKey}:{privateApiKey}")));
            request.Headers.Add("X-Vtex-Use-Https", "true");
            request.Headers.Add("Proxy-Authorization", _httpContextAccessor.HttpContext.Request.Headers[HEADER_VTEX_CREDENTIAL].ToString());

            var response = await _httpClient.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();

            return ParseAffirmResponse(response, responseContent);
        }

        /// <summary>
        /// Read the charge information, current charge status, and checkout data for one or more charges.
        /// This is useful for updating your records or order management system with current charge states
        /// before performing actions on them. It also allows you to keep your system in sync with Affirm
        /// if your staff manually manages loans in the merchant dashboard.
        /// </summary>
        /// <returns></returns>
        public async Task<JObject> ReadChargeAsync(string publicApiKey, string privateApiKey, string chargeId)
        {
            //Console.WriteLine($"Get to '{affirmBaseUrl}/{AffirmConstants.Transactions}/{chargeId}'");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{affirmBaseUrl}/{AffirmConstants.Transactions}/{chargeId}")
            };

            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{publicApiKey}:{privateApiKey}")));
            request.Headers.Add("X-Vtex-Use-Https", "true");
            request.Headers.Add("Proxy-Authorization", _httpContextAccessor.HttpContext.Request.Headers[HEADER_VTEX_CREDENTIAL].ToString());

            var response = await _httpClient.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();

            return ParseAffirmResponse(response, responseContent);
        }

        /// <summary>
        /// Refund a charge. (add link to refund info)
        /// </summary>
        /// <returns></returns>
        public async Task<JObject> RefundAsync(string publicApiKey, string privateApiKey, string chargeId, int amount)
        {
            AffirmRefundRequest refundRequest = new AffirmRefundRequest
            {
                amount = amount
            };

            var jsonSerializedRequest = JsonConvert.SerializeObject(refundRequest);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{affirmBaseUrl}/{AffirmConstants.Transactions}/{chargeId}/{AffirmConstants.Refund}"),
                Content = new StringContent(jsonSerializedRequest, Encoding.UTF8, APPLICATION_JSON)
            };

            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{publicApiKey}:{privateApiKey}")));
            request.Headers.Add("X-Vtex-Use-Https", "true");
            request.Headers.Add("Proxy-Authorization", _httpContextAccessor.HttpContext.Request.Headers[HEADER_VTEX_CREDENTIAL].ToString());

            var response = await _httpClient.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();

            return ParseAffirmResponse(response, responseContent);
        }

        /// <summary>
        /// Update a charge with new fulfillment or order information, such as shipment tracking number, shipping carrier, or order ID.
        /// </summary>
        /// <returns></returns>
        public async Task<JObject> UpdateAsync(string publicApiKey, string privateApiKey, string chargeId, string orderId, string customerContactInfo, string shippingCarrier, string trackingNumber)
        {
            AffirmUpdateRequest updateRequest = new AffirmUpdateRequest
            {
                order_id = orderId,
                shipping = customerContactInfo,
                shipping_carrier = shippingCarrier,
                shipping_confirmation = trackingNumber
            };

            var jsonSerializedRequest = JsonConvert.SerializeObject(updateRequest);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{affirmBaseUrl}/{AffirmConstants.Transactions}/{chargeId}/{AffirmConstants.Update}"),
                Content = new StringContent(jsonSerializedRequest, Encoding.UTF8, APPLICATION_JSON)
            };

            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{publicApiKey}:{privateApiKey}")));
            request.Headers.Add("X-Vtex-Use-Https", "true");
            request.Headers.Add("Proxy-Authorization", _httpContextAccessor.HttpContext.Request.Headers[HEADER_VTEX_CREDENTIAL].ToString());

            var response = await _httpClient.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();

            return ParseAffirmResponse(response, responseContent);
        }

        /// <summary>
        /// Cancel an authorized charge. After voiding a loan:
        /// It is permanently canceled and cannot be re-authorized
        /// The customer receives a notice that the transaction is canceled
        /// </summary>
        /// <returns></returns>
        public async Task<JObject> VoidAsync(string publicApiKey, string privateApiKey, string chargeId)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{affirmBaseUrl}/{AffirmConstants.Transactions}/{chargeId}/{AffirmConstants.Void}")
            };

            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{publicApiKey}:{privateApiKey}")));
            request.Headers.Add("X-Vtex-Use-Https", "true");
            request.Headers.Add("Proxy-Authorization", _httpContextAccessor.HttpContext.Request.Headers[HEADER_VTEX_CREDENTIAL].ToString());

            var response = await _httpClient.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();

            return ParseAffirmResponse(response, responseContent);
        }

        private JObject ParseAffirmResponse(HttpResponseMessage httpResponse, string responseContent)
        {
            JObject parsedResponse = null;
            if (httpResponse.Content.Headers.ContentType.MediaType == APPLICATION_JSON)
            {
                try
                {
                    parsedResponse = JObject.Parse(responseContent);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing success response: {ex.Message}");
                    Console.WriteLine($"Response: {responseContent}");
                }
            }
            else
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(responseContent);
                    string json = JsonConvert.SerializeXmlNode(doc);
                    parsedResponse = JObject.Parse(json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing failure response: {ex.Message}");
                    Console.WriteLine($"Response: {responseContent}");
                }
            }

            Console.WriteLine($"Parsed Response = {parsedResponse}");

            return parsedResponse;
        }
    }
}
