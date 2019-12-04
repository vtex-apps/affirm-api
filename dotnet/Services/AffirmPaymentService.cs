namespace Affirm.Services
{
    using Affirm.Data;
    using Affirm.Models;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
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
            // paymentResponse.tid = createPaymentRequest.reference; // Using reference because we don't have an identifier from the provider yet.
            string redirectUrl = "/affirm-payment";
            paymentResponse.paymentUrl = $"{redirectUrl}?g={paymentIdentifier}&k={publicKey}";

            // Load delay settings
            VtexSettings vtexSettings = await this._paymentRequestRepository.GetAppSettings();
            if (vtexSettings != null)
            {
                if (!string.IsNullOrEmpty(vtexSettings.delayInterval))
                {
                    int multiple = 1;
                    switch (vtexSettings.delayInterval)
                    {
                        case "Minutes":
                            multiple = 60;
                            break;
                        case "Hours":
                            multiple = 60 * 60;
                            break;
                        case "Days":
                            multiple = 60 * 60 * 24;
                            break;
                    }

                    paymentResponse.delayToAutoSettle = vtexSettings.delayToAutoSettle * multiple;
                    paymentResponse.delayToAutoSettleAfterAntifraud = vtexSettings.delayToAutoSettleAfterAntifraud * multiple;
                    paymentResponse.delayToCancel = vtexSettings.delayToCancel * multiple;
                }
            }

            return paymentResponse;
        }

        /// <summary>
        /// Cancels a payment that was not yet approved or captured (settled).
        /// </summary>
        /// <param name="cancelPaymentRequest"></param>
        /// <returns></returns>
        public async Task<CancelPaymentResponse> CancelPaymentAsync(CancelPaymentRequest cancelPaymentRequest, string publicKey, string privateKey)
        {
            CancelPaymentResponse cancelPaymentResponse = null;
            if (!string.IsNullOrEmpty(cancelPaymentRequest.authorizationId))
            {
                bool isLive = await this.GetIsLiveSetting();
                IAffirmAPI affirmAPI = new AffirmAPI(_httpContextAccessor, _httpClient, isLive);
                dynamic affirmResponse = await affirmAPI.VoidAsync(publicKey, privateKey, cancelPaymentRequest.authorizationId);

                // If affirmResponse.transaction_id is null, assume the payment was never authorized.
                // TODO: Make a call to 'Read' to get token status.
                // This will require storing and loading the token from vbase.
                cancelPaymentResponse = new CancelPaymentResponse
                {
                    paymentId = cancelPaymentRequest.paymentId,
                    cancellationId = affirmResponse.transaction_id ?? Guid.NewGuid().ToString(),
                    code = affirmResponse.type ?? affirmResponse.Error.Code,
                    message = affirmResponse.id ?? affirmResponse.Error.Message,
                    requestId = cancelPaymentRequest.requestId
                };
            }
            else
            {
                cancelPaymentResponse = new CancelPaymentResponse
                {
                    paymentId = cancelPaymentRequest.paymentId,
                    cancellationId = Guid.NewGuid().ToString(),
                    code = null,
                    message = null,
                    requestId = cancelPaymentRequest.requestId
                };
            }

            return cancelPaymentResponse;
        }

        /// <summary>
        /// Captures (settle) a payment that was previously approved.
        /// </summary>
        /// <param name="capturePaymentRequest"></param>
        /// <returns></returns>
        public async Task<CapturePaymentResponse> CapturePaymentAsync(CapturePaymentRequest capturePaymentRequest, string publicKey, string privateKey)
        {
            bool isLive = await this.GetIsLiveSetting();

            // Load request from storage for order id
            CreatePaymentRequest paymentRequest = await this._paymentRequestRepository.GetPaymentRequestAsync(capturePaymentRequest.paymentId);

            if (capturePaymentRequest.authorizationId == null)
            {
                // Get Affirm id from storage
                capturePaymentRequest.authorizationId = paymentRequest.transactionId;
            }

            IAffirmAPI affirmAPI = new AffirmAPI(_httpContextAccessor, _httpClient, isLive);
            dynamic affirmResponse = await affirmAPI.CaptureAsync(publicKey, privateKey, capturePaymentRequest.authorizationId, paymentRequest.orderId, string.Empty, string.Empty);

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
        public async Task<RefundPaymentResponse> RefundPaymentAsync(RefundPaymentRequest refundPaymentRequest, string publicKey, string privateKey)
        {
            bool isLive = await this.GetIsLiveSetting();

            if (refundPaymentRequest.authorizationId == null)
            {
                // Get Affirm id from storage
                CreatePaymentRequest paymentRequest = await this._paymentRequestRepository.GetPaymentRequestAsync(refundPaymentRequest.paymentId);
                refundPaymentRequest.authorizationId = paymentRequest.transactionId;
            }

            int amount = decimal.ToInt32(refundPaymentRequest.value * 100);

            IAffirmAPI affirmAPI = new AffirmAPI(_httpContextAccessor, _httpClient, isLive);
            dynamic affirmResponse = await affirmAPI.RefundAsync(publicKey, privateKey, refundPaymentRequest.authorizationId, amount);

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
        public async Task<CreatePaymentResponse> AuthorizeAsync(string paymentIdentifier, string token, string publicKey, string privateKey, string callbackUrl, int amount, string orderId)
        {
            bool isLive = await this.GetIsLiveSetting();

            if (string.IsNullOrEmpty(orderId))
            {
                orderId = paymentIdentifier;
            }

            IAffirmAPI affirmAPI = new AffirmAPI(_httpContextAccessor, _httpClient, isLive);
            dynamic affirmResponse = await affirmAPI.AuthorizeAsync(publicKey, privateKey, token, orderId);

            string paymentStatus = AffirmConstants.Vtex.Denied;
            if (affirmResponse.amount != null && affirmResponse.amount == amount)
            {
                paymentStatus = AffirmConstants.Vtex.Approved;
            }
            else if (affirmResponse.status_code != null && affirmResponse.status_code == StatusCodes.Status400BadRequest.ToString() && affirmResponse.code != null && affirmResponse.code == AffirmConstants.TokenUsed)
            {
                if (affirmResponse.charge_id != null)
                {
                    string chargeId = affirmResponse.charge_id;
                    Console.WriteLine($"Getting current status for {chargeId}");
                    affirmResponse = await affirmAPI.ReadChargeAsync(publicKey, privateKey, chargeId);
                    if (affirmResponse.status != null && affirmResponse.status == AffirmConstants.SuccessResponseCode)
                    {
                        paymentStatus = AffirmConstants.Vtex.Approved;
                    }
                }
            }

            CreatePaymentResponse paymentResponse = new CreatePaymentResponse();
            paymentResponse.paymentId = paymentIdentifier;
            paymentResponse.status = paymentStatus;
            paymentResponse.tid = affirmResponse.id ?? null;
            paymentResponse.authorizationId = affirmResponse.id ?? null;
            paymentResponse.code = affirmResponse.status ?? affirmResponse.status_code ?? affirmResponse.Error?.Code;
            string message = string.Empty;
            if (affirmResponse.events != null)
            {
                message = JsonConvert.SerializeObject(affirmResponse.events);
            }
            else if (affirmResponse.message != null)
            {
                message = affirmResponse.message;
            }
            else if (affirmResponse.Error != null && affirmResponse.Error.Message != null)
            {
                message = affirmResponse.Error.Message;
            }

            paymentResponse.message = message;

            await this._paymentRequestRepository.PostCallbackResponse(callbackUrl, paymentResponse);

            // Save the Affirm id & order number - will need for capture
            CreatePaymentRequest paymentRequest = new CreatePaymentRequest
            {
                transactionId = affirmResponse.id,
                orderId = orderId
            };

            await this._paymentRequestRepository.SavePaymentRequestAsync(paymentIdentifier, paymentRequest);

            return paymentResponse;
        }

        public async Task<object> ReadChargeAsync(string paymentId, string publicKey, string privateKey)
        {
            bool isLive = await this.GetIsLiveSetting();
            IAffirmAPI affirmAPI = new AffirmAPI(_httpContextAccessor, _httpClient, isLive);
            dynamic affirmResponse = await affirmAPI.ReadChargeAsync(publicKey, privateKey, paymentId);
            //dynamic affirmResponse = await affirmAPI.ReadAsync(publicKey, privateKey, paymentId);

            return affirmResponse;
        }

        public async Task<VtexSettings> GetSettingsAsync()
        {
            VtexSettings settings = await this._paymentRequestRepository.GetAppSettings();

            return settings;
        }

        private async Task<bool> GetIsLiveSetting()
        {
            bool retval = true;
            VtexSettings vtexSettings = await this._paymentRequestRepository.GetAppSettings();
            if (vtexSettings != null)
            {
                retval = vtexSettings.isLive;
            }

            return retval;
        }
    }
}
