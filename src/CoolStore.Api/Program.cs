﻿using CoolStore.Catalog;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Serilog;
using System.Net;
using System.Threading.Tasks;

namespace CoolStore.Api
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            var builder = WebApplicationHost.CreateDefaultBuilder(args);

            builder
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel((ctx, options) =>
                    {
                        if (ctx.HostingEnvironment.IsDevelopment())
                        {
                            IdentityModelEventSource.ShowPII = true;
                        }

                        options.Limits.MinRequestBodyDataRate = null;
                        options.Listen(IPAddress.Any, 5002);
                        options.Listen(IPAddress.Any, 15002, listenOptions =>
                        {
                            listenOptions.Protocols = HttpProtocols.Http2;
                        });
                    });
                });

            builder.UseSerilog();

            builder.Services
                .AddCatalogComponents(builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false).Build())
                .AddGrpc();

            var app = builder.Build();

            app.MapGrpcService<CatalogApi>();

            app.MapGet("/", async context =>
            {
                await context.Response.WriteAsync("Hello World");
            });

            await app.RunAsync();
        }
    }
}
