using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Logging;
using Serilog.Events;
using Serilog.Extensions.Logging;
using ClientApiFunctionHost.Support;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;

[assembly: FunctionsStartup(typeof(ClientApiFunctionHost.Startup))]
namespace ClientApiFunctionHost
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            Environment.SetEnvironmentVariable("BASEDIR", AppContext.BaseDirectory);
            Environment.SetEnvironmentVariable("ConnectionStrings:ClientsDbConnectionString", "Server=(local);Database=Clients;Trusted_Connection=True;");
            Environment.SetEnvironmentVariable("DisableAuthenticationAndAuthorization", "false");
            Environment.SetEnvironmentVariable("TokenProviderOptions:Issuer", "https://dev-318215.okta.com/oauth2/aus15ubkj0WHGsqEV4x6");
            Environment.SetEnvironmentVariable("TokenProviderOptions:Audience", "UserManagement");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            var loggerFactory = new SerilogLoggerFactory(Log.Logger, false);
            builder.Services.AddSingleton<ILoggerFactory>(loggerFactory);

            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            var services = new ServiceCollection();
            var diagnosticListener = new DiagnosticListener("Microsoft.AspNetCore");

            services.AddSingleton<DiagnosticSource>(diagnosticListener);
            services.AddSingleton(diagnosticListener);
            services.AddSingleton<ObjectPoolProvider>(new DefaultObjectPoolProvider());
            services.AddSingleton<ILoggerFactory>(loggerFactory);
            services.AddSingleton<IConfiguration>(configuration);
            services.AddLogging();

            var startup = new ClientApi.Startup(configuration);
            startup.ConfigureServices(services);

            var environment = new WebHostEnvironment { EnvironmentName = configuration["Environment"] };

            builder.Services.AddSingleton<IApplicationBuilder>(provider =>
            {
                var serviceProvider = provider.GetService<ServiceProvider>();
                var applicationBuilder = new ApplicationBuilder(serviceProvider, new FeatureCollection());

                startup.Configure(applicationBuilder, environment);
                return applicationBuilder;
            });

            builder.Services.AddScoped(provider => services.BuildServiceProvider());
            builder.Services.AddSingleton(provider => provider.GetService<IApplicationBuilder>().Build());
        }
    }
}
