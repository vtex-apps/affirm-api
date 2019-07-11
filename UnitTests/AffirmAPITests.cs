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

        const string privateKey = "jIRab2ct4mUnxltiNgrN0v3uFBpIvZHJ";
        const string publicKey = "1WWKWI5U36GAG5OV";

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
            dynamic response = await affirmAPI.ReadAsync(publicKey, privateKey, "test");
            if(response != null)
            {
                string status_code = GetDynamicValue("status_code", response);
                string message = GetDynamicValue("message", response);
                string code = GetDynamicValue("code", response);
                string type = GetDynamicValue("type", response);
            }
        }
    }
}
