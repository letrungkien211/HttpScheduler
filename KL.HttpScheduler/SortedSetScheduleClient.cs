using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KL.HttpScheduler
{
    /// <summary>
    /// Redis scheduler
    /// </summary>
    public class SortedSetScheduleClient
    {
        private readonly IDatabase _database;
        private readonly string _sortedSetKey;
        private readonly string _hashKey;

        /// <summary>
        /// Redis scheduler
        /// </summary>
        public SortedSetScheduleClient(IDatabase database, string sortedSetKey, string hashKey)
        {
            _database = database;
            _sortedSetKey = sortedSetKey;
            _hashKey = hashKey;
        }

        /// <summary>
        /// Dequeue async
        /// </summary>
        /// <param name="jobs"></param>
        /// <returns></returns>
        public async Task<IEnumerable<(bool, Exception)>> ScheduleAsync(IEnumerable<HttpJob> jobs)
        {
            var idToJobs = new LinkedList<HashEntry>();
            var scheduleItems = new LinkedList<SortedSetEntry>();
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var ret = new List<(bool, Exception)>();
            foreach (var _ in jobs)
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
                    ret.Add((false, new ArgumentException(
                        $"Cannot schedule item in the past!. Now={now}, ScheduleDequeueTime={queueItem.ScheduleDequeueTime}, JobMessage={JsonConvert.SerializeObject(queueItem)}",
                        nameof(queueItem.ScheduleDequeueTime))));
                    continue;
                }

                if (await _database.HashExistsAsync(_hashKey, queueItem.Id).ConfigureAwait(false))
                {
                    ret.Add((false, new ArgumentException(
                        $"Job with id={queueItem.Id} already exists!",
                        nameof(queueItem.Id)
                        )));
                    continue;
                }

                ret.Add((true, null));

                idToJobs.AddLast(new HashEntry(queueItem.Id, redisValue));

                scheduleItems.AddLast(new SortedSetEntry(redisValue, queueItem.ScheduleDequeueTime));
            }

            if (idToJobs.Any())
                await _database.HashSetAsync(_hashKey, idToJobs.ToArray()).ConfigureAwait(false);

            if (scheduleItems.Any())
            {
                await _database.SortedSetAddAsync(_sortedSetKey, scheduleItems.ToArray()).ConfigureAwait(false);
            }
            return ret;
        }

        /// <summary>
        /// Cancel async
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task CancelAsync(string id)
        {
            var job = await _database.HashGetAsync(_hashKey, id).ConfigureAwait(false);
            if (job.IsNullOrEmpty)
                throw new KeyNotFoundException($"Id={id} was not found!");
            await _database.HashDeleteAsync(_hashKey, id).ConfigureAwait(false);
            var ret = await _database.SortedSetRemoveAsync(_sortedSetKey, job).ConfigureAwait(false);
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
            var val = await _database.HashGetAsync(_hashKey, id).ConfigureAwait(false);
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
            return (await _database.SortedSetRangeByRankAsync(_sortedSetKey).ConfigureAwait(false)).Select(x => JsonConvert.DeserializeObject<HttpJob>(x));
        }

        /// <summary>
        /// Count
        /// </summary>
        /// <returns></returns>
        public Task<long> CountAsync()
        {
            return _database.SortedSetLengthAsync(_sortedSetKey);
        }

        /// <summary>
        /// Dequeue async
        /// </summary>
        /// <returns></returns>
        public async Task<HttpJob> DequeueAsync()
        {
            var now = (double)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var val = await _database.SortedSetRangeByScoreAsync(_sortedSetKey, 0, now, Exclude.None, Order.Ascending, 0, 1).ConfigureAwait(false);
            if (val == null || !val.Any())
            {
                return null;
            }

            var message = JsonConvert.DeserializeObject<HttpJob>(val[0]);

            if (!await _database.LockTakeAsync($"{_sortedSetKey}_{message.Id}", "Lock", TimeSpan.FromSeconds(5)).ConfigureAwait(false))
                return null;

            try
            {
                if (!await _database.SortedSetRemoveAsync(_sortedSetKey, val[0]).ConfigureAwait(false)
                    || !await _database.HashDeleteAsync(_hashKey, message.Id).ConfigureAwait(false)
                    )
                {
                    return null;
                }

                return message;
            }
            finally
            {
                await _database.LockReleaseAsync($"{_sortedSetKey}_{message.Id}", "Lock").ConfigureAwait(false);
            }
        }
    }
}
