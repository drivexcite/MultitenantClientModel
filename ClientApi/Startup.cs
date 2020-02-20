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
            services.AddAutoMapper(typeof(AccountProfile));

            var clientsDbConnectionString = Configuration["ConnectionStrings:ClientsDbConnectionString"];
            services.AddDbContext<ClientsDb>(options => options.UseSqlServer(clientsDbConnectionString));

            services.AddScoped<CreateAccountDelegate>();
            services.AddScoped<GetAccountDelegate>();
            services.AddScoped<GetSubscriptionDelegate>();
            services.AddScoped<GetIdentityProvidersDelegate>();
            services.AddScoped<CreateIdentityProviderDelegate>();

            //var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration.["TokenProviderOptions:SecretKey"]));

            var issuer = Configuration["TokenProviderOptions:Issuer"];
            var audience = Configuration["TokenProviderOptions:Audience"];

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;

                    options.Audience = audience;
                    options.Authority = issuer;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        RequireExpirationTime = true,
                        RequireSignedTokens = true,
                        ValidateIssuerSigningKey = false,
                        //IssuerSigningKey = signingKey,
                        ValidateIssuer = true,
                        ValidIssuer = issuer,
                        ValidateAudience = true,
                        ValidAudience = audience,
                        ValidateLifetime = false,
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
                        OnMessageReceived = context =>
                        {
                            return Task.CompletedTask;
                        },
                        OnChallenge = context =>
                        {
                            return Task.CompletedTask;
                        },
                        OnForbidden = context =>
                        {
                            return Task.CompletedTask;
                        },
                        OnAuthenticationFailed = context =>
                        {
                            return Task.CompletedTask;
                        },
                    };
                });

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

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
