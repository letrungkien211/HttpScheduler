using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace KL.HttpScheduler.Api.Health
{
    /// <summary>
    /// Check if queue is healthy or not
    /// </summary>
    internal class QueueHealthCheck : IHealthCheck
    {
        public QueueHealthCheck(MyActionBlock myActionBlock)
        {
            MyActionBlock = myActionBlock;
        }

        public MyActionBlock MyActionBlock { get; }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;
            MyActionBlock.Post(now);
            var stopwatch = Stopwatch.StartNew();
            while (!cancellationToken.IsCancellationRequested && stopwatch.ElapsedMilliseconds < 2000)
            {
                if (MyActionBlock.LatestDequeued >= now)
                {
                    if (stopwatch.ElapsedMilliseconds < 1000)
                    {
                        return HealthCheckResult.Healthy($"Dequeue Time: {stopwatch.Elapsed}");
                    }
                    else
                    {
                        return HealthCheckResult.Degraded($"Dequeue Time: {stopwatch.Elapsed}");
                    }
                }
                await Task.Delay(10);
            }
            return HealthCheckResult.Unhealthy("Queue is stuck");
        }
    }
}
