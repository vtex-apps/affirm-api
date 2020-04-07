using Affirm.Models;
using Affirm.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestClass]
    public class AffirmAPITests
    {
        const string APPLICATION_JSON = "application/json";
        const string BASE_URL = "https://sandbox.affirm.com/api/v1";
        readonly HttpContextAccessor contextAccessor = new HttpContextAccessor();
        readonly HttpClient httpClient = new HttpClient();

        // UU sandbox
        //const string privateKey = "jIRab2ct4mUnxltiNgrN0v3uFBpIvZHJ";
        //const string publicKey = "1WWKWI5U36GAG5OV";
        // UU prod
        //const string privateKey = "HQVQobWqxTjNDeyoZba6SeWBkWAePwfX";
        //const string publicKey = "RO3VDMNLGGTF2TL8";
        // Moto sandbox
        const string privateKey = "WqPPYUp0RJwjS2mg5oMOqmtnRQ9Qqo1n";
        const string publicKey = "84971L7SGAB1MVTX";

        private string GetDynamicValue(string name, dynamic dynamicObject)
        {
            string value = string.Empty;

            try
            {
                var propertyInfo = dynamicObject.GetType().GetProperty(name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                value = propertyInfo.GetValue(dynamicObject, null);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return value;
        }

        [TestMethod]
        public async Task TestMethod1()
        {
            IAffirmAPI affirmAPI = new AffirmAPI(contextAccessor, httpClient, false);
            dynamic response = await affirmAPI.ReadAsync(publicKey, privateKey, "U5LL34ABTDS8A7DM");
            if(response != null)
            {
                //response = GetDynamicValue("?xml", response);
                //string status_code = GetDynamicValue("status_code", response);
                //string message = GetDynamicValue("message", response);
                //string code = GetDynamicValue("code", response);
                //string type = GetDynamicValue("type", response);
                string status_code = response.status_code;
                string message = response.message;
                string code = response.code;
                string type = response.type;
            }
            else
            {
                Console.WriteLine(response);
            }
        }

        [TestMethod]
        public async Task TestMethod2()
        {
            IAffirmAPI affirmAPI = new AffirmAPI(contextAccessor, httpClient, false);
            dynamic response = await affirmAPI.ReadChargeAsync(publicKey, privateKey, "U5LL34ABTDS8A7DM");
            if (response != null)
            {
                //response = GetDynamicValue("?xml", response);
                //string status_code = GetDynamicValue("status_code", response);
                //string message = GetDynamicValue("message", response);
                //string code = GetDynamicValue("code", response);
                //string type = GetDynamicValue("type", response);
                string status_code = response.status_code;
                string message = response.message;
                string code = response.code;
                string type = response.type;
                Console.WriteLine(response);
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public async Task TestMethod3()
        {
            IAffirmAPI affirmAPI = new AffirmAPI(contextAccessor, httpClient, false);
            dynamic response = await affirmAPI.AuthorizeAsync(publicKey, privateKey, "U5LL34ABTDS8A7DM", "1023171562818");
            if (response != null)
            {
                //response = GetDynamicValue("?xml", response);
                //string status_code = GetDynamicValue("status_code", response);
                //string message = GetDynamicValue("message", response);
                //string code = GetDynamicValue("code", response);
                //string type = GetDynamicValue("type", response);
                string status_code = response.status_code;
                string message = response.message;
                string code = response.code;
                string type = response.type;
                Console.WriteLine(response);
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public async Task TestCapture()
        {
            string orderId = "1023252261358";
            string chargeId = "UDXG-SW24";

            AffirmCaptureRequest captureRequest = new AffirmCaptureRequest
            {
                order_id = orderId
            };

            var jsonSerializedRequest = JsonConvert.SerializeObject(captureRequest);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{BASE_URL}/{AffirmConstants.Transactions}/{chargeId}/{AffirmConstants.Capture}"),
                //Content = new StringContent(jsonSerializedRequest, Encoding.UTF8, APPLICATION_JSON)
            };

            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{publicKey}:{privateKey}")));

            HttpClient client = new HttpClient();
            var response = await client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseContent);
        }
    }
}
