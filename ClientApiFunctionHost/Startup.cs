using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using Microsoft.Extensions.ObjectPool;
using ClientApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;

[assembly: FunctionsStartup(typeof(ClientApiFunctionHost.Startup))]
namespace ClientApiFunctionHost
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Of course this magic won't work because: https://github.com/Azure/azure-functions-host/issues/5447
            Environment.SetEnvironmentVariable("BASEDIR", AppContext.BaseDirectory);

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddEnvironmentVariables()
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            var services = builder.Services;

            services.AddSingleton<DiagnosticSource>(new DiagnosticListener("Microsoft.AspNetCore"));
            services.AddSingleton<ObjectPoolProvider>(new DefaultObjectPoolProvider());
            services.AddSingleton<ILoggerFactory>(new SerilogLoggerFactory(Log.Logger, false));
            services.AddSingleton<IConfiguration>(configuration);

            var startup = new ClientApiStartup(configuration);
            startup.ConfigureServices(services);

            var serviceProvider = services.BuildServiceProvider();

            /* Initialize Application builder */
            var appicationBuilder = new ApplicationBuilder(serviceProvider, new FeatureCollection());
            /* Configure the HTTP request pipeline */
            startup.Configure(appicationBuilder, new WebHostEnvironment { EnvironmentName = configuration["Environment"] });

            services.AddSingleton<ServiceProvider>(serviceProvider);
            services.AddSingleton<ApplicationBuilder>(appicationBuilder);
        }
    }
}
