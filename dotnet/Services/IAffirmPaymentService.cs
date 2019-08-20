﻿namespace Affirm.Services
{
    using Affirm.Models;
    using System.Threading.Tasks;

    public interface IAffirmPaymentService
    {
        Task<CreatePaymentResponse> CreatePaymentAsync(CreatePaymentRequest createPaymentRequest, string publicKey);

        Task<CancelPaymentResponse> CancelPaymentAsync(CancelPaymentRequest cancelPaymentRequest, string publicKey, string privateKey, bool isLive);

        Task<CapturePaymentResponse> CapturePaymentAsync(CapturePaymentRequest capturePaymentRequest, string publicKey, string privateKey, bool isLive);

        Task<RefundPaymentResponse> RefundPaymentAsync(RefundPaymentRequest refundPaymentRequest, string publicKey, string privateKey, bool isLive);

        Task<CreatePaymentRequest> GetCreatePaymentRequestAsync(string paymentIdentifier);

        Task<CreatePaymentResponse> AuthorizeAsync(string paymentIdentifier, string token, string publicKey, string privateKey, bool isLive, string callbackUrl, int amount, string orderId);

        Task<object> ReadChargeAsync(string paymentId, string publicKey, string privateKey, bool isLive);

        Task<VtexSettings> GetSettingsAsync();
    }
}
