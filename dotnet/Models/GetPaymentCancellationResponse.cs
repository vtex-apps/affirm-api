#nullable enable
namespace Affirm.Models
{
    using System;
    using System.Collections.Generic;

    public class GetPaymentCancellationResponse
    {
        /// <summary>
        /// List of cancellation requests
        /// </summary>
        public List<Request>? requests { get; set; }

        /// <summary>
        /// List of actions performed during cancellation
        /// </summary>
        public List<PaymentAction>? actions { get; set; }

        /// <summary>
        /// Error details (null if response is successful)
        /// </summary>
        public ErrorResponse? error { get; set; }
    }

    public class Request
    {
        public string id { get; set; } = string.Empty;
        public DateTime date { get; set; }
        public int value { get; set; }
    }

    public class PaymentAction
    {
        public string id { get; set; } = string.Empty;
        public string paymentId { get; set; } = string.Empty;
        public Payment payment { get; set; } = new Payment();
        public DateTime date { get; set; }
        public int value { get; set; }
    }

    public class Payment
    {
        public string href { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents an error response structure
    /// </summary>
    public class ErrorResponse
    {
        public string code { get; set; } = string.Empty;
        public string message { get; set; } = string.Empty;
        public string? exception { get; set; }
    }
}
