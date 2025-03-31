namespace Affirm.Controllers
{
    using Affirm.Models;
    using Affirm.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using System;
    using System.Threading.Tasks;
    using Vtex.Api.Context;

    public class RoutesController : Controller
    {
        private readonly IAffirmPaymentService _affirmPaymentService;
        private readonly IVtexTransactionService _vtexTransactionService;
        private readonly IIOServiceContext _context;

        public RoutesController(IAffirmPaymentService affirmPaymentService, IVtexTransactionService vtexTransactionService, IIOServiceContext context)
        {
            this._affirmPaymentService = affirmPaymentService ?? throw new ArgumentNullException(nameof(affirmPaymentService));
            this._vtexTransactionService = vtexTransactionService ?? throw new ArgumentNullException(nameof(vtexTransactionService));
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// https://{{providerApiEndpoint}}/payments
        /// Creates a new payment and/or initiates the payment flow.
        /// </summary>
        /// <param name="createPaymentRequest"></param>
        /// <returns></returns>
        public async Task<IActionResult> CreatePayment()
        {
            string publicKey = HttpContext.Request.Headers[AffirmConstants.PublicKeyHeader];
            var bodyAsText = await new System.IO.StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            CreatePaymentRequest createPaymentRequest = JsonConvert.DeserializeObject<CreatePaymentRequest>(bodyAsText);
            CreatePaymentResponse paymentResponse = await this._affirmPaymentService.CreatePayment(createPaymentRequest, publicKey);

            Response.Headers.Add("Cache-Control", "private");

            return Json(paymentResponse);
        }

        /// <summary>
        /// https://{{providerApiEndpoint}}/payments/{{paymentId}}/cancellations
        /// </summary>
        /// <param name="paymentId">VTEX payment ID from this payment</param>
        /// <param name="cancelPaymentRequest"></param>
        /// <returns></returns>
        public async Task<IActionResult> CancelPayment(string paymentId)
        {
            string privateKey = HttpContext.Request.Headers[AffirmConstants.PrivateKeyHeader];
            string publicKey = HttpContext.Request.Headers[AffirmConstants.PublicKeyHeader];

            var bodyAsText = await new System.IO.StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            CancelPaymentRequest cancelPaymentRequest = JsonConvert.DeserializeObject<CancelPaymentRequest>(bodyAsText);

            if (string.IsNullOrWhiteSpace(privateKey) || string.IsNullOrWhiteSpace(publicKey))
            {
                return BadRequest();
            }
            else
            {
                CancelPaymentResponse cancelResponse = await this._affirmPaymentService.CancelPayment(cancelPaymentRequest, publicKey, privateKey);
                _context.Vtex.Logger.Info("CancelPayment", null, $"{bodyAsText} {JsonConvert.SerializeObject(cancelResponse)}");

                return Json(cancelResponse);
            }
        }

        /// <summary>
        /// https://{{providerApiEndpoint}}/payments/{{paymentId}}/settlements
        /// </summary>
        /// <param name="paymentId">VTEX payment ID from this payment</param>
        /// <param name="capturePaymentRequest"></param>
        /// <returns></returns>
        public async Task<IActionResult> CapturePayment(string paymentId)
        {
            string privateKey = HttpContext.Request.Headers[AffirmConstants.PrivateKeyHeader];
            string publicKey = HttpContext.Request.Headers[AffirmConstants.PublicKeyHeader];

            var bodyAsText = await new System.IO.StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            CapturePaymentRequest capturePaymentRequest = JsonConvert.DeserializeObject<CapturePaymentRequest>(bodyAsText);

            if (string.IsNullOrWhiteSpace(privateKey) || string.IsNullOrWhiteSpace(publicKey))
            {
                return BadRequest();
            }
            else
            {
                //Check if partial cancellation feature is enabled and void the amount if items are cancelled
                await DoPartialCancellation(capturePaymentRequest, publicKey, privateKey);

                CapturePaymentResponse captureResponse = await this._affirmPaymentService.CapturePayment(capturePaymentRequest, publicKey, privateKey);
                _context.Vtex.Logger.Info("CapturePayment", null, $"{bodyAsText} {JsonConvert.SerializeObject(captureResponse)}");
                return Json(captureResponse);
            }
        }

        /// <summary>
        /// Handles partial cancellation of a payment transaction if the feature is enabled.
        /// </summary>
        /// <param name="capturePaymentRequest">The payment capture request containing transaction details.</param>
        /// <param name="publicKey">The public key used for authentication with the Affirm payment service.</param>
        /// <param name="privateKey">The private key used for authentication with the Affirm payment service.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task DoPartialCancellation(CapturePaymentRequest capturePaymentRequest, string publicKey, string privateKey)
        {
            // Check if partial cancellation feature is enabled
            bool isPartialCancellationEnabled = await _vtexTransactionService.isPartialCancellationEnabled();
            if (isPartialCancellationEnabled)
            {
                try
                {
                    bool isPartialVoidDoneForTransaction = await _vtexTransactionService.isPartialVoidDoneForTransaction(capturePaymentRequest.transactionId);
                    if (isPartialVoidDoneForTransaction)
                    {
                        _context.Vtex.Logger.Info(
                            "DoPartialCancellation",
                            null,
                            $"isPartialVoidDoneForTransaction: {isPartialVoidDoneForTransaction} on Transaction ID: {capturePaymentRequest.transactionId}"
                        );
                        return; // Exit early if isPartialVoidDoneForTransaction is true
                    }

                    // Get total cancelled amount from the transactionId
                    int cancelledAmount = await _vtexTransactionService.GetPartialCancelledAmount(capturePaymentRequest.transactionId);
                    _context.Vtex.Logger.Info(
                        "DoPartialCancellation",
                        null,
                        $"Cancelled Amount: {cancelledAmount} on Transaction ID: {capturePaymentRequest.transactionId}"
                    );

                    // If the cancelledAmount > 0, then void the cancelledAmount
                    if (cancelledAmount > 0)
                    {
                        AffirmVoidResponse voidResponse = await _affirmPaymentService.VoidPayment(capturePaymentRequest, publicKey, privateKey, cancelledAmount);
                        _context.Vtex.Logger.Info(
                            "DoPartialCancellation",
                            null,
                            $"Successfully voided {cancelledAmount} for transactionId {capturePaymentRequest.transactionId}."
                        );
                        if (voidResponse != null)
                        {
                            string voidResponseString = JsonConvert.SerializeObject(voidResponse);
                            //Asynchronously adds void transaction data to the VTEX transaction API in a fire-and-forget manner.
                            _vtexTransactionService.AddTransactionVoidData(capturePaymentRequest.transactionId, voidResponseString);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _context.Vtex.Logger.Error("DoPartialCancellation", null, $"Error in processing partial cancellation: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// https://{{providerApiEndpoint}}/payments/{{paymentId}}/refunds
        /// </summary>
        /// <param name="paymentId">VTEX payment ID from this payment</param>
        /// <param name="refundPaymentRequest"></param>
        /// <returns></returns>
        public async Task<IActionResult> RefundPayment(string paymentId)
        {
            string privateKey = HttpContext.Request.Headers[AffirmConstants.PrivateKeyHeader];
            string publicKey = HttpContext.Request.Headers[AffirmConstants.PublicKeyHeader];

            var bodyAsText = await new System.IO.StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            RefundPaymentRequest refundPaymentRequest = JsonConvert.DeserializeObject<RefundPaymentRequest>(bodyAsText);

            if (string.IsNullOrWhiteSpace(privateKey) || string.IsNullOrWhiteSpace(publicKey))
            {
                return BadRequest();
            }
            else
            {
                RefundPaymentResponse refundResponse = await this._affirmPaymentService.RefundPayment(refundPaymentRequest, publicKey, privateKey);
                _context.Vtex.Logger.Info("RefundPayment", null, $"{bodyAsText} {JsonConvert.SerializeObject(refundResponse)}");

                return Json(refundResponse);
            }
        }

        /// <summary>
        /// Retrieve stored payment request
        /// </summary>
        /// <param name="paymentIdentifier">Payment GUID</param>
        /// <returns></returns>
        public async Task<IActionResult> GetPaymentRequest(string paymentIdentifier)
        {
            var paymentRequest = await this._affirmPaymentService.GetCreatePaymentRequest(paymentIdentifier);

            Response.Headers.Add("Cache-Control", "private");

            return Json(paymentRequest);
        }

        /// <summary>
        /// After completing the checkout flow and receiving the checkout token, authorize the charge.
        /// Authorizing generates a charge ID that you’ll use to reference the charge moving forward.
        /// You must authorize a charge to fully create it. A charge is not visible in the Read response,
        /// nor in the merchant dashboard until you authorize it.
        /// </summary>
        /// <param name="paymentIdentifier">Payment GUID</param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<IActionResult> Authorize(string paymentIdentifier, string token, string callbackUrl, int orderTotal, bool sandboxMode)
        {
            string privateKey = HttpContext.Request.Headers[AffirmConstants.PrivateKeyHeader];
            string publicKey = HttpContext.Request.Headers[AffirmConstants.PublicKeyHeader];

            if (string.IsNullOrWhiteSpace(privateKey) || string.IsNullOrWhiteSpace(publicKey))
            {
                return BadRequest();
            }
            else
            {
                CreatePaymentResponse paymentResponse = await this._affirmPaymentService.Authorize(paymentIdentifier, token, publicKey, privateKey, callbackUrl, orderTotal, string.Empty, sandboxMode, string.Empty);
                _context.Vtex.Logger.Info("Authorize", null, $"{paymentIdentifier} {JsonConvert.SerializeObject(paymentResponse)}");
                Response.Headers.Add("Cache-Control", "private");

                return Json(paymentResponse);
            }
        }

        /// <summary>
        /// Read the charge information, current charge status, and checkout data
        /// </summary>
        /// <param name="paymentIdentifier">Payment GUID</param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<IActionResult> ReadCharge(string paymentIdentifier, string sandboxMode)
        {
            string privateKey = HttpContext.Request.Headers[AffirmConstants.PrivateKeyHeader];
            string publicKey = HttpContext.Request.Headers[AffirmConstants.PublicKeyHeader];

            if (string.IsNullOrWhiteSpace(privateKey) || string.IsNullOrWhiteSpace(publicKey))
            {
                return BadRequest();
            }
            else
            {
                CreatePaymentResponse paymentResponse = await this._affirmPaymentService.ReadCharge(paymentIdentifier, publicKey, privateKey, bool.Parse(sandboxMode));
                _context.Vtex.Logger.Info("ReadCharge", null, $"{paymentIdentifier} {JsonConvert.SerializeObject(paymentResponse)}");
                Response.Headers.Add("Cache-Control", "private");

                return Json(paymentResponse);
            }
        }

        public async Task<IActionResult> Inbound(string paymentId, string actiontype)
        {
            string responseCode = string.Empty;
            string responseMessage = string.Empty;
            string responseStatusCode = string.Empty;
            string responseBody = string.Empty;

            string privateKey = HttpContext.Request.Headers[AffirmConstants.PrivateKeyHeader];
            string publicKey = HttpContext.Request.Headers[AffirmConstants.PublicKeyHeader];
            var bodyAsText = await new System.IO.StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            InboundRequest inboundRequest = null;
            try
            {
                inboundRequest = JsonConvert.DeserializeObject<InboundRequest>(bodyAsText);
            }
            catch(Exception ex)
            {
                responseMessage = ex.Message;
                _context.Vtex.Logger.Error("Inbound", null, $"Error parsing InboundRequest {bodyAsText}", ex);
            }

            dynamic inboundRequestBody = null;
            try
            {
                inboundRequestBody = JsonConvert.DeserializeObject(inboundRequest.requestData.body);
            }
            catch(Exception ex)
            {
                responseMessage = ex.Message;
                _context.Vtex.Logger.Error("Inbound", null, $"Error parsing InboundRequestBody {bodyAsText}", ex);
            }

            paymentId = inboundRequest.paymentId;
            string requestId = inboundRequest.requestId;

            if(inboundRequestBody == null)
            {
                responseStatusCode = StatusCodes.Status400BadRequest.ToString();
            }
            else if (string.IsNullOrWhiteSpace(privateKey) || string.IsNullOrWhiteSpace(publicKey))
            {
                responseStatusCode = StatusCodes.Status400BadRequest.ToString();
                responseMessage = "Missing keys.";
            }
            else
            {
                try
                {
                    switch (actiontype)
                    {
                        case AffirmConstants.Inbound.ActionAuthorize:
                            string token = inboundRequestBody.token;
                            string callbackUrl = inboundRequestBody.callbackUrl;
                            int amount = inboundRequestBody.orderTotal;
                            string orderId = inboundRequestBody.orderId;
                            bool sandboxMode = inboundRequest.sandboxMode;
                            string transactionId = inboundRequest.transactionId;
                            if (string.IsNullOrEmpty(paymentId) || string.IsNullOrEmpty(token) || string.IsNullOrEmpty(callbackUrl))
                            {
                                responseStatusCode = StatusCodes.Status400BadRequest.ToString();
                                responseMessage = "Missing parameters.";
                            }
                            else
                            {
                                var paymentRequest = await this._affirmPaymentService.Authorize(paymentId, token, publicKey, privateKey, callbackUrl, amount, orderId, sandboxMode, transactionId);
                                _context.Vtex.Logger.Info("Inbound", null, $"{paymentId} {JsonConvert.SerializeObject(paymentRequest)}");
                                Response.Headers.Add("Cache-Control", "private");

                                responseBody = JsonConvert.SerializeObject(paymentRequest);
                                responseStatusCode = StatusCodes.Status200OK.ToString();
                            }

                            break;
                        default:
                            responseStatusCode = StatusCodes.Status405MethodNotAllowed.ToString();
                            responseMessage = $"Action '{actiontype}' is not supported.";
                            break;
                    }
                }
                catch(Exception ex)
                {
                    _context.Vtex.Logger.Error("Inbound", null, $"Error processing request {bodyAsText}", ex);
                }
            }

            InboundResponse response = null;
            try
            {
                response = new InboundResponse
                {
                    code = responseCode,
                    message = responseMessage,
                    paymentId = paymentId,
                    requestId = requestId,
                    responseData = new ResponseData
                    {
                        body = responseBody,
                        statusCode = responseStatusCode
                    }
                };
            }
            catch(Exception ex)
            {
                _context.Vtex.Logger.Error("Inbound", null, $"Error creating Response {responseBody}", ex);
            }

            return Json(response);
        }

        public async Task<IActionResult> GetAppSettings()
        {
            VtexSettings paymentRequest = await this._affirmPaymentService.GetSettings();

            return Json(paymentRequest);
        }

        public string PrintHeaders()
        {
            string headers = "--->>> Headers <<<---\n";
            foreach (var header in HttpContext.Request.Headers)
            {
                headers += $"{header.Key}: {header.Value}\n";
            }
            return headers;
        }

        public string PrintHeadersPriv()
        {
            return PrintHeaders();
        }

        public JsonResult PaymentMethods()
        {
            PaymentMethods methods = new PaymentMethods();
            methods.paymentMethods = new System.Collections.Generic.List<string>();
            methods.paymentMethods.Add("Affirm");
            methods.paymentMethods.Add("Promissories");

            Response.Headers.Add("Cache-Control", "private");

            return Json(methods);
        }
    }
}
