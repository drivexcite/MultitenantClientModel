using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using AutoMapper;
using ClientApi.Authorization;
using ClientModel.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ClientModel.DataAccess.Create.CreateAccount;
using ClientModel.DataAccess.Create.CreateIdentityProvider;
using ClientModel.DataAccess.Get.GetAccount;
using ClientModel.DataAccess.Get.GetIdentityProviders;
using ClientModel.DataAccess.Get.GetSubscriptions;
using ClientModel.Dtos.Mappings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ClientModel.DataAccess.Create.CreateDataLink;
using ClientModel.DataAccess.Create.CreateSubscription;
using ClientModel.DataAccess.Get.GetDataLink;

namespace ClientApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddApplicationPart(GetType().Assembly);
            services.AddAutoMapper(typeof(AccountProfile).Assembly);

            var clientsDbConnectionString = Configuration["ConnectionStrings:ClientsDbConnectionString"];

            // For Testing
            if (clientsDbConnectionString.StartsWith("InMemory:", StringComparison.OrdinalIgnoreCase))
            {
                var databaseName = clientsDbConnectionString.Replace("InMemory:", "", StringComparison.OrdinalIgnoreCase);
                services.AddDbContext<ClientsDb>(options => options.UseInMemoryDatabase(databaseName: databaseName));
            }
            else
            {
                services.AddDbContext<ClientsDb>(options => options.UseSqlServer(clientsDbConnectionString));
            }

            services.AddScoped<CreateAccountDelegate>();
            services.AddScoped<CreateSubscriptionDelegate>();
            services.AddScoped<CreateIdentityProviderDelegate>();
            services.AddScoped<CreateDataLinkDelegate>();

            services.AddScoped<GetAccountDelegate>();
            services.AddScoped<GetSubscriptionDelegate>();
            services.AddScoped<GetIdentityProvidersDelegate>();
            services.AddScoped<GetDataLinkDelegte>();
            
            var signingKey = Configuration["TokenProviderOptions:SecretKey"] is string secretKey
                ? new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey))
                : null;

            var issuer = Configuration["TokenProviderOptions:Issuer"];
            var audience = Configuration["TokenProviderOptions:Audience"];
            var requireHttpsMetadata = Configuration["TokenProviderOptions:RequireHttpsMetadata"] == "true";
            var requireExpirationTime = Configuration["TokenProviderOptions:RequireExpirationTime"] == "true";
            var requireSignedTokens = Configuration["TokenProviderOptions:RequireSignedTokens"] == "true";
            var validateIssuerSigningKey = Configuration["TokenProviderOptions:ValidateIssuerSigningKey"] == "true";
            var validateIssuer = Configuration["TokenProviderOptions:ValidateIssuer"] == "true";
            var validateAudience = Configuration["TokenProviderOptions:ValidateAudience"] == "true";
            var validateLifetime = Configuration["TokenProviderOptions:ValidateLifetime"] == "true";

            var disableAuthenticationAndAuthorization = Configuration["DisableAuthenticationAndAuthorization"] == "true";

            if (!disableAuthenticationAndAuthorization)
            {
                services
                    .AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    })
                    .AddJwtBearer(options =>
                    {
                        options.RequireHttpsMetadata = requireHttpsMetadata;

                        options.Audience = audience;
                        options.Authority = issuer;
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            RequireExpirationTime = requireExpirationTime,
                            RequireSignedTokens = requireSignedTokens,
                            ValidateIssuerSigningKey = validateIssuerSigningKey,
                            IssuerSigningKey = signingKey,
                            ValidateIssuer = validateIssuer,
                            ValidIssuer = issuer,
                            ValidateAudience = validateAudience,
                            ValidAudience = audience,
                            ValidateLifetime = validateLifetime,
                            ClockSkew = TimeSpan.Zero
                        };

                        options.SaveToken = true;
                        options.Events = new JwtBearerEvents
                        {
                            OnTokenValidated = context =>
                            {
                                var jwt = (context.SecurityToken as JwtSecurityToken)?.ToString();
                                return Task.CompletedTask;
                            },
                            OnMessageReceived = context => Task.CompletedTask,
                            OnChallenge = context => Task.CompletedTask,
                            OnForbidden = context => Task.CompletedTask,
                            OnAuthenticationFailed = context => Task.CompletedTask,
                        };
                    });
            }

            services.AddSingleton<IAuthorizationHandler, RbacAuthorizationHandler>();
            services.AddSingleton<IAuthorizationPolicyProvider, RbacAuhtorizationPolicyProvider>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var disableHttpsRedirection = Configuration["DisableHttpsRedirection"] == "true";

            if (!disableHttpsRedirection)
            {
                app.UseHttpsRedirection();
            }

            app.UseRouting();

            var disableAuthenticationAndAuthorization = Configuration["DisableAuthenticationAndAuthorization"] == "true";

            if (!disableAuthenticationAndAuthorization)
            {
                app.UseAuthentication();
                app.UseAuthorization();
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
