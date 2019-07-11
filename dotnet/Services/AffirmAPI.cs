namespace Affirm.Services
{
    using System;
    using System.Collections.Generic;
    using System.Net;
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

        public AffirmAPI(bool isLive)
        {
            string prefix = isLive ? "api" : "sandbox";
            this.affirmBaseUrl = $"http://{prefix}.affirm.com/api/v2";
        }

        public AffirmAPI(IHttpContextAccessor httpContextAccessor, HttpClient httpClient, bool isLive)
        {
            this._httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            this._httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            string prefix = isLive ? "api" : "sandbox";
            this.affirmBaseUrl = $"https://{prefix}.affirm.com/api/v2";
        }

        public async Task<JObject> AuthorizeAsync(string publicApiKey, string privateApiKey, string checkoutToken, string orderId)
        {
            AffirmAuthorizeRequest authRequest = new AffirmAuthorizeRequest
            {
                checkout_token = checkoutToken,
                order_id = orderId
            };

            var jsonSerializedRequest = JsonConvert.SerializeObject(authRequest);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{affirmBaseUrl}/charges"),
                Content = new StringContent(jsonSerializedRequest, Encoding.UTF8, APPLICATION_JSON)
            };

            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{publicApiKey}:{privateApiKey}")));

            var response = await _httpClient.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();

            return JObject.Parse(responseContent);
        }

        public async Task<JObject> CaptureAsync(string publicApiKey, string privateApiKey, string chargeId, string orderId, string shippingCarrier, string shippingConfirmation)
        {
            AffirmCaptureRequest captureRequest = new AffirmCaptureRequest
            {
                order_id = orderId,
                shipping_carrier = shippingCarrier,
                shipping_confirmation = shippingConfirmation
            };

            var jsonSerializedRequest = JsonConvert.SerializeObject(captureRequest);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{affirmBaseUrl}/charges/{chargeId}/capture"),
                Content = new StringContent(jsonSerializedRequest, Encoding.UTF8, APPLICATION_JSON)
            };

            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{publicApiKey}:{privateApiKey}")));

            var response = await _httpClient.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();

            return JObject.Parse(responseContent);
        }

        public async Task<JObject> ReadAsync(string publicApiKey, string privateApiKey, string checkoutId)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{affirmBaseUrl}/checkout/{checkoutId}")
            };

            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{publicApiKey}:{privateApiKey}")));
            //var base64authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{ publicApiKey}:{ privateApiKey}"));
            //request.Headers.TryAddWithoutValidation("Authorization", $"Basic {base64authorization}");

            var response = await _httpClient.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();
            JObject parsedResponse = null;
            if (response.IsSuccessStatusCode)
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

            return parsedResponse;
        }

        public async Task<JObject> ReadChargeAsync(string publicApiKey, string privateApiKey, string chargeId)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{affirmBaseUrl}/charges/{chargeId}/")
            };

            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{publicApiKey}:{privateApiKey}")));

            var response = await _httpClient.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();

            return JObject.Parse(responseContent);
        }

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
                RequestUri = new Uri($"{affirmBaseUrl}/charges/{chargeId}/refund"),
                Content = new StringContent(jsonSerializedRequest, Encoding.UTF8, APPLICATION_JSON)
            };

            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{publicApiKey}:{privateApiKey}")));

            var response = await _httpClient.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();

            return JObject.Parse(responseContent);
        }

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
                RequestUri = new Uri($"{affirmBaseUrl}/charges/{chargeId}/update"),
                Content = new StringContent(jsonSerializedRequest, Encoding.UTF8, APPLICATION_JSON)
            };

            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{publicApiKey}:{privateApiKey}")));

            var response = await _httpClient.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();

            return JObject.Parse(responseContent);
        }

        public async Task<JObject> VoidAsync(string publicApiKey, string privateApiKey, string chargeId)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{affirmBaseUrl}/charges/{chargeId}/update")
            };

            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{publicApiKey}:{privateApiKey}")));

            var response = await _httpClient.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();

            return JObject.Parse(responseContent);
        }
    }
}
