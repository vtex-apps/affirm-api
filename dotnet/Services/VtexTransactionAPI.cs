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
    using Vtex.Api.Context;
    using System.Collections.Generic;

    public class VtexTransactionAPI : IVtexTransactionAPI
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string APPLICATION_JSON = "application/json";
        private readonly IIOServiceContext _context;
        private string vtexAccount;

        public VtexTransactionAPI(IHttpContextAccessor httpContextAccessor, HttpClient httpClient, IIOServiceContext context)
        {
            this._httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            this._httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this._context = context ?? throw new ArgumentNullException(nameof(context));
            this.vtexAccount = httpContextAccessor.HttpContext.Request.Headers[AffirmConstants.Vtex.HEADER_VTEX_ACCOUNT];
        }

        /// <summary>
        /// VTEX Payment API to get the Cancellation activities on the transaction that was previously approved, but not settled. 
        /// It is possible to cancel partially or complete value of the transaction.
        /// </summary>
        /// <returns></returns>
        public async Task<JObject> GetPaymentCancellationsAsync(string vtexAppKey, string vtexAppToken, string transactionId)
        {
            string vtexGetCancellationBaseUrl = $"https://{this.vtexAccount}.{AffirmConstants.Vtex.VtexPaymentBaseUrl}/{AffirmConstants.Transactions}/{transactionId}/{AffirmConstants.Vtex.Cancellations}";
            Console.WriteLine("GetPaymentCancellationsAsync : vtexGetCancellationBaseUrl : " + vtexGetCancellationBaseUrl);
            _context.Vtex.Logger.Info("GetPaymentCancellationsAsync : vtexGetCancellationBaseUrl : " + vtexGetCancellationBaseUrl);

            try
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(vtexGetCancellationBaseUrl)
                };

                request.Headers.Add(AffirmConstants.Vtex.HEADER_VTEX_API_APP_KEY, vtexAppKey);
                request.Headers.Add(AffirmConstants.Vtex.HEADER_VTEX_API_APP_TOKEN, vtexAppToken);

                HttpResponseMessage response = await _httpClient.SendAsync(request);

                string responseContent = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    _context.Vtex.Logger.Error("GetPaymentCancellationsAsync", null, $"VTEX API Error: {response.StatusCode}, Response: {responseContent}");
                    return new JObject { ["error"] = $"VTEX API returned error {response.StatusCode}.", ["details"] = responseContent };
                }

                return JObject.Parse(responseContent);
            }
            catch (HttpRequestException ex)
            {
                _context.Vtex.Logger.Error("GetPaymentCancellationsAsync", null, $"Network error: {ex.Message}");
                Console.WriteLine("GetPaymentCancellationsAsync HttpRequestException : " + ex.StackTrace);
                return new JObject { ["error"] = "Network error while calling VTEX API." };
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("GetPaymentCancellationsAsync", null, $"Unexpected error: {ex.Message}");
                Console.WriteLine("GetPaymentCancellationsAsync Exception : " + ex.StackTrace);
                return new JObject { ["error"] = "An unexpected error occurred." };
            }
        }

        public async Task AddTransactionDataAsync(string vtexAppKey, string vtexAppToken, string transactionId, string transactionData)
        {
            Console.WriteLine("######### AddTransactionDataAsync : transactionId : " + transactionId + " , voidResponse : " + transactionData);
            string vtexAdditionalDataBaseUrl = $"https://{this.vtexAccount}.{AffirmConstants.Vtex.VtexPaymentBaseUrl}/{AffirmConstants.Transactions}/{transactionId}/{AffirmConstants.Vtex.AdditionalData}";
            Console.WriteLine("AddTransactionDataAsync : vtexAdditionalDataBaseUrl : " + vtexAdditionalDataBaseUrl);
            _context.Vtex.Logger.Info("AddTransactionDataAsync : vtexAdditionalDataBaseUrl : " + vtexAdditionalDataBaseUrl);

            //string escapedJsonString = JsonConvert.SerializeObject(transactionData);
            //Console.WriteLine("#%%%%%%% : escapedJsonString : " + escapedJsonString);

            var requestBody = new List<object>
            {
                new
                {
                    name = "voidResponse",
                    value = transactionData
                }
            };

            string requestBodySerial = JsonConvert.SerializeObject(requestBody);

            try
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post, // Changed from GET to POST (GET should not have a body)
                    RequestUri = new Uri(vtexAdditionalDataBaseUrl),
                    Content = new StringContent(
                    requestBodySerial,
                    Encoding.UTF8,
                    "application/json"
                    )
                };

                request.Headers.Add(AffirmConstants.Vtex.HEADER_VTEX_API_APP_KEY, vtexAppKey);
                request.Headers.Add(AffirmConstants.Vtex.HEADER_VTEX_API_APP_TOKEN, vtexAppToken);

                HttpResponseMessage response = await _httpClient.SendAsync(request);

                string responseContent = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    _context.Vtex.Logger.Error("AddTransactionDataAsync", null, $"VTEX API Error: {response.StatusCode}, Response: {responseContent}");
                }
                _context.Vtex.Logger.Info("AddTransactionDataAsync", responseContent);
                Console.WriteLine("AddTransactionDataAsync 11111 : ", responseContent);
            }
            catch (HttpRequestException ex)
            {
                _context.Vtex.Logger.Error("AddTransactionDataAsync", null, $"Network error: {ex.Message}");
                Console.WriteLine("AddTransactionDataAsync HttpRequestException : ", ex.StackTrace);
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("AddTransactionDataAsync", null, $"Unexpected error: {ex.Message}");
                Console.WriteLine("AddTransactionDataAsync Exception : " + ex.StackTrace);
            }
        }
	}
}