﻿using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KL.HttpScheduler
{
    /// <summary>
    /// Redis scheduler
    /// </summary>
    public class SortedSetScheduleClient
    {
        private IDatabase Database { get; }
        private string SortedSetKey { get; }
        private string HashKey { get; }
        private TelemetryClient Logger { get; }

        /// <summary>
        /// Redis scheduler
        /// </summary>
        public SortedSetScheduleClient(
            IDatabase database,
            string sortedSetKey,
            string hashKey,
            TelemetryClient logger
            )
        {
            Database = database;
            SortedSetKey = sortedSetKey;
            HashKey = hashKey;
            Logger = logger;
        }

        /// <summary>
        /// Dequeue async
        /// </summary>
        /// <param name="jobs"></param>
        /// <returns></returns>
        public async Task<(bool, Exception)> ScheduleAsync(IEnumerable<HttpJob> jobs)
        {
            // Make a copy of original jobs
            var jobList = jobs.Select(job => JsonConvert.DeserializeObject<HttpJob>(JsonConvert.SerializeObject(job))).ToList();

            var idToJobs = new LinkedList<HashEntry>();
            var scheduleItems = new LinkedList<SortedSetEntry>();
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var success = true;
            Exception ex = null;
            foreach (var job in jobList)
            {
                job.EnqueuedTime = now;
                if (job.ScheduleDequeueTime < 0)
                {
                    job.ScheduleDequeueTime = now - job.ScheduleDequeueTime;
                }

                var redisValue = (RedisValue)JsonConvert.SerializeObject(job);

                // Cannot schedule past job. 1000ms to compensate time different among servers and network latency.
                if (job.ScheduleDequeueTime < now - 1000)
                {
                    success = false;
                    ex = new ArgumentException(
                        $"Id={job.Id} Cannot schedule item in the past!. Now={now}, ScheduleDequeueTime={job.ScheduleDequeueTime}, JobMessage={JsonConvert.SerializeObject(job)}",
                        nameof(job.ScheduleDequeueTime));
                    break;
                }

                if (await Database.HashExistsAsync(HashKey, job.Id).ConfigureAwait(false))
                {
                    success = false;
                    ex = new ConflictException(
                        $"Id={job.Id} already exists!"
                        );
                    break;
                }

                idToJobs.AddLast(new HashEntry(job.Id, redisValue));

                scheduleItems.AddLast(new SortedSetEntry(redisValue, job.ScheduleDequeueTime));
            }

            if (idToJobs.Any())
                await Database.HashSetAsync(HashKey, idToJobs.ToArray()).ConfigureAwait(false);

            if (scheduleItems.Any())
            {
                await Database.SortedSetAddAsync(SortedSetKey, scheduleItems.ToArray()).ConfigureAwait(false);
            }

            var jobListJsonStr = JsonConvert.SerializeObject(jobList);
            foreach (var job in jobList)
            {
                var str = success ? "Success" : $"Failure. Ex={ex}";
                var telemetry = new TraceTelemetry($"Id={job.Id}. Schedule: {str}.")
                {
                    SeverityLevel = success ? SeverityLevel.Information : SeverityLevel.Error
                };
                telemetry.Properties["httpJob"] = JsonConvert.SerializeObject(job);
                telemetry.Properties["httpJobs"] = jobListJsonStr;
                telemetry.Context.Operation.Id = job.Id;
                Logger.TrackTrace(telemetry);
            }
            return (success, ex);
        }

        /// <summary>
        /// Cancel async
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task CancelAsync(string id)
        {
            var job = await Database.HashGetAsync(HashKey, id).ConfigureAwait(false);
            if (job.IsNullOrEmpty)
                throw new KeyNotFoundException($"Id={id} was not found!");
            await Database.HashDeleteAsync(HashKey, id).ConfigureAwait(false);
            var ret = await Database.SortedSetRemoveAsync(SortedSetKey, job).ConfigureAwait(false);
            if (!ret)
                throw new KeyNotFoundException($"Id={id} was not found!");
        }

        /// <summary>
        /// Get job by id. Only cancellable job can be retrieved by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<HttpJob> GetAsync(string id)
        {
            var val = await Database.HashGetAsync(HashKey, id).ConfigureAwait(false);
            if (val.HasValue)
            {
                return JsonConvert.DeserializeObject<HttpJob>(val);
            }
            return null;
        }

        /// <summary>
        /// List async
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<HttpJob>> ListAsync(int start = 0, int count = 1000)
        {
            return (await Database.SortedSetRangeByRankAsync(SortedSetKey, start: start, stop: start + count - 1).ConfigureAwait(false)).Select(x => JsonConvert.DeserializeObject<HttpJob>(x));
        }

        /// <summary>
        /// Count
        /// </summary>
        /// <returns></returns>
        public Task<long> CountAsync()
        {
            return Database.SortedSetLengthAsync(SortedSetKey);
        }

        /// <summary>
        /// Dequeue async
        /// </summary>
        /// <returns></returns>
        public async Task<HttpJob> DequeueAsync()
        {
            var now = (double)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var val = await Database.SortedSetRangeByScoreAsync(SortedSetKey, 0, now, Exclude.None, Order.Ascending, 0, 1).ConfigureAwait(false);
            if (val == null || !val.Any())
            {
                return null;
            }

            var httpJob = JsonConvert.DeserializeObject<HttpJob>(val[0]);

            if (!await Database.LockTakeAsync($"{SortedSetKey}_{httpJob.Id}", "Lock", TimeSpan.FromSeconds(5)).ConfigureAwait(false))
                return null;

            try
            {
                if (!await Database.SortedSetRemoveAsync(SortedSetKey, val[0]).ConfigureAwait(false)
                    || !await Database.HashDeleteAsync(HashKey, httpJob.Id).ConfigureAwait(false)
                    )
                {
                    return null;
                }
                httpJob.DequeuedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                return httpJob;
            }
            finally
            {
                await Database.LockReleaseAsync($"{SortedSetKey}_{httpJob.Id}", "Lock").ConfigureAwait(false);
            }
        }
    }
}
