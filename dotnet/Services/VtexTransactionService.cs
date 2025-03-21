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
                Console.WriteLine("getPaymentCancellationsResponse : " + getPaymentCancellationsResponse);
                _context.Vtex.Logger.Info("getPaymentCancellationsResponse : " + getPaymentCancellationsResponse);
                // Convert dynamic response to JSON string
                string jsonResponse = JsonConvert.SerializeObject(getPaymentCancellationsResponse);
                // Deserialize JSON string into GetPaymentCancellationResponse object
                paymentCancellationResponse = JsonConvert.DeserializeObject<GetPaymentCancellationResponse>(jsonResponse);
            }
            return paymentCancellationResponse;
        }

        /// <summary>
        /// Gets the Partial Cancelled Item Amount on the transaction
        /// </summary>
        /// <param name="GetCancelledAmount"></param>
        /// <returns></returns>
        public async Task<int> GetPartialCancelledAmount(string transactionId)
        {
            // Need to get details from Katapult
            VtexSettings vtexSettings = await _paymentRequestRepository.GetAppSettings();

            //string vtexAppKey = vtexSettings.vtexAppKey;
            //string vtexAppToken = vtexSettings.vtexAppToken;

            //string vtexAppKey = vtexSettings.vtexAppKey;
            string vtexAppKey = "vtexappkey-logixalpartnerus-YPIMAJ";
            //string vtexAppToken = vtexSettings.vtexAppToken;
            string vtexAppToken = "UHQUVUXMVJSNEWCBDCIWZCQCVLUGMPKEPWXWDDVHIWXPQNWUKKSZZLCODUVOFFPCMNYWYXSECVYRXUALEKJTWIXITZWIBJNYTQNAMPFAXVIHIFEPHHQJQBQOOUWPLYOO";

            //Gets the Cancellations actions done on payment
            var getPaymentCancellationsResponse = await GetPaymentCancellations(vtexAppKey, vtexAppToken, transactionId);

            if (getPaymentCancellationsResponse == null || getPaymentCancellationsResponse?.actions == null)
            {
                return 0; // Return 0 if there are no actions
            }
            //Getting the sum of all the action value on Payment Cancellations Response
            int cancelledAmount = getPaymentCancellationsResponse.actions.Sum(action => action.value);

            Console.WriteLine("Cancelled Amount: " + cancelledAmount);

            return cancelledAmount;
        }

        public async Task<bool> isPartialCancellationEnabled()
        {
            // Need to get details from enablePartialCancellation
            VtexSettings vtexSettings = await _paymentRequestRepository.GetAppSettings();

            Console.WriteLine("enablePartialCancellation : " + vtexSettings.enablePartialCancellation);

            // Ensure it's properly converted to a boolean
            if (bool.TryParse(vtexSettings.enablePartialCancellation.ToString(), out bool result))
            {
                return result;
            }
            return false; // Default value if conversion fails
        }

        public async Task AddTransactionVoidData(string transactionId, string voidResponse)
        {
            // Need to get details from vtexSettings
            //VtexSettings vtexSettings = await _paymentRequestRepository.GetAppSettings();

            //string vtexAppKey = vtexSettings.vtexAppKey;
            string vtexAppKey = "vtexappkey-logixalpartnerus-YPIMAJ";
            //string vtexAppToken = vtexSettings.vtexAppToken;
            string vtexAppToken = "UHQUVUXMVJSNEWCBDCIWZCQCVLUGMPKEPWXWDDVHIWXPQNWUKKSZZLCODUVOFFPCMNYWYXSECVYRXUALEKJTWIXITZWIBJNYTQNAMPFAXVIHIFEPHHQJQBQOOUWPLYOO";

            IVtexTransactionAPI vtexTransactionAPI = new VtexTransactionAPI(_httpContextAccessor, _httpClient, _context);
            Console.WriteLine("------- AddTransactionVoidData : transactionId : " + transactionId + " , voidResponse : " + voidResponse);
            await vtexTransactionAPI.AddTransactionDataAsync(vtexAppKey, vtexAppToken, transactionId, voidResponse);
        }

        
        public async Task<bool> isPartialVoidDoneForTransaction(string transactionId)
        {
            // Need to get details from enablePartialCancellation
            AffirmVoidResponse affirmVoidResponse  = await _paymentRequestRepository.GetVoidResponseAsync(transactionId);

            Console.WriteLine(" isPartialVoidDoneForTransaction:: affirmVoidResponse : " + JsonConvert.SerializeObject(affirmVoidResponse));

            // Ensure it's properly converted to a boolean
            if (affirmVoidResponse != null)
            {
                return true;
            }
            return false; // Default value if affirmVoidResponse is null
        }
    }
}