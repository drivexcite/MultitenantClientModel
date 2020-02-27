using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ClientModel.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ClientApi.Filters
{
    public class ClientModelExceptionFilter : ExceptionFilterAttribute
    {
        public static Dictionary<Type, HttpStatusCode> DesiredStatusCodes { get; set; }

        static ClientModelExceptionFilter()
        {
            DesiredStatusCodes = new Dictionary<Type, HttpStatusCode>
            {
                [typeof(AccountNotFoundException)] = HttpStatusCode.NotFound,
                [typeof(SubscriptionNotFoundException)] = HttpStatusCode.NotFound,
                [typeof(ExistingAccountException)] = HttpStatusCode.BadRequest,
                [typeof(MalformedAccountException)] = HttpStatusCode.BadRequest,
                [typeof(MalformedSubscriptionException)] = HttpStatusCode.BadRequest,
                [typeof(PersistenceException)] = HttpStatusCode.InternalServerError,
                [typeof(ClientModelAggregateException)] = HttpStatusCode.BadRequest,
                [typeof(DataLinkTypeNotFoundException)] = HttpStatusCode.NotFound,
                [typeof(IdentityProviderNotFoundException)] = HttpStatusCode.NotFound
            };
        }

        private readonly ILogger<ClientModelExceptionFilter> _logger;
        private readonly IConfiguration _configuration;

        public ClientModelExceptionFilter(ILogger<ClientModelExceptionFilter> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        private void CommonHandler(ExceptionContext context)
        {
            var exception = context.Exception;
            var request = context.HttpContext.Request;
            var baseUrl = $"{request?.Scheme}://{request?.Host}{request?.PathBase}{request?.Path}?{request?.QueryString}";

            var isDomainSpecificException = DesiredStatusCodes.TryGetValue(exception.GetType(), out var suggestedStatusCode);

            object message = new { error = exception.Message };
            var statusCode = context.HttpContext.Response.StatusCode;

            if (isDomainSpecificException)
            {
                statusCode = (int)suggestedStatusCode;

                if (exception is ClientModelAggregateException aggregateException)
                {
                    message = new
                    {
                        result = aggregateException.Message,
                        details = (
                            from i in aggregateException.InnerExceptions
                            select new { error = i.Message }
                        ).ToList()
                    };
                }

                _logger.LogDebug($"A domain specific exception was raised while calling {baseUrl}", exception);
            }
            else
            {
                // This prevents any application state from propagating to the client in the HTTP response.
                // Unless of course, we are in a development environment.
#if !DEBUG
                message = new { error = "An unexpected error occurred." };

#else
                message = new { error = context.Exception.GetBaseException().Message };
#endif

                _logger.LogError($"An unexpected exception was raised while calling {baseUrl}", exception);
            }

            context.Result = new JsonResult(message);

            context.HttpContext.Response.ContentType = "application/json";
            context.HttpContext.Response.StatusCode = statusCode;
        }

        public override void OnException(ExceptionContext context)
        {
            var disableGlobalExceptionFilter = _configuration["DisableGlobalExceptionFilter"] == "true";

            if (context != null && !disableGlobalExceptionFilter)
                CommonHandler(context);

            base.OnException(context);
        }

        public override Task OnExceptionAsync(ExceptionContext context)
        {
            var disableGlobalExceptionFilter = _configuration["DisableGlobalExceptionFilter"] == "true";

            if (context != null && !disableGlobalExceptionFilter)
                CommonHandler(context);

            return base.OnExceptionAsync(context);
        }
    }
}