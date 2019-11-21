using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;

namespace KL.HttpScheduler.Api.Models
{
    internal class HttpJobExample : IExamplesProvider<HttpJob>
    {
        public HttpJob GetExamples()
        {
            return new HttpJob()
            {
                Body = "",
                Headers = new Dictionary<string, string>(),
                ScheduleDequeueTime = -2000,
                Id = Guid.NewGuid().ToString(),
                Uri = new Uri("https://github.com"),
                ContentType = "",
                HttpMethod = "GET"
            };
        }
    }

    internal class HttpJobsExample : IExamplesProvider<IEnumerable<HttpJob>>
    {
        public IEnumerable<HttpJob> GetExamples()
        {
            yield return new HttpJob()
            {
                Body = "",
                Headers = new Dictionary<string, string>(),
                ScheduleDequeueTime = -2000,
                Id = Guid.NewGuid().ToString(),
                Uri = new Uri("https://github.com"),
                ContentType = "",
                HttpMethod = "GET"
            };
        }
    }
}
