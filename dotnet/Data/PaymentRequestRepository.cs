namespace Affirm.Data
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Affirm.Models;
    using Affirm.Services;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;

    /// <summary>
    /// Concrete implementation of <see cref="IPaymentRequestRepository"/> for persisting data to/from vbase.
    /// </summary>
    public class PaymentRequestRepository : IPaymentRequestRepository
    {
        private const string SETTINGS_NAME = "merchantSettings";
        private const string BUCKET = "paymentRequest";
        private const string HEADER_VTEX_CREDENTIAL = "X-Vtex-Credential";
        private const string HEADER_VTEX_WORKSPACE = "X-Vtex-Workspace";
        private const string HEADER_VTEX_ACCOUNT = "X-Vtex-Account";
        private const string APPLICATION_JSON = "application/json";
        private readonly IVtexEnvironmentVariableProvider _environmentVariableProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly string _applicationName;
        private string AUTHORIZATION_HEADER_NAME;


        public PaymentRequestRepository(IVtexEnvironmentVariableProvider environmentVariableProvider, IHttpContextAccessor httpContextAccessor, HttpClient httpClient)
        {
            this._environmentVariableProvider = environmentVariableProvider ??
                                                throw new ArgumentNullException(nameof(environmentVariableProvider));

            this._httpContextAccessor = httpContextAccessor ??
                                        throw new ArgumentNullException(nameof(httpContextAccessor));

            this._httpClient = httpClient ??
                               throw new ArgumentNullException(nameof(httpClient));

            this._applicationName =
                $"{this._environmentVariableProvider.ApplicationVendor}.{this._environmentVariableProvider.ApplicationName}";

            AUTHORIZATION_HEADER_NAME = "Authorization";
        }


        public async Task<CreatePaymentRequest> GetPaymentRequestAsync(string paymentIdentifier)
        {
            Console.WriteLine($"GetPaymentRequestAsync called with {this._environmentVariableProvider.Account},{this._environmentVariableProvider.Workspace},{this._environmentVariableProvider.ApplicationName},{this._environmentVariableProvider.ApplicationVendor},{this._environmentVariableProvider.Region}");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"http://vbase.{this._environmentVariableProvider.Region}.vtex.io/{this._environmentVariableProvider.Account}/{this._environmentVariableProvider.Workspace}/buckets/{this._applicationName}/{BUCKET}/files/{paymentIdentifier}"),
            };

            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(AUTHORIZATION_HEADER_NAME, authToken);
            }

            var response = await _httpClient.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            // A helper method is in order for this as it does not return the stack trace etc.
            response.EnsureSuccessStatusCode();

            CreatePaymentRequest paymentRequest =  JsonConvert.DeserializeObject<CreatePaymentRequest>(responseContent);
            return paymentRequest;
        }

        public async Task SavePaymentRequestAsync(string paymentIdentifier, CreatePaymentRequest createPaymentRequest)
        {
            if (createPaymentRequest == null)
            {
                createPaymentRequest = new CreatePaymentRequest();
            }

            var jsonSerializedCreatePaymentRequest = JsonConvert.SerializeObject(createPaymentRequest);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri($"http://vbase.{this._environmentVariableProvider.Region}.vtex.io/{this._environmentVariableProvider.Account}/{this._environmentVariableProvider.Workspace}/buckets/{this._applicationName}/{BUCKET}/files/{paymentIdentifier}"),
                Content = new StringContent(jsonSerializedCreatePaymentRequest, Encoding.UTF8, APPLICATION_JSON)
            };

            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(AUTHORIZATION_HEADER_NAME, authToken);
            }

            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();
        }

        public async Task<MerchantSettings> GetMerchantSettings()
        {
            // Load merchant settings
            // 'http://apps.{{region}}.vtex.io/{{account}}/{{workspace}}/apps/{{appName}}/settings'
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                //RequestUri = new Uri($"http://apps.{this._environmentVariableProvider.Region}.vtex.io/{this._environmentVariableProvider.Account}/{this._environmentVariableProvider.Workspace}/apps/{this._applicationName}/settings"),
                RequestUri = new Uri($"http://vbase.{this._environmentVariableProvider.Region}.vtex.io/{this._httpContextAccessor.HttpContext.Request.Headers[HEADER_VTEX_ACCOUNT]}/{this._httpContextAccessor.HttpContext.Request.Headers[HEADER_VTEX_WORKSPACE]}/buckets/{this._applicationName}/{BUCKET}/files/{SETTINGS_NAME}"),
            };

            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(AUTHORIZATION_HEADER_NAME, authToken);
            }

            var response = await _httpClient.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<MerchantSettings>(responseContent);
        }

        public async Task SetMerchantSettings(MerchantSettings merchantSettings)
        {
            if (merchantSettings == null)
            {
                merchantSettings = new MerchantSettings();
            }

            var jsonSerializedMerchantSettings = JsonConvert.SerializeObject(merchantSettings);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri($"http://vbase.{this._environmentVariableProvider.Region}.vtex.io/{this._httpContextAccessor.HttpContext.Request.Headers[HEADER_VTEX_ACCOUNT]}/{this._httpContextAccessor.HttpContext.Request.Headers[HEADER_VTEX_WORKSPACE]}/buckets/{this._applicationName}/{BUCKET}/files/{SETTINGS_NAME}"),
                Content = new StringContent(jsonSerializedMerchantSettings, Encoding.UTF8, APPLICATION_JSON)
            };

            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(AUTHORIZATION_HEADER_NAME, authToken);
            }

            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();
        }

        public async Task PostCallbackResponse(string callbackUrl, CreatePaymentResponse createPaymentResponse)
        {
            HttpResponseMessage response = new HttpResponseMessage();

            // Internal vtex posts must be to http with use https header
            callbackUrl.Replace("https", "http");

            try
            {
                var jsonSerializedPaymentResponse = JsonConvert.SerializeObject(createPaymentResponse);
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(callbackUrl),
                    Content = new StringContent(jsonSerializedPaymentResponse, Encoding.UTF8, APPLICATION_JSON)
                };

                string authToken = this._httpContextAccessor.HttpContext.Request.Headers[HEADER_VTEX_CREDENTIAL];
                if (authToken != null)
                {
                    request.Headers.Add(AUTHORIZATION_HEADER_NAME, authToken);
                }

                response = await _httpClient.SendAsync(request);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PostCallbackResponse {callbackUrl} Error: {ex.Message} InnerException: {ex.InnerException} StackTrace: {ex.StackTrace}");
            }

            response.EnsureSuccessStatusCode();
        }
    }
}
