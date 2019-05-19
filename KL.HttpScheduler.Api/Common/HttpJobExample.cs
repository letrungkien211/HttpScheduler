using Swashbuckle.AspNetCore.Examples;
using System;
using System.Collections.Generic;

namespace KL.HttpScheduler.Api.Common
{
    internal class HttpJobExample : IExamplesProvider
    {
        public object GetExamples()
        {
            return new HttpJob()
            {
                Body = "",
                Headers = new Dictionary<string, string>(),
                ScheduleDequeueTime = -1000,
                Id = Guid.NewGuid().ToString(),
                Uri = new Uri("https://github.com"),
                ContentType = "",
                HttpMethod = "GET"
            };
        }
    }
}
