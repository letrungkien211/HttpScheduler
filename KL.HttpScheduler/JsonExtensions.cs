using Newtonsoft.Json;

namespace KL.HttpScheduler
{
    /// <summary>
    /// Json extensions
    /// </summary>
    public static class JsonExtensions
    {
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        /// <summary>
        /// To json str. Ignore null.
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public static string ToJsonStr(this object obj)
        {
            return JsonConvert.SerializeObject(obj, JsonSettings);
        }
    }
}
