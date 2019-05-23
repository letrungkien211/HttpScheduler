using Microsoft.ApplicationInsights;
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
        public async Task<IEnumerable<(bool, Exception)>> ScheduleAsync(IEnumerable<HttpJob> jobs)
        {
            var jobList = jobs.ToList();

            var idToJobs = new LinkedList<HashEntry>();
            var scheduleItems = new LinkedList<SortedSetEntry>();
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var rets = new List<(bool, Exception)>();
            foreach (var _ in jobList)
            {
                var queueItem = JsonConvert.DeserializeObject<HttpJob>(JsonConvert.SerializeObject(_));
                queueItem.EnqueuedTime = now;
                if (queueItem.ScheduleDequeueTime < 0)
                {
                    queueItem.ScheduleDequeueTime = now - queueItem.ScheduleDequeueTime;
                }

                var redisValue = (RedisValue)JsonConvert.SerializeObject(queueItem);

                if (queueItem.ScheduleDequeueTime < now)
                {
                    rets.Add((false, new ArgumentException(
                        $"Cannot schedule item in the past!. Now={now}, ScheduleDequeueTime={queueItem.ScheduleDequeueTime}, JobMessage={JsonConvert.SerializeObject(queueItem)}",
                        nameof(queueItem.ScheduleDequeueTime))));
                    continue;
                }

                if (await Database.HashExistsAsync(HashKey, queueItem.Id).ConfigureAwait(false))
                {
                    rets.Add((false, new ConflictException(
                        $"Job with id={queueItem.Id} already exists!"
                        )));
                    continue;
                }

                rets.Add((true, null));

                idToJobs.AddLast(new HashEntry(queueItem.Id, redisValue));

                scheduleItems.AddLast(new SortedSetEntry(redisValue, queueItem.ScheduleDequeueTime));
            }

            if (idToJobs.Any())
                await Database.HashSetAsync(HashKey, idToJobs.ToArray()).ConfigureAwait(false);

            if (scheduleItems.Any())
            {
                await Database.SortedSetAddAsync(SortedSetKey, scheduleItems.ToArray()).ConfigureAwait(false);
            }

            // Log here
            for (var i = 0; i < rets.Count; i++)
            {

                var telemetry = new TraceTelemetry(string.Format($"Id={jobList[i].Id}. Schedule: {0}", rets[i].Item1 ? "Success" : "Failure"))
                {
                    SeverityLevel = rets[i].Item1 ? SeverityLevel.Information : SeverityLevel.Error
                };
                telemetry.Context.Operation.Id = jobList[i].Id;
                Logger.TrackTrace(telemetry);

            }
            return rets;
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
        public async Task<IEnumerable<HttpJob>> ListAsync()
        {
            return (await Database.SortedSetRangeByRankAsync(SortedSetKey).ConfigureAwait(false)).Select(x => JsonConvert.DeserializeObject<HttpJob>(x));
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
