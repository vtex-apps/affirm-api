namespace Affirm
{
    using Affirm.Models;
    using Affirm.Services;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Threading.Tasks;

    public class RoutesController : Controller
    {
        private readonly IAffirmPaymentService _affirmPaymentService;

        public RoutesController(IAffirmPaymentService AffirmPaymentService)
        {
            this._affirmPaymentService = AffirmPaymentService ?? throw new ArgumentNullException(nameof(AffirmPaymentService));
        }

        /// <summary>
        /// https://{{providerApiEndpoint}}/payments
        /// Creates a new payment and/or initiates the payment flow.
        /// </summary>
        /// <param name="createPaymentRequest"></param>
        /// <returns></returns>
        public async Task<IActionResult> CreatePaymentAsync([FromBody] CreatePaymentRequest createPaymentRequest)
        {
            var paymentResponse = await this._affirmPaymentService.CreatePaymentAsync(createPaymentRequest);

            Response.Headers.Add("Cache-Control", "private");

            return Json(paymentResponse);
        }

        /// <summary>
        /// https://{{providerApiEndpoint}}/payments/{{paymentId}}/cancellations
        /// </summary>
        /// <param name="paymentId">VTEX payment ID from this payment</param>
        /// <param name="cancelPaymentRequest"></param>
        /// <returns></returns>
        public async Task<IActionResult> CancelPaymentAsync(string paymentId, [FromBody] CancelPaymentRequest cancelPaymentRequest)
        {
            string privateKey = HttpContext.Request.Headers[AffirmConstants.PrivateKeyHeader];
            string publicKey = HttpContext.Request.Headers[AffirmConstants.PublicKeyHeader];
            var cancelResponse = await this._affirmPaymentService.CancelPaymentAsync(cancelPaymentRequest, publicKey, privateKey);

            return Json(cancelResponse);
        }

        /// <summary>
        /// https://{{providerApiEndpoint}}/payments/{{paymentId}}/settlements
        /// </summary>
        /// <param name="paymentId">VTEX payment ID from this payment</param>
        /// <param name="capturePaymentRequest"></param>
        /// <returns></returns>
        public async Task<IActionResult> CapturePaymentAsync(string paymentId, [FromBody] CapturePaymentRequest capturePaymentRequest)
        {
            string privateKey = HttpContext.Request.Headers[AffirmConstants.PrivateKeyHeader];
            string publicKey = HttpContext.Request.Headers[AffirmConstants.PublicKeyHeader];
            var captureResponse = await this._affirmPaymentService.CapturePaymentAsync(capturePaymentRequest, publicKey, privateKey);

            return Json(captureResponse);
        }

        /// <summary>
        /// https://{{providerApiEndpoint}}/payments/{{paymentId}}/refunds
        /// </summary>
        /// <param name="paymentId">VTEX payment ID from this payment</param>
        /// <param name="refundPaymentRequest"></param>
        /// <returns></returns>
        public async Task<IActionResult> RefundPaymentAsync(string paymentId, [FromBody] RefundPaymentRequest refundPaymentRequest)
        {
            string privateKey = HttpContext.Request.Headers[AffirmConstants.PrivateKeyHeader];
            string publicKey = HttpContext.Request.Headers[AffirmConstants.PublicKeyHeader];
            var refundResponse = await this._affirmPaymentService.RefundPaymentAsync(refundPaymentRequest, publicKey, privateKey);

            return Json(refundResponse);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="paymentIdentifier">Payment GUID</param>
        /// <returns></returns>
        public async Task<IActionResult> GetPaymentRequestAsync(string paymentIdentifier)
        {
            var paymentRequest = await this._affirmPaymentService.GetCreatePaymentRequest(paymentIdentifier);

            Response.Headers.Add("Cache-Control", "private");

            return Json(paymentRequest);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="paymentIdentifier">Payment GUID</param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<IActionResult> AuthorizeAsync(string paymentIdentifier, string token)
        {
            string privateKey = HttpContext.Request.Headers[AffirmConstants.PrivateKeyHeader];
            string publicKey = HttpContext.Request.Headers[AffirmConstants.PublicKeyHeader];
            var paymentRequest = await this._affirmPaymentService.AuthorizeAsync(paymentIdentifier, token, publicKey, privateKey);

            Response.Headers.Add("Cache-Control", "private");

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
