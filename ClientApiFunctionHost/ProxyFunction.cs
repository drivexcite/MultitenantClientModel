using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;

namespace ClientApiFunctionHost
{
    public class ProxyFunction
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly ApplicationBuilder _applicationBuilder;

        public ProxyFunction(ServiceProvider serviceProvider, ApplicationBuilder applicationBuilder)
        {
            _serviceProvider = serviceProvider;
            _applicationBuilder = applicationBuilder;
        }

        [FunctionName("ProxyFunction")]
        public async Task<IActionResult> DelegateInvocation([HttpTrigger(AuthorizationLevel.Anonymous, Route = "{*any}")] HttpRequest request, ILogger log)
        {
            request.HttpContext.RequestServices = _serviceProvider;
            var handleRequest = _applicationBuilder.Build();

            await handleRequest(request.HttpContext);

            // This response object is redundant. 
            // The HttpContext will have whatever IActionResult set by the ASP.NET Pipeline.
            return new EmptyResult();
        }
    }
}
