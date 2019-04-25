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
        private ActionBlock<HttpJob> ActionBlock { get; }
        public SchedulerRunner(
            SortedSetScheduleClient sortedSetDequeueClient, 
            ActionBlock<HttpJob> actionBlock
            )
        {
            SortedSetDequeueClient = sortedSetDequeueClient;

            ActionBlock = actionBlock;
            
            // new ActionBlock<HttpJob>(async (httpJob) =>
            // {
            //     try
            //     {
            //         using(var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            //         {
            //             await jobProcessor.ProcessAsync(httpJob, cancellationSource.Token).ConfigureAwait(false);
            //         }
            //     }
            //     catch(Exception)
            //     {
            //         // Put logging here
            //     }
            // });
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
                    if(!ActionBlock.Post(httpJob)){
                        // Put error log here.
                    }
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
