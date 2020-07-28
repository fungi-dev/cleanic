using Cleanic.Application;
using Cleanic.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace Cleanic.Framework
{
    public abstract class StartupBase
    {
        public StartupBase(IConfiguration configuration)
        {
            AspNetAppConfig = configuration;
            SerializationOptions = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                PropertyNameCaseInsensitive = true
            };
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Application base
            services.AddSingleton(LanguageInfoType);
            services.AddSingleton(x => (LanguageInfo)x.GetRequiredService(LanguageInfoType));
            services.AddSingleton<IEventStore>(x => new MongoEventStore(AspNetAppConfig["MongoDb:ConnectionString"], x.GetRequiredService<ILogger<MongoEventStore>>(), (LanguageInfo)x.GetRequiredService(LanguageInfoType)));

            // Write application
            services.AddSingleton(DomainInfoType);
            services.AddSingleton(x => (DomainInfo)x.GetRequiredService(DomainInfoType));
            services.AddSingleton<ICommandBus, InMemoryCommandBus>();
            services.AddSingleton<CommandAgent>();
            services.AddSingleton<SagaAgent>();
            RegisterDomainServices(services);
            services.AddTransient<Func<Type, Service[]>>(sp => type => sp.GetServices<Service>().Where(s => type.IsAssignableFrom(s.GetType())).ToArray());
            services.AddSingleton<WriteApplicationFacade>();

            // Read application
            services.AddSingleton(ProjectionsInfoType);
            services.AddSingleton(x => (ProjectionsInfo)x.GetRequiredService(ProjectionsInfoType));
            services.AddSingleton<IProjectionStore>(x => new MongoProjectionStore(AspNetAppConfig["MongoDb:ConnectionString"], x.GetRequiredService<ILogger<MongoProjectionStore>>(), (ProjectionsInfo)x.GetRequiredService(ProjectionsInfoType)));
            services.AddSingleton<ProjectionsAgent>();
            RegisterQueryRunners(services);
            services.AddTransient<Func<Type, QueryRunner>>(sp => type => sp.GetServices<QueryRunner>().Single(qr => qr.Query.Type == type));
            services.AddSingleton<ReadApplicationFacade>();

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = AspNetAppConfig["Locations:Identity"];
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false,
                        NameClaimType = "name",
                        RoleClaimType = "role"
                    };
                });
            services.AddAuthorization();
            services.AddCors();

            ConfigureServicesPostAction(services);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ProjectionsAgent _, CommandAgent __, SagaAgent ___, WriteApplicationFacade writeApp, ReadApplicationFacade readApp)
        {
            WriteApp = writeApp;
            ReadApp = readApp;

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapPost("/{aggregate:alpha}/{aggregateId:regex(^[a-zA-Z0-9-_]+$)}/{command:alpha}", ProcessCommandRequest).RequireAuthorization();
                endpoints.MapGet("/{aggregate:alpha}/{aggregateId:regex(^[a-zA-Z0-9-_]+$)}/{query:alpha}", ProcessQueryRequest).RequireAuthorization();

                endpoints.MapPost("/service/init-data", ProcessInitDataRequest).RequireAuthorization(new AuthorizeAttribute { Roles = "admin" });
                endpoints.MapPost("/service/rebuild-projections/{aggregate:alpha}/{projection:alpha}", ProcessRebuildProjectionsRequest).RequireAuthorization(new AuthorizeAttribute { Roles = "admin" });

                AddCustomEndpoints(endpoints);
            });
        }

        protected abstract Type LanguageInfoType { get; }
        protected abstract Type DomainInfoType { get; }
        protected abstract Type ProjectionsInfoType { get; }
        protected abstract void RegisterDomainServices(IServiceCollection services);
        protected abstract void RegisterQueryRunners(IServiceCollection services);
        protected abstract Task ProcessInitDataRequest(HttpContext httpContext);
        protected virtual void ConfigureServicesPostAction(IServiceCollection services) { }
        protected virtual void AddCustomEndpoints(IEndpointRouteBuilder endpointRouteBuilder) { }

        protected WriteApplicationFacade WriteApp;
        protected ReadApplicationFacade ReadApp;
        protected readonly IConfiguration AspNetAppConfig;
        protected readonly JsonSerializerOptions SerializationOptions;

        private async Task ProcessCommandRequest(HttpContext context)
        {
            var aggregateName = context.Request.RouteValues["aggregate"].ToString();
            var aggregateId = context.Request.RouteValues["aggregateId"].ToString();
            var commandName = context.Request.RouteValues["command"].ToString();
            var language = (LanguageInfo)context.RequestServices.GetRequiredService(LanguageInfoType);
            var commandType = language.FindCommand(aggregateName, commandName);
            using var reader = new StreamReader(context.Request.Body);
            var requestBodyString = await reader.ReadToEndAsync();
            var command = (Command)Activator.CreateInstance(commandType);
            if (!String.IsNullOrWhiteSpace(requestBodyString)) command = (Command)JsonSerializer.Deserialize(requestBodyString, commandType, SerializationOptions);
            command.AggregateId = aggregateId;

            await WriteApp.Do(command);
            context.Response.StatusCode = StatusCodes.Status202Accepted;
        }

        private async Task ProcessQueryRequest(HttpContext context)
        {
            var authorizedTenants = context.User.Claims.Where(x => x.Type == "tenant").Select(x => x.Value);

            var aggregateName = context.Request.RouteValues["aggregate"].ToString();
            var aggregateId = context.Request.RouteValues["aggregateId"].ToString();
            var queryName = context.Request.RouteValues["query"].ToString();
            var language = (LanguageInfo)context.RequestServices.GetRequiredService(LanguageInfoType);
            var queryType = language.FindQuery(aggregateName, queryName);
            var queryParams = context.Request.Query.Keys.Cast<String>().ToDictionary(k => k, v => context.Request.Query[v].Single());
            var queryParamsJson = JsonSerializer.Serialize(queryParams, SerializationOptions);
            var query = (Query)JsonSerializer.Deserialize(queryParamsJson, queryType, SerializationOptions);
            query.AggregateId = aggregateId;

            var result = await ReadApp.Get(query, authorizedTenants);
            if (result != null)
            {
                var responseBodyString = JsonSerializer.Serialize(result, result.GetType(), SerializationOptions);
                context.Response.ContentType = new MediaTypeHeaderValue("application/json").ToString();
                await context.Response.WriteAsync(responseBodyString, Encoding.UTF8);
            }
        }

        private async Task ProcessRebuildProjectionsRequest(HttpContext context)
        {
            var aggregateName = context.Request.RouteValues["aggregate"].ToString();
            var projectionName = context.Request.RouteValues["projection"].ToString();
            var projections = (ProjectionsInfo)context.RequestServices.GetRequiredService(ProjectionsInfoType);
            var projectionInfo = projections.FindProjection(aggregateName, projectionName);

            await ReadApp.RebuildProjections(projectionInfo);
        }
    }
}