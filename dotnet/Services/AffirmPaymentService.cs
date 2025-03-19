namespace Affirm.Services
{
    using Affirm.Data;
    using Affirm.Models;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Vtex.Api.Context;

    public class AffirmPaymentService : IAffirmPaymentService
    {
        private readonly IPaymentRequestRepository _paymentRequestRepository;
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IIOServiceContext _context;

        public AffirmPaymentService(IPaymentRequestRepository paymentRequestRepository, IHttpContextAccessor httpContextAccessor, HttpClient httpClient, IIOServiceContext context)
        {
            this._paymentRequestRepository = paymentRequestRepository ??
                                            throw new ArgumentNullException(nameof(paymentRequestRepository));

            this._httpContextAccessor = httpContextAccessor ??
                                        throw new ArgumentNullException(nameof(httpContextAccessor));

            this._httpClient = httpClient ??
                               throw new ArgumentNullException(nameof(httpClient));

            this._context = context ??
                               throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Creates a new payment and/or initiates the payment flow.
        /// </summary>
        /// <param name="createPaymentRequest"></param>
        /// <returns></returns>
        public async Task<CreatePaymentResponse> CreatePayment(CreatePaymentRequest createPaymentRequest, string publicKey)
        {
            CreatePaymentResponse paymentResponse = null;
            paymentResponse = await this._paymentRequestRepository.GetPaymentResponseAsync(createPaymentRequest.paymentId);
            if (paymentResponse != null)
            {
                // If this is a retry, a stored payment with undefined status should be treated as denied
                if (paymentResponse.status == AffirmConstants.Vtex.Undefined)
                {
                    paymentResponse.status = AffirmConstants.Vtex.Denied;
                }

                List<(string, string)> log = new List<(string, string)>()
                {
                ( "paymentId", paymentResponse.paymentId ),
                ( "tid", paymentResponse.tid ),
                };

                // Lots of these warnings in an account indicate that delayToCancel is too short
                _context.Vtex.Logger.Warn("CreatePaymentResponse", null, "paymentDeniedOnRetry", log.ToArray());
                return paymentResponse;
            }
            else
            {
                paymentResponse = new CreatePaymentResponse();
            }

            string paymentIdentifier = Guid.NewGuid().ToString();

            // Save the request for later retrieval
            await this._paymentRequestRepository.SavePaymentRequestAsync(paymentIdentifier, createPaymentRequest);
            paymentResponse.paymentId = createPaymentRequest.paymentId;
            paymentResponse.status = AffirmConstants.Vtex.Undefined;
            string siteHostSuffix = string.Empty;
            paymentResponse.paymentAppData = new PaymentAppData
            {
                appName = AffirmConstants.PaymentFlowAppName,
                payload = JsonConvert.SerializeObject(new Payload
                {
                    paymentIdentifier = paymentIdentifier,
                    publicKey = publicKey,
                    sandboxMode = createPaymentRequest.sandboxMode
                })
            };

            // Set minimum delayToCancel so that automatic authorization retry isn't sent before user completes modal
            paymentResponse.delayToCancel = AffirmConstants.MinimumDelayToCancel;

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

                    int delayToCancelSetting = vtexSettings.delayToCancel * multiple;
                    if (delayToCancelSetting > AffirmConstants.MinimumDelayToCancel)
                    {
                        paymentResponse.delayToCancel = delayToCancelSetting;
                    }
                }

                siteHostSuffix = vtexSettings.siteHostSuffix;
            }

            await this._paymentRequestRepository.SavePaymentResponseAsync(paymentResponse);

            return paymentResponse;
        }

        /// <summary>
        /// Cancels a payment that was not yet approved or captured (settled).
        /// </summary>
        /// <param name="cancelPaymentRequest"></param>
        /// <returns></returns>
        public async Task<CancelPaymentResponse> CancelPayment(CancelPaymentRequest cancelPaymentRequest, string publicKey, string privateKey)
        {
            CancelPaymentResponse cancelPaymentResponse = new CancelPaymentResponse
            {
                cancellationId = null,
                code = null,
                message = "Empty",
                paymentId = cancelPaymentRequest.paymentId,
                requestId = cancelPaymentRequest.requestId
            };

            // Load request from storage for order id
            CreatePaymentRequest paymentRequest = await this._paymentRequestRepository.GetPaymentRequestAsync(cancelPaymentRequest.paymentId);
            if (paymentRequest == null)
            {
                cancelPaymentResponse.message = $"Could not load Payment Request for Payment Id {cancelPaymentRequest.paymentId}.";
                _context.Vtex.Logger.Warn("CancelPayment", null, $"Could not load Payment Request for Payment Id: {cancelPaymentRequest.paymentId}");
            }
            else
            {
                // Get Affirm id from storage
                cancelPaymentRequest.authorizationId = paymentRequest.transactionId;
            }

            if (!string.IsNullOrEmpty(cancelPaymentRequest.authorizationId))
            {
                bool isLive = !cancelPaymentRequest.sandboxMode; // await this.GetIsLiveSetting();
                IAffirmAPI affirmAPI = new AffirmAPI(_httpContextAccessor, _httpClient, isLive, _context);
                dynamic affirmResponse = await affirmAPI.VoidAsync(publicKey, privateKey, cancelPaymentRequest.authorizationId, cancelPaymentRequest.transactionId);

                // If affirmResponse.reference_id is null, assume the payment was never authorized.
                // TODO: Make a call to 'Read' to get token status.
                // This will require storing and loading the token from vbase.
                cancelPaymentResponse = new CancelPaymentResponse
                {
                    paymentId = cancelPaymentRequest.paymentId,
                    cancellationId = affirmResponse.id ?? null,
                    code = affirmResponse.type ?? affirmResponse.Error.Code,
                    message = affirmResponse.created ?? affirmResponse.Error.Message,
                    requestId = cancelPaymentRequest.requestId
                };
            }
            else
            {
                // Assume that the order was never authorized
                cancelPaymentResponse.cancellationId = "not_authorized";
            }

            return cancelPaymentResponse;
        }

        /// <summary>
        /// Captures (settle) a payment that was previously approved.
        /// </summary>
        /// <param name="capturePaymentRequest"></param>
        /// <returns></returns>
        public async Task<CapturePaymentResponse> CapturePayment(CapturePaymentRequest capturePaymentRequest, string publicKey, string privateKey)
        {

            bool isLive = !capturePaymentRequest.sandboxMode; // await this.GetIsLiveSetting();
            CapturePaymentResponse capturePaymentResponse = new CapturePaymentResponse
            {
                message = "Unknown Error."
            };

            // Load request from storage for order id
            CreatePaymentRequest paymentRequest = await this._paymentRequestRepository.GetPaymentRequestAsync(capturePaymentRequest.paymentId);
            if (paymentRequest == null)
            {
                capturePaymentResponse.message = "Could not load Payment Request.";
                _context.Vtex.Logger.Info("CapturePayment", null, $"{capturePaymentResponse.message}");
            }
            else
            {
                // Get Affirm id from storage
                capturePaymentRequest.authorizationId = paymentRequest.transactionId;

                if (string.IsNullOrEmpty(capturePaymentRequest.authorizationId))
                {
                    capturePaymentResponse.message = "Missing authorizationId.";
                }
                else
                {
                    IAffirmAPI affirmAPI = new AffirmAPI(_httpContextAccessor, _httpClient, isLive, _context);
                    try
                    {
                        dynamic affirmResponse = await affirmAPI.CaptureAsync(publicKey, privateKey, capturePaymentRequest.authorizationId, paymentRequest.orderId, capturePaymentRequest.value, capturePaymentRequest.transactionId);
                        if (affirmResponse == null)
                        {
                            capturePaymentResponse.message = "Null affirmResponse.";
                            _context.Vtex.Logger.Warn("CapturePaymentResponse", null, $"{capturePaymentResponse.message}");
                        }
                        else
                        {
                            // If "Already Captured" then fake a success response.
                            if (affirmResponse.type != null && affirmResponse.type == AffirmConstants.AlreadyCaptured)
                            {
                                capturePaymentResponse = new CapturePaymentResponse
                                {
                                    paymentId = capturePaymentRequest.paymentId,
                                    settleId = affirmResponse.id ?? affirmResponse.type,
                                    value = affirmResponse.amount == null ? capturePaymentRequest.value : (decimal)affirmResponse.amount / 100m,
                                    code = affirmResponse.type ?? null, //affirmResponse.Error.Code,
                                    message = affirmResponse.id != null ? $"Fee={((affirmResponse.fee != null && affirmResponse.fee > 0) ? (decimal)affirmResponse.fee / 100m : 0):F2}" : affirmResponse.message ?? "Error", //: affirmResponse.Error.Message,
                                    requestId = capturePaymentRequest.requestId
                                };
                            }
                            else
                            {
                                capturePaymentResponse = new CapturePaymentResponse
                                {
                                    paymentId = capturePaymentRequest.paymentId,
                                    settleId = affirmResponse.id ?? null,
                                    value = affirmResponse.amount == null ? 0m : (decimal)affirmResponse.amount / 100m,
                                    code = affirmResponse.type ?? null, //affirmResponse.Error.Code,
                                    message = affirmResponse.id != null ? $"Fee={((affirmResponse.fee != null && affirmResponse.fee > 0) ? (decimal)affirmResponse.fee / 100m : 0):F2}" : affirmResponse.message ?? "Error", //: affirmResponse.Error.Message,
                                    requestId = capturePaymentRequest.requestId
                                };
                            }

                            if (capturePaymentRequest.authorizationId.StartsWith(AffirmConstants.KatapultIdPrefix))
                            {
                                // Need to get details from Katapult
                                VtexSettings vtexSettings = await _paymentRequestRepository.GetAppSettings();
                                if (vtexSettings.enableKatapult)
                                {
                                    capturePaymentResponse.value = affirmResponse.amount == null ? capturePaymentRequest.value : (decimal)affirmResponse.amount / 100m;
                                    KatapultFunding katapultResponse = await affirmAPI.KatapultFundingAsync(vtexSettings.katapultPrivateToken);
                                    if (katapultResponse != null)
                                    {
                                        FundingObject fundingObject = katapultResponse.FundingReport.FundingObjects.Where(f => f.OrderId.Equals(paymentRequest.orderId)).FirstOrDefault();
                                        capturePaymentResponse.message = JsonConvert.SerializeObject(fundingObject);
                                        _context.Vtex.Logger.Info("CapturePayment", null, $"Katapult Funding Response  {capturePaymentResponse.message}");
                                    }
                                    else
                                    {
                                        _context.Vtex.Logger.Info("CapturePayment", null, $"Katapult Funding Response was null.  [{capturePaymentRequest.authorizationId}]");
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _context.Vtex.Logger.Error("CapturePaymentAsync", null, $"CapturePaymentResponse:{ex.Message}", ex);
                        capturePaymentResponse.message = $"CapturePaymentAsync Error: {ex.Message}";
                    };
                }
            }

            return capturePaymentResponse;
        }

        /// <summary>
        /// Refunds a payment that was previously captured (settled). You can expect partial refunds.
        /// </summary>
        /// <param name="refundPaymentRequest"></param>
        /// <returns></returns>
        public async Task<RefundPaymentResponse> RefundPayment(RefundPaymentRequest refundPaymentRequest, string publicKey, string privateKey)
        {
            bool isLive = !refundPaymentRequest.sandboxMode; // await this.GetIsLiveSetting();
            CreatePaymentRequest paymentRequest = await this._paymentRequestRepository.GetPaymentRequestAsync(refundPaymentRequest.paymentId);
            // Get Affirm id from storage
            refundPaymentRequest.authorizationId = paymentRequest.transactionId;

            int amount = decimal.ToInt32(refundPaymentRequest.value * 100);

            IAffirmAPI affirmAPI = new AffirmAPI(_httpContextAccessor, _httpClient, isLive, _context);
            dynamic affirmResponse = await affirmAPI.RefundAsync(publicKey, privateKey, refundPaymentRequest.authorizationId, amount, refundPaymentRequest.transactionId);

            RefundPaymentResponse refundPaymentResponse = new RefundPaymentResponse
            {
                paymentId = refundPaymentRequest.paymentId,
                refundId = affirmResponse.reference_id ?? affirmResponse.id,
                value = affirmResponse.amount == null ? 0m : (decimal)affirmResponse.amount / 100m,
                code = affirmResponse.type ?? affirmResponse.Error.Code,
                message = affirmResponse.id != null ? $"Id:{affirmResponse.id} Fee={(affirmResponse.fee_refunded > 0 ? (decimal)affirmResponse.fee_refunded / 100m : 0):F2}" : affirmResponse.Error.Message,
                requestId = refundPaymentRequest.requestId
            };

            if (refundPaymentRequest.authorizationId.StartsWith(AffirmConstants.KatapultIdPrefix))
            {
                // Need to get details from Katapult
                VtexSettings vtexSettings = await _paymentRequestRepository.GetAppSettings();
                if (vtexSettings.enableKatapult)
                {
                    KatapultFunding katapultResponse = await affirmAPI.KatapultFundingAsync(vtexSettings.katapultPrivateToken);
                    if (katapultResponse != null)
                    {
                        FundingObject fundingObject = katapultResponse.FundingReport.FundingObjects.Where(f => f.OrderId.Equals(paymentRequest.orderId)).FirstOrDefault();
                        if (fundingObject != null)
                        {
                            refundPaymentResponse.message = $"Id:{affirmResponse.id} Fee={(fundingObject.Discount):F2}";
                            _context.Vtex.Logger.Info("RefundPayment", null, $"RefundPayment {refundPaymentResponse}");
                        }
                    }
                }
            }

            return refundPaymentResponse;
        }

        /// <summary>
        /// Retrieve saved Payment Request
        /// </summary>
        /// <param name="paymentIdentifier"></param>
        /// <returns></returns>
        public async Task<CreatePaymentRequest> GetCreatePaymentRequest(string paymentIdentifier)
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
        public async Task<CreatePaymentResponse> Authorize(string paymentIdentifier, string token, string publicKey, string privateKey, string callbackUrl, int amount, string orderId, bool sandboxMode, string transactionId)
        {
            bool isLive = !sandboxMode; // await this.GetIsLiveSetting();

            if (string.IsNullOrEmpty(orderId))
            {
                orderId = paymentIdentifier;
            }

            IAffirmAPI affirmAPI = new AffirmAPI(_httpContextAccessor, _httpClient, isLive, _context);
            dynamic affirmResponse = await affirmAPI.AuthorizeAsync(publicKey, privateKey, token, orderId, transactionId);

            string paymentStatus = AffirmConstants.Vtex.Denied;
            if (affirmResponse.amount != null && affirmResponse.amount == amount)
            {
                paymentStatus = AffirmConstants.Vtex.Approved;
                _context.Vtex.Logger.Info("CreatePaymentResponse", null, $"PaymentResponse: {paymentStatus}");
            }
            else //if (affirmResponse.status_code != null && affirmResponse.status_code == StatusCodes.Status403Forbidden.ToString() && affirmResponse.code != null && affirmResponse.code == AffirmConstants.TokenUsed)
            {
                if (affirmResponse.charge_id != null || affirmResponse.reference_id != null)
                {
                    string chargeId = affirmResponse.charge_id ?? affirmResponse.reference_id;
                    affirmResponse = await affirmAPI.ReadChargeAsync(publicKey, privateKey, chargeId);
                    if (affirmResponse.status != null && affirmResponse.status == AffirmConstants.SuccessResponseCode)
                    {
                        paymentStatus = AffirmConstants.Vtex.Approved;
                        _context.Vtex.Logger.Info("CreatePaymentResponse", null, $"PaymentResponse: {paymentStatus}");
                    }
                }
            }

            CreatePaymentResponse paymentResponse = new CreatePaymentResponse();
            paymentResponse.paymentId = paymentIdentifier;
            paymentResponse.status = paymentStatus;
            paymentResponse.tid = affirmResponse.id ?? null;
            paymentResponse.authorizationId = affirmResponse.checkout_id ?? null;
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

            if (affirmResponse.fields != null)
            {
                message = $"{message}: {affirmResponse.fields}";
            }

            paymentResponse.message = message;
            _context.Vtex.Logger.Info("PaymentResponse", null, $"PaymentResponse: {paymentResponse.message}");

            // Save the Affirm id & order number - will need for capture
            CreatePaymentRequest paymentRequest = new CreatePaymentRequest
            {
                transactionId = affirmResponse.id,
                orderId = orderId
            };

            // Don't save if data is missing
            if (!string.IsNullOrEmpty(paymentRequest.transactionId) && !string.IsNullOrEmpty(paymentRequest.orderId))
            {
                await this._paymentRequestRepository.SavePaymentRequestAsync(paymentIdentifier, paymentRequest);
                await this._paymentRequestRepository.SavePaymentResponseAsync(paymentResponse);
            }

            await this._paymentRequestRepository.PostCallbackResponse(callbackUrl, paymentResponse);

            return paymentResponse;
        }

        public async Task<CreatePaymentResponse> ReadCharge(string paymentId, string publicKey, string privateKey, bool sandboxMode)
        {
            bool isLive = !sandboxMode; // await this.GetIsLiveSetting();
            IAffirmAPI affirmAPI = new AffirmAPI(_httpContextAccessor, _httpClient, isLive, _context);
            dynamic affirmResponse = await affirmAPI.ReadChargeAsync(publicKey, privateKey, paymentId);

            return affirmResponse;
        }

        public async Task<VtexSettings> GetSettings()
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
