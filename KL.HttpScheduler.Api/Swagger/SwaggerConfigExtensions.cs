using KL.HttpScheduler.Api.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace KL.HttpScheduler.Api.Swagger
{
    internal static class SwaggerConfigExtensions
    {
        public static IServiceCollection AddMySwagger(this IServiceCollection services)
        {

            services.AddSwaggerExamplesFromAssemblyOf<Config>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Http Jobs Scheduler", Version = "v1" });
                c.ExampleFilters();

                var xmlFiles = new[] {
                    $"{Assembly.GetExecutingAssembly().GetName().Name}.xml",
                    $"{typeof(HttpJob).Assembly.GetName().Name}.xml"
                };
                foreach (var xmlFile in xmlFiles)
                {
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    if (File.Exists(xmlPath))
                        c.IncludeXmlComments(xmlPath);
                }
            });

            return services;
        }

        public static IApplicationBuilder UseMySwagger(this IApplicationBuilder app)
        {
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger(c =>
            {
                c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                {
                    swaggerDoc.Servers = new List<OpenApiServer> {
                        new OpenApiServer {
                            Url = app.ApplicationServices.GetRequiredService<Config>().SwaggerBasePath
                        }
                    };
                });
            });

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("v1/swagger.json", "Http Jobs Scheduler");
            });

            return app;
        }
    }
}
