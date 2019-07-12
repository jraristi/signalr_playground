﻿// <copyright file="Startup.cs" company="Eppendorf AG - 2018">
// Copyright (c) Eppendorf AG - 2018. All rights reserved.
// </copyright>

using System;
using System.Linq;
using System.Threading.Tasks;
using Eppendorf.VNCloud.StatusDataPushService.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eppendorf.VNCloud.StatusDataPushService
{
    public class Startup
    {
        // We use a key generated on this server during startup to secure our tokens.
        // This means that if the app restarts, existing tokens become invalid. It also won't work
        // when using multiple servers.
        public static readonly SymmetricSecurityKey SecurityKey = new SymmetricSecurityKey(Guid.NewGuid().ToByteArray());

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

            services.AddAuthentication()
                .AddJwtBearer(options => {
                    options.Events = new JwtBearerEvents 
                    {
                        OnMessageReceived = context => 
                        {
                            var accessToken = context.Request.Query["access_token"];
                            if (string.IsNullOrEmpty(accessToken) == false) {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

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

            app.Use(
                (context, next) =>
                {
                if (context.Request.Headers.TryGetValue("Authorization", out StringValues pathBase))
                {
                    var test = "";
                }

                if (context.Request.Headers.TryGetValue("Authorization", out StringValues proto))
                {
                    var test = "";
                }

                // context.Response.Headers.Add("Access-Control-Allow-Headers",new StringValues("authorization"));
                return next();
                });

            app.UseStaticFiles();
            app.UseMiddleware<WebSocketsMiddleware>();
            app.UseAuthentication();
            
            app.UseAzureSignalR(routes =>
            {
                routes.MapHub<ChatHub>("/chat");
            });
            app.UseMvc();
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
