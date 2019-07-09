namespace Affirm
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
        // This method is called inside Startup's constructor
        // You can use it to build a custom configuration
        // This method is called inside Startup's constructor 
        // You can use it to build a custom configuration 
        public void ExtendConstructor(IConfiguration config, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {

        }

        // This method is called inside Startup.ConfigureServices()
        // Note that you don't need to call AddMvc() here 
        public void ExtendConfigureServices(IServiceCollection services)
        {
            // Register the service responsible for payment business logic.
            

            // Register the respoistory responsible for getting and persisting payment data.
            services.AddTransient<IPaymentRequestRepository, PaymentRequestRepository>();

            // Register a provider which reads Vtex environment variables.
            services.AddSingleton<IVtexEnvironmentVariableProvider, VtexEnvironmentVariableProvider>();

            // Register the HttpContentAccessor responsible for getting the HttpContext from within other services (ie from within the payment repository).
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-context?view=aspnetcore-2.2 - The AddHttpContextAccessor extension method isnt avail until asp.net core 2.1, but the concept is the same.
            // https://blog.pedrofelix.org/2016/11/01/accessing-the-http-context-on-asp-net-core/ - The IHttpContextAccessor must be added as a singleton. 
            // The concreate implementation of HttpContextAccessor uses AsyncLocal storage to achieve request scoped data from a singleton.
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Register the HttpClient as a singleton.
            // This is not best practice as it doesnt handle cases where the DNS changes...
            // but its better practice than creating an instance on every requst and allowing socket starvation.
            // https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
            // http://byterot.blogspot.com/2016/07/singleton-httpclient-dns.html
            // the best practice is to use httpclientfactory in .net core 2.1 https://www.stevejgordon.co.uk/introduction-to-httpclientfactory-aspnetcore
            // but vtex currently only supports 2.0. I tried grabbing the nuget package with just httpclientfactory but that didnt work.
            // So singleton it is for this POC...
            services.AddSingleton<HttpClient>();
        }

        // This method is called inside Startup.Configure() before calling app.UseMvc()
        public void ExtendConfigureBeforeMvc(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {

        }

        // This method is called inside Startup.Configure() after calling app.UseMvc()
        public void ExtendConfigureAfterMvc(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {

        }
    }
}