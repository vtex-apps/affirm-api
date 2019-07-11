namespace Vtex
{
    using Affirm.Data;
    using Affirm.Services;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using System.Net.Http;

    public class StartupExtender
    {
        public void ExtendConstructor(IConfiguration config, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {

        }

        public void ExtendConfigureServices(IServiceCollection services)
        {
            //throw new System.Exception("extend");
            services.AddSingleton<HttpClient>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IVtexEnvironmentVariableProvider, VtexEnvironmentVariableProvider>();
            services.AddTransient<IPaymentRequestRepository, PaymentRequestRepository>();
            services.AddTransient<IAffirmPaymentService, AffirmPaymentService>();
        }

        public void ExtendConfigureBeforeMvc(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {

        }

        public void ExtendConfigureAfterMvc(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {

        }
    }
}