using Microsoft.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace KL.HttpScheduler.Api.Common
{
    internal static class DIExtensions
    {
        public static IServiceCollection AddHttpJobProcessor(this IServiceCollection services, Config config)
        {
            services.AddSingleton<IJobProcessor, HttpJobProcessor>()
                    .AddSingleton<JobProcessorWrapper>()
                    .AddSingleton<MyActionBlock>()
                    .AddHttpClient()
                    .AddHostedService<ActionBlockBackgroundService>();
            if (!config.IsNotRunner)
            {
                services.AddSingleton<SchedulerRunner>()
                        .AddHostedService<SchedulerBackgroundService>();
            }

            return services;
        }

        public static IServiceCollection AddRedis(this IServiceCollection services, Config config)
        {
            services.AddSingleton<IDatabase>(_ => (IDatabase)ConnectionMultiplexer.Connect(config.RedisConnectionString).GetDatabase())
                    .AddSingleton<SortedSetScheduleClient>(provider =>
            {
                return new SortedSetScheduleClient(
                        provider.GetService<MyActionBlock>(),
                        provider.GetService<IDatabase>(),
                        config.SortedSetKey, config.HashKey,
                        provider.GetService<TelemetryClient>()
                        );
            });
            return services;
        }

        public static IServiceCollection AddForUnitTests(this IServiceCollection services, bool isUnitTest)
        {
            if (isUnitTest)
            {
                services.AddSingleton<IJobProcessor, MockJobProcessor>();
            }
            return services;
        }

        public static IServiceCollection AddMyMvc(this IServiceCollection services)
        {
            services.AddRouting(options => options.LowercaseUrls = true)
            .AddControllers()
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            });
            return services;
        }
    }
}
