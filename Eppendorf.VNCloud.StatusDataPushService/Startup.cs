// <copyright file="Startup.cs" company="Eppendorf AG - 2018">
// Copyright (c) Eppendorf AG - 2018. All rights reserved.
// </copyright>

using System.Linq;
using System.Threading.Tasks;
using Eppendorf.VNCloud.StatusDataPushService.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eppendorf.VNCloud.StatusDataPushService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddApiVersioning(o =>
                {
                    o.ReportApiVersions = true;
                    o.AssumeDefaultVersionWhenUnspecified = true;
                    o.DefaultApiVersion = new ApiVersion(1, 0);
                });

            var connectionString = Configuration["SignalR.ConnectionString"];

            services.AddSignalR().AddAzureSignalR(connectionString);
            services.AddHealthChecks();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHealthChecks("/api/heartbeat", new HealthCheckOptions()
            {
                AllowCachingResponses = false,
                ResponseWriter = WriteResponse
            });

            app.UseHttpsRedirection();
            app.UseMvc();

            app.UseFileServer();
            app.UseAzureSignalR(routes =>
            {
                routes.MapHub<ChatHub>("/chat");
            });
        }

        private static Task WriteResponse(HttpContext httpContext, HealthReport result)
        {
            httpContext.Response.ContentType = "application/json";

            var json = new JObject(
                new JProperty("status", result.Status.ToString()),
                new JProperty("results", new JObject(result.Entries.Select(pair =>
                    {
                        return new JProperty(pair.Key, new JObject(
                            new JProperty("status", pair.Value.Status.ToString()),
                            new JProperty("description", pair.Value.Description),
                            new JProperty("data", new JObject(pair.Value.Data.Select(
                                p => new JProperty(p.Key, p.Value))))));
                    }))));
            return httpContext.Response.WriteAsync(
                json.ToString(Formatting.Indented));
        }
    }
}
