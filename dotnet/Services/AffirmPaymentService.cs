namespace Affirm.Services
{
    using Affirm.Data;
    using Affirm.Models;
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class AffirmPaymentService : IAffirmPaymentService
    {
        private readonly IPaymentRequestRepository _paymentRequestRepository;
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AffirmPaymentService(IPaymentRequestRepository paymentRequestRepository, IHttpContextAccessor httpContextAccessor, HttpClient httpClient)
        {
            this._paymentRequestRepository = paymentRequestRepository ??
                                            throw new ArgumentNullException(nameof(paymentRequestRepository));

            this._httpContextAccessor = httpContextAccessor ??
                                        throw new ArgumentNullException(nameof(httpContextAccessor));

            this._httpClient = httpClient ??
                               throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<CreatePaymentResponse> CreatePaymentAsync(CreatePaymentRequest createPaymentRequest)
        {
            CreatePaymentResponse paymentResponse = new CreatePaymentResponse();
            string paymentIdentifier = Guid.NewGuid().ToString();

            // Save the request for later retrieval
            await this._paymentRequestRepository.SavePaymentRequestAsync(paymentIdentifier, createPaymentRequest);
            paymentResponse.paymentId = createPaymentRequest.paymentId;
            paymentResponse.status = "undefined";
            paymentResponse.tid = createPaymentRequest.reference; // Using reference because we don't have an identifier from the provider yet.
            string redirectUrl = "/affirm-payment";
            paymentResponse.paymentUrl = $"{redirectUrl}?g={paymentIdentifier}";

            return paymentResponse;
        }

        /// <summary>
        /// Making function async to support future API integration
        /// </summary>
        /// <param name="cancelPaymentRequest"></param>
        /// <returns></returns>
        public async Task<CancelPaymentResponse> CancelPaymentAsync(CancelPaymentRequest cancelPaymentRequest, string publicKey, string privateKey)
        {
            bool isLive = true;
            string[] publicKeyArray = publicKey.Split('|');
            publicKey = publicKeyArray[0];
            if (publicKeyArray.Length > 1)
            {
                isLive = !publicKeyArray[1].Equals("test");
            }

            AffirmAPI affirmAPI = new AffirmAPI(_httpContextAccessor, _httpClient, isLive);
            dynamic affirmResponse = await affirmAPI.VoidAsync(publicKey, privateKey, cancelPaymentRequest.paymentId);

            CancelPaymentResponse cancelPaymentResponse = new CancelPaymentResponse
            {
                paymentId = cancelPaymentRequest.paymentId,
                cancellationId = affirmResponse.transaction_id,
                code = affirmResponse.type,
                message = affirmResponse.id,
                requestId = cancelPaymentRequest.requestId
            };

            return cancelPaymentResponse;
        }

        /// <summary>
        /// Making function async to support future API integration
        /// </summary>
        /// <param name="capturePaymentRequest"></param>
        /// <returns></returns>
        public async Task<CapturePaymentResponse> CapturePaymentAsync(CapturePaymentRequest capturePaymentRequest, string publicKey, string privateKey)
        {
            bool isLive = true;
            string[] publicKeyArray = publicKey.Split('|');
            publicKey = publicKeyArray[0];
            if (publicKeyArray.Length > 1)
            {
                isLive = !publicKeyArray[1].Equals("test");
            }

            AffirmAPI affirmAPI = new AffirmAPI(_httpContextAccessor, _httpClient, isLive);
            dynamic affirmResponse = await affirmAPI.CaptureAsync(publicKey, privateKey, capturePaymentRequest.requestId, capturePaymentRequest.paymentId, string.Empty, string.Empty);

            CapturePaymentResponse capturePaymentResponse = new CapturePaymentResponse
            {
                paymentId = capturePaymentRequest.paymentId,
                settleId = affirmResponse.transaction_id,
                value = affirmResponse.amount/100,
                code = affirmResponse.type,
                message = affirmResponse.id,
                requestId = capturePaymentRequest.requestId
            };

            return capturePaymentResponse;
        }

        /// <summary>
        /// Making function async to support future API integration
        /// </summary>
        /// <param name="refundPaymentRequest"></param>
        /// <returns></returns>
        public async Task<RefundPaymentResponse> RefundPaymentAsync(RefundPaymentRequest refundPaymentRequest, string publicKey, string privateKey)
        {
            bool isLive = true;
            string[] publicKeyArray = publicKey.Split('|');
            publicKey = publicKeyArray[0];
            if (publicKeyArray.Length > 1)
            {
                isLive = !publicKeyArray[1].Equals("test");
            }

            int amount = decimal.ToInt32(refundPaymentRequest.value * 100);

            AffirmAPI affirmAPI = new AffirmAPI(_httpContextAccessor, _httpClient, isLive);
            dynamic affirmResponse = await affirmAPI.RefundAsync(publicKey, privateKey, refundPaymentRequest.requestId, amount);

            RefundPaymentResponse refundPaymentResponse = new RefundPaymentResponse
            {
                paymentId = refundPaymentRequest.paymentId,
                refundId = affirmResponse.transaction_id,
                value = affirmResponse.amount / 100,
                code = affirmResponse.type,
                message = affirmResponse.id,
                requestId = refundPaymentRequest.requestId
            };

            return refundPaymentResponse;
        }

        public async Task<CreatePaymentRequest> GetCreatePaymentRequest(string paymentIdentifier)
        {
            CreatePaymentRequest paymentRequest = await this._paymentRequestRepository.GetPaymentRequestAsync(paymentIdentifier);

            return paymentRequest;
        }

        public async Task<CreatePaymentResponse> AuthorizeAsync(string paymentIdentifier, string token, string publicKey, string privateKey)
        {
            bool isLive = true;
            string[] publicKeyArray = publicKey.Split('|');
            publicKey = publicKeyArray[0];
            if (publicKeyArray.Length > 1)
            {
                isLive = !publicKeyArray[1].Equals("test");
            }

            AffirmAPI affirmAPI = new AffirmAPI(_httpContextAccessor, _httpClient, isLive);
            dynamic affirmResponse = await affirmAPI.AuthorizeAsync(publicKey, privateKey, token, paymentIdentifier);
            int amount = affirmResponse.amount;

            // Retrieve the original Payment Request from storage
            CreatePaymentRequest createPaymentRequest = await this._paymentRequestRepository.GetPaymentRequestAsync(paymentIdentifier);

            CreatePaymentResponse paymentResponse = new CreatePaymentResponse();
            paymentResponse.paymentId = createPaymentRequest.paymentId;
            string paymentStatus = "denied";
            if(amount > 0)
            {
                paymentStatus = "approved";
            }

            paymentResponse.status = paymentStatus;
            paymentResponse.tid = affirmResponse.id;

            paymentResponse.message = affirmResponse.events;

            await this._paymentRequestRepository.PostCallbackResponse(createPaymentRequest, paymentResponse);

            return paymentResponse;
        }
    }
}
