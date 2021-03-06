﻿using System.Net;
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
        Task<HttpStatusCode> ProcessAsync(HttpJob httpJob, CancellationToken cancellationToken);
    }
}