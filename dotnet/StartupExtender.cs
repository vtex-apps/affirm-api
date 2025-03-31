namespace Vtex
{
    using Affirm.Data;
    using Affirm.Services;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using System.Net.Http;

    public class StartupExtender
    {
        public void ExtendConstructor(IConfiguration config, IWebHostEnvironment env)
        {

        }

        public void ExtendConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<HttpClient>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IVtexEnvironmentVariableProvider, VtexEnvironmentVariableProvider>();
            services.AddTransient<IPaymentRequestRepository, PaymentRequestRepository>();
            services.AddTransient<IAffirmPaymentService, AffirmPaymentService>();
            services.AddTransient<IVtexTransactionService, VtexTransactionService>();
        }

        public void ExtendConfigureBeforeRouting(IApplicationBuilder app, IWebHostEnvironment env)
        {

        }

        public void ExtendConfigureBeforeEndpoint(IApplicationBuilder app, IWebHostEnvironment env)
        {

        }
    }
}