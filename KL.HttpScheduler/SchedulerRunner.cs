using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        private ILogger<SchedulerRunner> Logger { get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sortedSetDequeueClient"></param>
        /// <param name="actionBlock"></param>
        /// <param name="logger"></param>
        public SchedulerRunner(
            SortedSetScheduleClient sortedSetDequeueClient,
            MyActionBlock actionBlock,
            ILogger<SchedulerRunner> logger
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
                    Logger.LogError(ex, "DequeueException");
                }

                if (httpJob != null)
                {
                    var success = ActionBlock.Post(httpJob);
                    using (Logger.BeginScope(new Dictionary<string, object>() {
                        {"id", httpJob.Id },
                        {"httpJob", JsonConvert.SerializeObject(httpJob) },
                        {"dequeued_diff", httpJob.DequeuedTime - httpJob.ScheduleDequeueTime }
                    }))
                    {
                        if (success)
                        {
                            Logger.LogInformation($"Id={httpJob.Id}. Queue for local execution: Success");
                        }
                        else
                        {
                            Logger.LogError($"Id={httpJob.Id}. Queue for local execution: Failed");
                        }
                    }

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
