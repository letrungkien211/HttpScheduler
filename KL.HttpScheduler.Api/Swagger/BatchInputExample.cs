using KL.HttpScheduler.Api.Models;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;

namespace KL.HttpScheduler.Api.Swagger
{
    internal class BatchInputExample : IExamplesProvider<BatchInput>
    {
        public BatchInput GetExamples()
        {
            return new BatchInput()
            {
                Jobs = new List<HttpJob>()
                {
                    new HttpJob()
                    {
                        Body = "",
                        Headers = new Dictionary<string, string>(),
                        ScheduleDequeueTime = -2000,
                        Id = Guid.NewGuid().ToString(),
                        Uri = new Uri("https://github.com"),
                        ContentType = "",
                        HttpMethod = "GET"
                    }
                }
            };
        }
    }
}
