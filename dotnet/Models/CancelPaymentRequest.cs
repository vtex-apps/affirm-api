﻿namespace Affirm.Models
{
    public class CancelPaymentRequest
    {
        public string paymentId { get; set; }
        public string requestId { get; set; }
    }
}
