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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using System.Text.Encodings.Web;
using System.Security.Claims;

[assembly: FunctionsStartup(typeof(ClientApiFunctionHost.Startup))]
namespace ClientApiFunctionHost
{
    public static class HackyAuthenticationExtensions
    {
        public static AuthenticationBuilder AddAuthentication(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddAuthenticationCore();
            services.AddWebEncoders();
            services.TryAddSingleton<ISystemClock, SystemClock>();
            return new AuthenticationBuilder(services);
        }

        public static AuthenticationBuilder AddAuthentication(this IServiceCollection services, string defaultScheme)
            => services.AddAuthentication(o => o.DefaultScheme = defaultScheme);

        public static AuthenticationBuilder AddAuthentication(this IServiceCollection services, Action<AuthenticationOptions> configureOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            var builder = services.AddAuthentication();
            services.Configure(configureOptions);
            return builder;
        }

        // Used to ensure that there's always a sign in scheme
        private class EnsureSignInScheme<TOptions> : IPostConfigureOptions<TOptions> where TOptions : RemoteAuthenticationOptions
        {
            private readonly AuthenticationOptions _authOptions;

            public EnsureSignInScheme(IOptions<AuthenticationOptions> authOptions)
            {
                _authOptions = authOptions.Value;
            }

            public void PostConfigure(string name, TOptions options)
            {
                options.SignInScheme = options.SignInScheme ?? _authOptions.DefaultSignInScheme;
            }
        }

        public static AuthenticationBuilder AddNullHandler(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<NullAuthenticationOptions> configureOptions)
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<NullAuthenticationOptions>, NullAuthenticationPostConfigureOptions>());
            return builder.AddScheme<NullAuthenticationOptions, NullAuhtenticationHandler>(authenticationScheme, displayName, configureOptions);
        }
    }

    public class NullAuthenticationPostConfigureOptions : IPostConfigureOptions<NullAuthenticationOptions>
    {
        public void PostConfigure(string name, NullAuthenticationOptions options)
        {
        }
    }

    public class NullAuthenticationOptions : AuthenticationSchemeOptions
    {

    }

    public class NullAuhtenticationHandler : AuthenticationHandler<NullAuthenticationOptions>
    {
        public NullAuhtenticationHandler(IOptionsMonitor<NullAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var principal = new ClaimsPrincipal();
            var claims = new[]
            {
                new Claim("Granted", "yes"),
                new Claim(ClaimTypes.Role, "Bar"),
                new Claim(ClaimTypes.NameIdentifier, "Foo")
            };

            principal.AddIdentity(new ClaimsIdentity(claims, Scheme.Name));

            var result = AuthenticateResult.Success(new AuthenticationTicket(principal, new AuthenticationProperties(), Scheme.Name));
            return Task.FromResult(result);
        }
    }

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

            var issuer = configuration["TokenProviderOptions:Issuer"];
            var audience = configuration["TokenProviderOptions:Audience"];

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "WebJobsAuthLevel";
                    options.DefaultScheme = "WebJobsAuthLevel";
                    options.DefaultChallengeScheme = "WebJobsAuthLevel";
                })
                .AddNullHandler("WebJobsAuthLevel", "WebJobsAuthLevel", options => { })
                .AddNullHandler(JwtBearerDefaults.AuthenticationScheme, JwtBearerDefaults.AuthenticationScheme, options => { });

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
