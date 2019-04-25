namespace KL.HttpScheduler
{
    /// <summary>
    /// Configuration
    /// </summary>
    public class Config
    {
        public string RedisConnectionString { get; set; } = "localhost:6379, ssl=False,abortConnect=False";
        public string HashKey { get; set; } = "hash";
        public string SortedSetKey { get; set; } = "sortedset";
        public bool EnableForward { get; set; } = false;
    }
}
