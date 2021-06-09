using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GeneralInsurance.Api.Services;
using GeneralInsurance.Api.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json.Converters;
using App.Metrics.Health;
using System.Net.Http;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using GeneralInsurance.Api.Authorisation;
using GeneralInsurance.Api.Validators;
using GeneralInsurance.CustomerValidation;
using GeneralInsurance.Interfaces.Interfaces;
using GeneralInsurance.DataAccess.Middleware;
using GeneralInsurance.DataAccess.Middleware.ChannelFactory;
using GeneralInsurance.DataAccess.Middleware.Entities;
using Castle.DynamicProxy;
using GeneralInsurance.DataAccess;

namespace GeneralInsurance.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            // The following line is added due to an issue in dotnet core 2.1 when using htpclient behind an authenticating proxy - https://github.com/dotnet/corefx/issue
            AppContext.SetSwitch("System.Net.Htpp.UseSocketsHttpHandler",false);
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.0000'",
                        options.SerializerSettings.Converters.Add(new StringEnumConverter());
                });
            services.AddMvcCore()
                .AddApiExplorer();

            services.AddHttpClient(HttpClients.ApigeeClient, client =>
                {
                    client.Timeout =
                        TimeSpan.FromSeconds(90); // overall client timeout, all processing stops when this limit is hit
                })
                .AddPolicyHandler(HttpPolicyExtensions
                    .HandleTransientHttpError() // retry on http 5xx & 408 (timeout) response plus any http request exception thrown.
                    .Or<TimeoutRejectedException
                    >() // retry when TimeoutRejectedException raised internally by polly when a timeout policy is reached
                    .WaitAndRetryAsync(3,
                        retryAttempt =>
                            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))) // retry 3 times with exponential back off
                .AddPolicyHandler(PolicyServiceCollectionExtensions.TimeoutAsync<HttpResponseMessage>(10)); //each polly initiated retry will timeout after 10 seconds (and throw a TimeoutRejectedException

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var tokenValidationService = services.BuildServiceProvider().GetService<ITokenValidationService>();
                    options.TokenValidationParameters = tokenValidationService.GetTokenValidationParameters();
                });

            services.AddAuthorization(auth =>
            {
                auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build());
                auth.AddPolicy("HasReadAccess", new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                    .RequireRole("customers.generalinsurance.write")
                    .AddRequirements(new AcceptedContextTypeRequirement(IdentityContextTypes.customer, IdentityContextTypes.staff))
                    .Build());
            });

            var metrics = AppMetricsHealth.CreateDefaultBuilder()
                .HealthChecks.RegisterFromAssembly(services)
                .BuildAndAddTo(services);
            services.AddHealth(metrics);
            services.AddHealthEndpoints();

            services.AddTransient<ILineService, LinkService>();
            services.AddTransient<ICustomerMiddlewareStore, CustomerMiddlewareStore>();
            services.AddTransient<IWcfChannelFactory<IInitialiseParty>, WcfChannelFactory<IInitialiseParty>>();
            services.AddTransient<ISearchMiddlewareStore, SearchMiddlewareStore>();
            services.AddTransient<IWcfChannelFactory<IReportParty>, WcfChannelFactory2<IReportParty>>();
            services.AddTransient<IMaintainMiddlewareStore, MaintainMiddlewareStore>();
            services.AddTransient<IWcfChannelFactory<IMaintainParty>, WcfChannelFactory3<IMaintainParty>>();
            services.AddTransient<ICustomerOrderMiddlewareStore, CustomerOrderMiddlewareStore>();
            services.AddTransient<IWcfChannelFactory<IInitialiseCustomerOrder>, WcfChannelFactory4<IInitialiseCustomerOrder>>();

            services.AddTransient<IWcfCredentialsFactory, WcfCredentialsService>();
            services.AddTransient<IProxyGenerator, ProxyGenerator>();

            services.AddTransient<IInsuranceRequestValidator, InsuranceRequestValidator>();
            services.AddTransient<IValidationItemsFactory, ValidationItemsFactory>();
            services.AddTransient<ICustomerAccessValidation, CustomerAccessValidationLibrary>();

            services.AddSingleton<ITokenValidationService, TokenValidationService>();
            services.AddSingleton<IAuthorizationHandler, SystemContextTypeHandler>();
            services.AddSingleton<IAuthorizationHandler, CustomerContextTypeHandler>();
            services.AddSingleton<IAuthorizationHandler, StaffContextTypeHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.UseRequestResponseLogging();
            app.UseCustomHeaders();
            app.UseMiddleware<MandatoryCorrelationId>();
            app.UseMvc();
        }
    }
}
