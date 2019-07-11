﻿namespace Affirm.Services
{
    using Affirm.Models;
    using System.Threading.Tasks;

    public interface IAffirmPaymentService
    {
        Task<CreatePaymentResponse> CreatePaymentAsync(CreatePaymentRequest createPaymentRequest);

        Task<CancelPaymentResponse> CancelPaymentAsync(CancelPaymentRequest cancelPaymentRequest, string publicKey, string privateKey);

        Task<CapturePaymentResponse> CapturePaymentAsync(CapturePaymentRequest capturePaymentRequest, string publicKey, string privateKey);

        Task<RefundPaymentResponse> RefundPaymentAsync(RefundPaymentRequest refundPaymentRequest, string publicKey, string privateKey);

        Task<CreatePaymentRequest> GetCreatePaymentRequest(string paymentIdentifier);

        Task<CreatePaymentResponse> AuthorizeAsync(string paymentIdentifier, string token, string publicKey, string privateKey);
    }
}
