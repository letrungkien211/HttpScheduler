using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace KL.HttpScheduler
{
    /// <summary>
    /// Scheduler runner
    /// </summary>
    public class SchedulerRunner
    {
        private SortedSetScheduleClient SortedSetDequeueClient { get; }
        private MyActionBlock ActionBlock { get; }
        private TelemetryClient TelemetryClient { get; }
        public SchedulerRunner(
            SortedSetScheduleClient sortedSetDequeueClient, 
            MyActionBlock actionBlock,
            TelemetryClient telemetryClient
            )
        {
            SortedSetDequeueClient = sortedSetDequeueClient;
            ActionBlock = actionBlock;
            TelemetryClient = telemetryClient;
        }
        /// <summary>
        /// Run async
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var httpJob = await SortedSetDequeueClient.DequeueAsync(cancellationToken).ConfigureAwait(false);
                if (httpJob != null)
                {
                    var success = ActionBlock.Post(httpJob);
                    var telemetry = new EventTelemetry(success ? "EnqueueSuccess": "EnqueueFailure");
                    telemetry.Context.Session.Id = httpJob.Id;
                    TelemetryClient.TrackEvent(telemetry);
                }
                else
                {
                    try
                    {
                        await Task.Delay(100, cancellationToken).ConfigureAwait(false);
                    }
                    catch
                    {
                        // 
                    }
                }
            }
        }
    }
}
