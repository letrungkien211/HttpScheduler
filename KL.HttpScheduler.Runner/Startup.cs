using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace KL.HttpScheduler.Runner
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
            var config = Configuration.GetSection("Config").Get<Config>() ?? new Config();

            services.AddHttpClient(ForwardJobProcessor.ForwardClientName, client =>
            {
                client.BaseAddress = new Uri("http://localhost:5000/api/jobs/execute");
            });

            services.AddSingleton<IDatabase>(_ =>
            {
                return (ConnectionMultiplexer.Connect(config.RedisConnectionString)).GetDatabase();
            });

            services.AddSingleton<IJobProcessor, ForwardJobProcessor>();

            services.AddSingleton<JobProcessorWrapper>();
            services.AddSingleton<TelemetryClient>();

            services.AddSingleton<ActionBlock<HttpJob>>(provider =>
            {
                var jobProcessor = provider.GetService<JobProcessorWrapper>();

                return new ActionBlock<HttpJob>((httpJob) =>
                {
                    return jobProcessor.ProcessAsync(httpJob);
                });
            });
            services.AddSingleton<SchedulerRunner>();

            services.AddSingleton<SortedSetScheduleClient>(provider =>
            {
                return new SortedSetScheduleClient(provider.GetService<IDatabase>(), config.SortedSetKey, config.HashKey);
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime applicationLifetime)
        {
            var schedulerRunner = app.ApplicationServices.GetService<SchedulerRunner>();
            var manualEvent = new ManualResetEventSlim();
            var cancelSource = new CancellationTokenSource();

            applicationLifetime.ApplicationStarted.Register(() =>
            {
                Task.Run(async () =>
                {
                    await schedulerRunner.RunAsync(cancelSource.Token).ConfigureAwait(false);
                    manualEvent.Set();
                    Console.WriteLine("Background stopped");
                });
            });

            applicationLifetime.ApplicationStopped.Register(() =>
            {
                cancelSource.Cancel();
                if (manualEvent.Wait(TimeSpan.FromSeconds(2)))
                {
                    Console.WriteLine("Gracefully shutdown");
                }
                else
                {
                    Console.Error.WriteLine("Shutdown incorrectly");
                }
                cancelSource.Dispose();
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseMvc();
        }
    }
}
