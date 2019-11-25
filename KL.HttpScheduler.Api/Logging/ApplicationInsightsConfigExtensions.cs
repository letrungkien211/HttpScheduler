using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;
using Microsoft.Extensions.DependencyInjection;

namespace KL.HttpScheduler.Api.Logging
{
    internal static class ApplicationInsightsConfigExtensions
    {
        public static IServiceCollection AddApplicationInsights(this IServiceCollection services, ApplicationInsightsConfig appInsightsConfig)
        {
            services.AddApplicationInsightsTelemetry(o =>
            {
                o.EnableAdaptiveSampling = false;
                o.InstrumentationKey = appInsightsConfig.InstrumentationKey;
            });
            services.AddSingleton<TelemetryClient>(provider => new TelemetryClient(provider.GetRequiredService<TelemetryConfiguration>()));

            services.ConfigureTelemetryModule<QuickPulseTelemetryModule>((module, o) =>
            {
                if (!string.IsNullOrEmpty(appInsightsConfig.ApiKey))
                    module.AuthenticationApiKey = appInsightsConfig.ApiKey;
            });

            services.AddApplicationInsightsTelemetryProcessor<ApplicationInsightsFilter>();

            return services;
        }
    }
}
