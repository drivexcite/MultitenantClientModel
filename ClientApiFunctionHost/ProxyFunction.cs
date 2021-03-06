using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Microsoft.Extensions.DependencyInjection;

namespace ClientApiFunctionHost
{
    public class ProxyFunction
    {
        private readonly RequestDelegate _aspNetPipeline;
        private readonly ServiceProvider _serviceProvider;

        public ProxyFunction(RequestDelegate aspNetPipeline, ServiceProvider serviceProvider)
        {
            _aspNetPipeline = aspNetPipeline;
            _serviceProvider = serviceProvider;
        }

        [FunctionName("ProxyFunction")]
        public async Task<IActionResult> DelegateInvocation([HttpTrigger(AuthorizationLevel.Anonymous, Route = "{*any}")] HttpRequest request, ILogger log)
        {
            request.HttpContext.RequestServices = _serviceProvider;
            await _aspNetPipeline(request.HttpContext);

            // This response object is redundant. 
            // The HttpContext will have whatever IActionResult set by the ASP.NET Pipeline.
            return new EmptyResult();
        }
    }
}
