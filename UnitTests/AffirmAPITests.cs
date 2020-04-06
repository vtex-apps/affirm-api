using Affirm.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestClass]
    public class AffirmAPITests
    {
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
            dynamic response = await affirmAPI.ReadAsync(publicKey, privateKey, "9XE8-EJX1");
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
            dynamic response = await affirmAPI.ReadChargeAsync(publicKey, privateKey, "9XE8-EJX1");
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
            dynamic response = await affirmAPI.AuthorizeAsync(publicKey, privateKey, "PQ7AQ3X8H9W94V1Z", "1023171562818");
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
    }
}
