using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace KL.HttpScheduler
{
    /// <summary>
    /// Job processor
    /// </summary>
    public class JobProcessorWrapper
    {
        private IJobProcessor JobProcessor { get; }
        private TelemetryClient Logger { get; }
        private TimeSpan DefaultTimeout { get; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="jobProcessor"></param>
        /// <param name="logger"></param>
        public JobProcessorWrapper(IJobProcessor jobProcessor, TelemetryClient logger)
        {
            this.JobProcessor = jobProcessor;
            this.Logger = logger;
        }

        /// <summary>
        /// Process async
        /// </summary>
        /// <param name="httpJob"></param>
        /// <returns></returns>
        public async Task ProcessAsync(HttpJob httpJob)
        {
            try
            {
                HttpStatusCode statusCode;
                using (var cancellationSource = new CancellationTokenSource(DefaultTimeout))
                {
                    statusCode = await this.JobProcessor.ProcessAsync(httpJob, cancellationSource.Token).ConfigureAwait(false);
                }
                var telemetry = new TraceTelemetry($"Id={httpJob.Id}. ExecuteSuccess. Return StatusCode={statusCode}");
                telemetry.Context.Operation.Id = httpJob.Id;
                telemetry.Properties["httpJob"] = JsonConvert.SerializeObject(httpJob);
                Logger.TrackTrace(telemetry);
            }
            catch (Exception ex)
            {
                var telemetry = new ExceptionTelemetry(ex);
                telemetry.Context.Operation.Id = httpJob.Id;
                telemetry.Properties["httpJob"] = JsonConvert.SerializeObject(httpJob);
                telemetry.Message = $"Id={httpJob.Id}. ExecuteException";
                Logger.TrackException(ex);
            }
        }
    }
}
