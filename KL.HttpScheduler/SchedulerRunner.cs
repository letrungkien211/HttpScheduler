﻿using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KL.HttpScheduler
{
    /// <summary>
    /// Scheduler runner
    /// </summary>
    public class SchedulerRunner
    {
        private SortedSetScheduleClient SortedSetDequeueClient { get; }
        private MyActionBlock ActionBlock { get; }
        private TelemetryClient Logger { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sortedSetDequeueClient"></param>
        /// <param name="actionBlock"></param>
        /// <param name="logger"></param>
        public SchedulerRunner(
            SortedSetScheduleClient sortedSetDequeueClient,
            MyActionBlock actionBlock,
            TelemetryClient logger
            )
        {
            SortedSetDequeueClient = sortedSetDequeueClient;
            ActionBlock = actionBlock;
            Logger = logger;
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
                HttpJob httpJob = null;

                try
                {
                    httpJob = await SortedSetDequeueClient.DequeueAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Logger.TrackException(ex);
                }

                if (httpJob != null)
                {
                    Logger.GetMetric("ScheduleDequeueLatency").TrackValue(httpJob.DequeuedTime - httpJob.ScheduleDequeueTime);

                    var success = ActionBlock.Post(httpJob);
                    var str = success ? "Success" : "Failure";
                    var telemetry = new TraceTelemetry($"Id={httpJob.Id}. Queue for local execution: {str } ");
                    telemetry.Context.Operation.Id = httpJob.Id;
                    telemetry.Properties["httpJob"] = JsonConvert.SerializeObject(httpJob);
                    Logger.TrackTrace(telemetry);
                    continue;
                }

                try
                {
                    await Task.Delay(100, cancellationToken).ConfigureAwait(false);
                }
                catch
                {
                    // Ignore
                }
            }
        }
    }
}
