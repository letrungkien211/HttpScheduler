using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KL.HttpScheduler
{
    /// <summary>
    /// Job processor
    /// </summary>
    public class JobProcessorWrapper
    {
        private readonly IJobProcessor jobProcessor;
        private readonly TelemetryClient telemetryClient;

        public JobProcessorWrapper(IJobProcessor jobProcessor, TelemetryClient telemetryClient)
        {
            this.jobProcessor = jobProcessor;
            this.telemetryClient = telemetryClient;
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
                using (var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
                {
                    await this.jobProcessor.ProcessAsync(httpJob, cancellationSource.Token).ConfigureAwait(false);
                }
                var telemetry = new EventTelemetry("Execute");
                telemetry.Properties["HttpJob"] = JsonConvert.SerializeObject(httpJob);
                this.telemetryClient.TrackEvent(telemetry);
            }
            catch (Exception ex)
            {
                var telemetry = new ExceptionTelemetry(ex);
                telemetry.Properties["HttpJob"] = JsonConvert.SerializeObject(httpJob);
                telemetry.ProblemId = "ExecuteFailure";
                this.telemetryClient.TrackException(telemetry);
            }
        }
    }
}
