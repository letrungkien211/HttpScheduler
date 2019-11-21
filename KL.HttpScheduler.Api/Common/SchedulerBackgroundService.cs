using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace KL.HttpScheduler.Api.Common
{
    internal class SchedulerBackgroundService : BackgroundService
    {
        public SchedulerBackgroundService(SchedulerRunner schedulerRunner)
        {
            SchedulerRunner = schedulerRunner;
        }

        public SchedulerRunner SchedulerRunner { get; }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return SchedulerRunner.RunAsync(stoppingToken);
        }
    }
}
