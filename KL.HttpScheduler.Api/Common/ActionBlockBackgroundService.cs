using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace KL.HttpScheduler.Api.Common
{
    internal class ActionBlockBackgroundService : BackgroundService
    {
        public ActionBlockBackgroundService(MyActionBlock actionBlock)
        {
            ActionBlock = actionBlock;
        }

        private MyActionBlock ActionBlock { get; }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() => ActionBlock.RunAsync(stoppingToken));
        }
    }
}
