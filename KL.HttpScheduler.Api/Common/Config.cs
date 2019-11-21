using System;

namespace KL.HttpScheduler.Api.Common
{
    /// <summary>
    /// Configuration
    /// </summary>
    public class Config
    {
        /// <summary>
        /// Decide if the background runner task pulling from redis cache would run or not.
        /// </summary>
        public bool IsNotRunner { get; set; } = false;

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

        /// <summary>
        /// Health Base Path
        /// </summary>
        public Uri HealthBasePath { get; set; } = new Uri("http://localhost/");
    }
}
