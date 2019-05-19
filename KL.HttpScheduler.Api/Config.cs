﻿namespace KL.HttpScheduler.Api
{
    /// <summary>
    /// Configuration
    /// </summary>
    public class Config
    {
        /// <summary>
        /// Redis connection string
        /// </summary>
        public string RedisConnectionString { get; set; } = "localhost:6379,ssl=False,abortConnect=False";

        /// <summary>
        /// Hash key
        /// </summary>
        public string HashKey { get; set; } = "hash";

        /// <summary>
        /// Sorted set key
        /// </summary>
        public string SortedSetKey { get; set; } = "sortedset";

        /// <summary>
        /// Unit test
        /// </summary>
        public bool UnitTest { get; set; } = false;

        /// <summary>
        /// Swagger base path
        /// </summary>
        public string SwaggerBasePath { get; set; } = "/";
    }
}
