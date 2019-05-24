using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace KL.HttpScheduler
{
    /// <summary>
    /// Mock job processor
    /// </summary>
    public class MockJobProcessor : IJobProcessor
    {
        /// <summary>
        /// Dict
        /// </summary>
        private IDictionary<string, HttpJob> Dict { get; } = new ConcurrentDictionary<string, HttpJob>();

        /// <summary>
        /// Process async
        /// </summary>
        /// <param name="httpJob"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<HttpStatusCode> ProcessAsync(HttpJob httpJob, CancellationToken cancellationToken)
        {
            Dict[httpJob.Id] = httpJob;
            return Task.FromResult(HttpStatusCode.OK);
        }

        /// <summary>
        /// Get by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public HttpJob Get(string id)
        {
            return Dict.TryGetValue(id, out var val) ? val : null;
        }
    }
}