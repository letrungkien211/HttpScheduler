using HealthChecks.UI.Client;
using KL.HttpScheduler.Api.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace KL.HttpScheduler.Api.Health
{
    internal static class HealthConfigExtensions
    {
        public static IServiceCollection AddMyHealthChecks(this IServiceCollection services, Config config)
        {
            services.AddHealthChecks()
                    .AddUrlGroup(config.HealthBasePath, "base")
                    .AddCheck<QueueHealthCheck>("queue")
                    .AddRedis(config.RedisConnectionString);
            return services;
        }

        public static void MapCustomHealthChecks(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapHealthChecks("/health/live", new HealthCheckOptions()
            {
                Predicate = check => check.Tags.Contains("live"),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                ResultStatusCodes = {
                            [HealthStatus.Healthy] = StatusCodes.Status200OK,
                            [HealthStatus.Degraded] = StatusCodes.Status200OK,
                            [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                        }
            });

            endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions()
            {
                Predicate = check => check.Tags.Contains("ready"),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                ResultStatusCodes = {
                            [HealthStatus.Healthy] = StatusCodes.Status200OK,
                            [HealthStatus.Degraded] = StatusCodes.Status200OK,
                            [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                        }
            });

            endpoints.MapHealthChecks("/health", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                ResultStatusCodes = {
                            [HealthStatus.Healthy] = StatusCodes.Status200OK,
                            [HealthStatus.Degraded] = StatusCodes.Status200OK,
                            [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                        }
            });
        }
    }
}
