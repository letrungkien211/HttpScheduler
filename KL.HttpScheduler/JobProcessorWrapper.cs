using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        private ILogger<JobProcessorWrapper> Logger { get; }
        private TimeSpan DefaultTimeout { get; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="jobProcessor"></param>
        /// <param name="logger"></param>
        public JobProcessorWrapper(IJobProcessor jobProcessor, ILogger<JobProcessorWrapper> logger)
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
            using (Logger.BeginScope(new Dictionary<string, object>() {
                        {"id", httpJob.Id },
                        {"httpJob", JsonConvert.SerializeObject(httpJob) }
                    }))
            {
                try
                {
                    using (var cancellationSource = new CancellationTokenSource(DefaultTimeout))
                    {
                        await this.JobProcessor.ProcessAsync(httpJob, cancellationSource.Token).ConfigureAwait(false);
                    }
                    Logger.LogTrace($"Id={httpJob.Id}. ExecuteSuccess");
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"Id={httpJob.Id}. ExecuteException");
                }
            }
        }
    }
}
