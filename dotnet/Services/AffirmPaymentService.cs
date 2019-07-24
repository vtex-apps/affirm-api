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

        //public AffirmPaymentService(IPaymentRequestRepository paymentRequestRepository)
        //{
        //    this._paymentRequestRepository = paymentRequestRepository ??
        //                                    throw new ArgumentNullException(nameof(paymentRequestRepository));
        //}

        /// <summary>
        /// Creates a new payment and/or initiates the payment flow.
        /// </summary>
        /// <param name="createPaymentRequest"></param>
        /// <returns></returns>
        public async Task<CreatePaymentResponse> CreatePaymentAsync(CreatePaymentRequest createPaymentRequest, string publicKey)
        {
            CreatePaymentResponse paymentResponse = new CreatePaymentResponse();
            string paymentIdentifier = Guid.NewGuid().ToString();

            // Save the request for later retrieval
            await this._paymentRequestRepository.SavePaymentRequestAsync(paymentIdentifier, createPaymentRequest);
            paymentResponse.paymentId = createPaymentRequest.paymentId;
            paymentResponse.status = "undefined";
            paymentResponse.tid = createPaymentRequest.reference; // Using reference because we don't have an identifier from the provider yet.
            string redirectUrl = "/affirm-payment";
            paymentResponse.paymentUrl = $"{redirectUrl}?g={paymentIdentifier}&k={publicKey}";

            return paymentResponse;
        }

        /// <summary>
        /// Cancels a payment that was not yet approved or captured (settled).
        /// </summary>
        /// <param name="cancelPaymentRequest"></param>
        /// <returns></returns>
        public async Task<CancelPaymentResponse> CancelPaymentAsync(CancelPaymentRequest cancelPaymentRequest, string publicKey, string privateKey, bool isLive)
        {
            IAffirmAPI affirmAPI = new AffirmAPI(_httpContextAccessor, _httpClient, isLive);
            dynamic affirmResponse = await affirmAPI.VoidAsync(publicKey, privateKey, cancelPaymentRequest.paymentId);

            CancelPaymentResponse cancelPaymentResponse = new CancelPaymentResponse
            {
                paymentId = cancelPaymentRequest.paymentId,
                cancellationId = affirmResponse.transaction_id,
                code = affirmResponse.type ?? affirmResponse.Error.Code,
                message = affirmResponse.id ?? affirmResponse.Error.Message,
                requestId = cancelPaymentRequest.requestId
            };

            return cancelPaymentResponse;
        }

        /// <summary>
        /// Captures (settle) a payment that was previously approved.
        /// </summary>
        /// <param name="capturePaymentRequest"></param>
        /// <returns></returns>
        public async Task<CapturePaymentResponse> CapturePaymentAsync(CapturePaymentRequest capturePaymentRequest, string publicKey, string privateKey, bool isLive)
        {
            IAffirmAPI affirmAPI = new AffirmAPI(_httpContextAccessor, _httpClient, isLive);
            dynamic affirmResponse = await affirmAPI.CaptureAsync(publicKey, privateKey, capturePaymentRequest.requestId, capturePaymentRequest.paymentId, string.Empty, string.Empty);

            CapturePaymentResponse capturePaymentResponse = new CapturePaymentResponse
            {
                paymentId = capturePaymentRequest.paymentId,
                settleId = affirmResponse.transaction_id,
                value = affirmResponse.amount == null ? 0m : affirmResponse.amount/100,
                code = affirmResponse.type ?? affirmResponse.Error.Code,
                message = affirmResponse.id ?? affirmResponse.Error.Message,
                requestId = capturePaymentRequest.requestId
            };

            return capturePaymentResponse;
        }

        /// <summary>
        /// Refunds a payment that was previously captured (settled). You can expect partial refunds.
        /// </summary>
        /// <param name="refundPaymentRequest"></param>
        /// <returns></returns>
        public async Task<RefundPaymentResponse> RefundPaymentAsync(RefundPaymentRequest refundPaymentRequest, string publicKey, string privateKey, bool isLive)
        {
            int amount = decimal.ToInt32(refundPaymentRequest.value * 100);

            IAffirmAPI affirmAPI = new AffirmAPI(_httpContextAccessor, _httpClient, isLive);
            dynamic affirmResponse = await affirmAPI.RefundAsync(publicKey, privateKey, refundPaymentRequest.requestId, amount);

            RefundPaymentResponse refundPaymentResponse = new RefundPaymentResponse
            {
                paymentId = refundPaymentRequest.paymentId,
                refundId = affirmResponse.transaction_id,
                value = affirmResponse.amount == null ? 0m : affirmResponse.amount / 100,
                code = affirmResponse.type ?? affirmResponse.Error.Code,
                message = affirmResponse.id ?? affirmResponse.Error.Message,
                requestId = refundPaymentRequest.requestId
            };

            return refundPaymentResponse;
        }

        /// <summary>
        /// Retrieve saved Payment Request
        /// </summary>
        /// <param name="paymentIdentifier"></param>
        /// <returns></returns>
        public async Task<CreatePaymentRequest> GetCreatePaymentRequestAsync(string paymentIdentifier)
        {
            CreatePaymentRequest paymentRequest = await this._paymentRequestRepository.GetPaymentRequestAsync(paymentIdentifier);

            return paymentRequest;
        }

        /// <summary>
        /// After completing the checkout flow and receiving the checkout token, authorize the charge.
        /// Authorizing generates a charge ID that you’ll use to reference the charge moving forward.
        /// You must authorize a charge to fully create it. A charge is not visible in the Read response,
        /// nor in the merchant dashboard until you authorize it.
        /// </summary>
        /// <param name="paymentIdentifier"></param>
        /// <param name="token"></param>
        /// <param name="publicKey"></param>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public async Task<CreatePaymentResponse> AuthorizeAsync(string paymentIdentifier, string token, string publicKey, string privateKey, bool isLive)
        {
            IAffirmAPI affirmAPI = new AffirmAPI(_httpContextAccessor, _httpClient, isLive);
            dynamic affirmResponse = await affirmAPI.AuthorizeAsync(publicKey, privateKey, token, paymentIdentifier);

            string paymentStatus = "denied";
            if (affirmResponse.status != null && affirmResponse.status == AffirmConstants.SuccessResponseCode)
            {
                paymentStatus = "approved";
            }

            CreatePaymentResponse paymentResponse = new CreatePaymentResponse();
            paymentResponse.paymentId = paymentIdentifier;
            paymentResponse.status = paymentStatus;
            paymentResponse.tid = affirmResponse.id ?? null;
            paymentResponse.authorizationId = affirmResponse.id ?? null;
            paymentResponse.code = affirmResponse.status ?? affirmResponse.status_code ?? affirmResponse.Error?.Code;
            paymentResponse.message = affirmResponse.events ?? affirmResponse.message ?? affirmResponse.Error?.Message;

            // Retrieve the original Payment Request from storage
            CreatePaymentRequest createPaymentRequest = await this._paymentRequestRepository.GetPaymentRequestAsync(paymentIdentifier);
            if (createPaymentRequest != null)
            {
                await this._paymentRequestRepository.PostCallbackResponse(createPaymentRequest, paymentResponse);   
            }
            else
            {
                Console.WriteLine($"Could not load request for '{paymentIdentifier}'");
            }

            return paymentResponse;
        }

        public async Task<object> ReadChargeAsync(string paymentId, string publicKey, string privateKey, bool isLive)
        {
            IAffirmAPI affirmAPI = new AffirmAPI(_httpContextAccessor, _httpClient, isLive);
            dynamic affirmResponse = await affirmAPI.ReadChargeAsync(publicKey, privateKey, paymentId);

            return affirmResponse;
        }
    }
}
