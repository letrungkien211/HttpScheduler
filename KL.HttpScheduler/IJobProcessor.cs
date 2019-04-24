using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KL.HttpScheduler
{
    /// <summary>
    /// Job Processor
    /// </summary>
    public interface IJobProcessor
    {
        /// <summary>
        /// Processor
        /// </summary>
        /// <param name="httpJob"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task ProcessAsync(HttpJob httpJob, CancellationToken cancellationToken);
    }

    public class MockJobProcessor : IJobProcessor
    {
        public IDictionary<string, HttpJob> Dict = new ConcurrentDictionary<string, HttpJob>();
        public Task ProcessAsync(HttpJob httpJob, CancellationToken cancellationToken)
        {
            Dict[httpJob.Id] = httpJob;
            return Task.FromResult(0);
        }

        public HttpJob Get(string id)
        {
            return Dict.TryGetValue(id, out var val) ? val : null;
        }
    }
}