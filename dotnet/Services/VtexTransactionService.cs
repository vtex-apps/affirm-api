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

    public class VtexTransactionService : IVtexTransactionService
    {
        private readonly IPaymentRequestRepository _paymentRequestRepository;
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IIOServiceContext _context;

        public VtexTransactionService(IPaymentRequestRepository paymentRequestRepository, IHttpContextAccessor httpContextAccessor, HttpClient httpClient, IIOServiceContext context)
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
        /// Cancels a payment that was not yet approved or captured (settled).
        /// </summary>
        /// <param name="cancelPaymentRequest"></param>
        /// <returns></returns>
        public async Task<GetPaymentCancellationResponse> GetPaymentCancellations(string vtexAppKey, string vtexAppToken, string transactionId)
        {
            IVtexTransactionAPI vtexTransactionAPI = new VtexTransactionAPI(_httpContextAccessor, _httpClient, _context);
            dynamic getPaymentCancellationsResponse =
                await vtexTransactionAPI.GetPaymentCancellationsAsync(vtexAppKey, vtexAppToken, transactionId);
            GetPaymentCancellationResponse paymentCancellationResponse = null;
            if (getPaymentCancellationsResponse != null)
            {
                // Convert dynamic response to JSON string
                string jsonResponse = JsonConvert.SerializeObject(getPaymentCancellationsResponse);
                // Deserialize JSON string into GetPaymentCancellationResponse object
                paymentCancellationResponse = JsonConvert.DeserializeObject<GetPaymentCancellationResponse>(jsonResponse);
            }
            return paymentCancellationResponse;
        }

        /// <summary>
        /// Retrieves the total amount of partially cancelled items payments for a given transaction.
        /// </summary>
        /// <param name="transactionId">The unique identifier of the transaction.</param>
        /// <returns>
        /// A task that returns the total partially canceled amount in minor units (e.g., cents).
        /// Returns 0 if there are no partially cancelled payments.
        /// </returns>
        public async Task<int> GetPartialCancelledAmount(string transactionId)
        {
            // Need to get details from VTEX App key and Token
            VtexSettings vtexSettings = await _paymentRequestRepository.GetAppSettings();

            string vtexAppKey = vtexSettings.vtexAppKey;
            string vtexAppToken = vtexSettings.vtexAppToken;

            // Validate credentials
            if (string.IsNullOrWhiteSpace(vtexAppKey) || string.IsNullOrWhiteSpace(vtexAppToken))
            {
                _context.Vtex.Logger.Warn("GetPartialCancelledAmount", null, "VTEX App Key or Token is missing.");
                return 0;
            }

            //Gets the Cancellations actions done on payment
            var getPaymentCancellationsResponse = await GetPaymentCancellations(vtexAppKey, vtexAppToken, transactionId);

            if (getPaymentCancellationsResponse == null || getPaymentCancellationsResponse?.actions == null)
            {
                return 0; // Return 0 if there are no actions
            }
            //Getting the sum of all the action value on Payment Cancellations Response
            int cancelledAmount = getPaymentCancellationsResponse.actions.Sum(action => action.value);

            _context.Vtex.Logger.Info(
                "GetPartialCancelledAmount",
                null,
                $"Cancelled Amount: {cancelledAmount} on Transaction ID: {transactionId}"
            );

            return cancelledAmount;
        }

        /// <summary>
        /// Checks if the partial cancellation feature is enabled in VTEX settings.
        /// </summary>
        /// <returns>
        /// A boolean indicating whether partial cancellation is enabled.
        /// Returns <c>false</c> if the setting is <c>null</c> or in case of any exception.
        /// </returns>
        /// <exception cref="Exception">
        /// Catches any exception that occurs while retrieving settings and logs the error.
        /// </exception>
        public async Task<bool> isPartialCancellationEnabled()
        {
            try
            {
                // Ensure vtexSettings is not null
                VtexSettings vtexSettings = await _paymentRequestRepository.GetAppSettings();

                // Log the enablePartialCancellation value
                _context.Vtex.Logger.Info("isPartialCancellationEnabled", null, $"enablePartialCancellation: {vtexSettings.enablePartialCancellation}");

                // Directly return the value for enablePartialCancellation
                return vtexSettings.enablePartialCancellation;
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("isPartialCancellationEnabled", null, $"Exception occurred: {ex.Message}");
                return false; // Return false in case of any failure
            }
        }

        /// <summary>
        /// Adds void transaction data to VTEX if valid credentials are available.
        /// </summary>
        /// <param name="transactionId">The transaction ID for which the void data is being added.</param>
        /// <param name="voidResponse">The void response data in JSON format.</param>
        /// <returns>A task representing the asynchronous operation for Adding Transaction data</returns>
        public async Task AddTransactionVoidData(string transactionId, string voidResponse)
        {
            // Retrieve VTEX settings
            VtexSettings vtexSettings = await _paymentRequestRepository.GetAppSettings();

            // Extract VTEX Key and Token credentials
            string vtexAppKey = vtexSettings.vtexAppKey;
            string vtexAppToken = vtexSettings.vtexAppToken;

            // Validate credentials
            if (string.IsNullOrWhiteSpace(vtexAppKey) || string.IsNullOrWhiteSpace(vtexAppToken))
            {
                _context.Vtex.Logger.Warn("AddTransactionVoidData", null, "VTEX App Key or Token is missing, aborting transaction update.");
                return;
            }

            // Initialize VTEX Transaction API
            IVtexTransactionAPI vtexTransactionAPI = new VtexTransactionAPI(_httpContextAccessor, _httpClient, _context);

            // Add transaction data
            await vtexTransactionAPI.AddTransactionDataAsync(vtexAppKey, vtexAppToken, transactionId, voidResponse);
        }

        /// <summary>
        /// Checks whether a partial void has been processed for a given transaction.
        /// </summary>
        /// <param name="transactionId">The unique identifier of the transaction.</param>
        /// <returns>
        /// A boolean indicating whether a partial void has been processed for the transaction.
        /// Returns <c>true</c> if a void response exists; otherwise, returns <c>false</c>.
        /// </returns>
        public async Task<bool> isPartialVoidDoneForTransaction(string transactionId)
        {
            // Retrieve void response for the given transaction
            AffirmVoidResponse affirmVoidResponse = await _paymentRequestRepository.GetVoidResponseAsync(transactionId);
            // Log the response in structured format
            _context.Vtex.Logger.Info(
                "isPartialVoidDoneForTransaction",
                null,
                $"Transaction ID: {transactionId}, isPartialVoidDoneForTransaction: {affirmVoidResponse != null}"
            );
            return affirmVoidResponse != null;
        }
    }
}