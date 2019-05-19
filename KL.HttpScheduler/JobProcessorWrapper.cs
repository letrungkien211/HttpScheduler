using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using System;
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
        private TelemetryClient TelemetryClient { get; }
        private TimeSpan DefaultTimeout { get; } = TimeSpan.FromSeconds(5);

        public JobProcessorWrapper(IJobProcessor jobProcessor, TelemetryClient telemetryClient)
        {
            this.JobProcessor = jobProcessor;
            this.TelemetryClient = telemetryClient;
        }

        /// <summary>
        /// Process async
        /// </summary>
        /// <param name="httpJob"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task ProcessAsync(HttpJob httpJob)
        {
            try
            {
                using (var cancellationSource = new CancellationTokenSource(DefaultTimeout))
                {
                    await this.JobProcessor.ProcessAsync(httpJob, cancellationSource.Token).ConfigureAwait(false);
                }
                var telemetry = new EventTelemetry("ExecuteSuccess");
                telemetry.Context.Session.Id = httpJob.Id;
                this.TelemetryClient.TrackEvent(telemetry);
            }
            catch (Exception ex)
            {
                var telemetry = new ExceptionTelemetry(ex);
                telemetry.ProblemId = "ExecuteFailure";
                telemetry.Context.Session.Id = httpJob.Id;
                this.TelemetryClient.TrackException(telemetry);
            }
        }
    }
}
