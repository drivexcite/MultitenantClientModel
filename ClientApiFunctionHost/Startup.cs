using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using Microsoft.Extensions.ObjectPool;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Serilog.Events;
using Serilog.Extensions.Logging;
using ClientApiFunctionHost.Support;

[assembly: FunctionsStartup(typeof(ClientApiFunctionHost.Startup))]
namespace ClientApiFunctionHost
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            Environment.SetEnvironmentVariable("BASEDIR", AppContext.BaseDirectory);
            Environment.SetEnvironmentVariable("ConnectionStrings:ClientsDbConnectionString", "Server=(local);Database=Clients;Trusted_Connection=True;");

            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            var services = new ServiceCollection();
            var loggerFactory = new SerilogLoggerFactory(Log.Logger, false);
            var diagnosticListener = new DiagnosticListener("Microsoft.AspNetCore");

            services.AddSingleton<DiagnosticSource>(diagnosticListener);
            services.AddSingleton(diagnosticListener);
            services.AddSingleton<ObjectPoolProvider>(new DefaultObjectPoolProvider());
            services.AddSingleton<ILoggerFactory>(loggerFactory);
            services.AddSingleton<IConfiguration>(configuration);
            services.AddLogging();

            var startup = new ClientApi.Startup(configuration);
            startup.ConfigureServices(services);

            var serviceProvider = services.BuildServiceProvider();

            var applicationBuilder = new ApplicationBuilder(serviceProvider, new FeatureCollection());
            startup.Configure(applicationBuilder, new WebHostEnvironment { EnvironmentName = configuration["Environment"] });

            builder.Services.AddSingleton<ILoggerFactory>(loggerFactory);
            builder.Services.AddSingleton(serviceProvider);
            builder.Services.AddSingleton(applicationBuilder);
        }
    }
}
