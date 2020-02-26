using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ClientApiFunctionHost.Support;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;

namespace ClientApiFunctionHost
{
    public class ServiceCollectionContainer
    {
        public ServiceCollection ServiceCollection { get; set; }
        public WebHostEnvironment HostingEnvironment { get; set; }
        public ClientApi.Startup Initializer { get; set; }
    }

    public class ProxyFunction
    {
        private readonly ServiceCollectionContainer _serviceCollectionContainer;

        public ProxyFunction(ServiceCollectionContainer serviceCollectionContainer)
        {
            _serviceCollectionContainer = serviceCollectionContainer;
        }

        [FunctionName("ProxyFunction")]
        public async Task<IActionResult> DelegateInvocation([HttpTrigger(AuthorizationLevel.Anonymous, Route = "{*any}")] HttpRequest request, ILogger log)
        {
            try
            {
                var services = _serviceCollectionContainer.ServiceCollection;
                var environment = _serviceCollectionContainer.HostingEnvironment;
                var startup = _serviceCollectionContainer.Initializer;

                var serviceProvider = services.BuildServiceProvider();

                var applicationBuilder = new ApplicationBuilder(serviceProvider, new FeatureCollection());
                startup.Configure(applicationBuilder, environment);

                request.HttpContext.RequestServices = serviceProvider;

                var handleRequest = applicationBuilder.Build();
                await handleRequest(request.HttpContext);
            }
            catch (Exception e)
            {
                log.LogError(e.ToString());
            }

            // This response object is redundant. 
            // The HttpContext will have whatever IActionResult set by the ASP.NET Pipeline.
            return new EmptyResult();
        }
    }
}
