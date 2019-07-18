namespace UnitTests
{
    using Affirm.Controllers;
    using Affirm.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using UnitTests.Fakes;

    [TestClass]
    public class RouteControllerTests
    {
        const string PaymentId = "9XE8-EJX1";
        const string PrivateKey = "jIRab2ct4mUnxltiNgrN0v3uFBpIvZHJ";
        const string PublicKey = "1WWKWI5U36GAG5OV";

        [TestMethod]
        public async Task TestReadChargeAsync()
        {
            FakePaymentRepository fakePaymentRepository = new FakePaymentRepository();
            AffirmPaymentService affirmPaymentsService = new AffirmPaymentService(fakePaymentRepository);
            FakeContextAccessor httpContextAccessor = new FakeContextAccessor();
            httpContextAccessor.HttpContext.Request.Headers.Add(AffirmConstants.PrivateKeyHeader, PrivateKey);
            httpContextAccessor.HttpContext.Request.Headers.Add(AffirmConstants.PublicKeyHeader, PublicKey);
            var routesController = new RoutesController(affirmPaymentsService);

            IActionResult controllerResult = await routesController.ReadChargeAsync(PaymentId);

            Assert.IsTrue(controllerResult is JsonResult);
        }
    }
}
