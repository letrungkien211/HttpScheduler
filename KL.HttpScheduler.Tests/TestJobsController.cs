using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace KL.HttpScheduler.Api.Tests
{
    public class TestJobsController : IClassFixture<WebApplicationFactory<Startup>>
    {
        private WebApplicationFactory<Startup> Factory { get; }

        private ITestOutputHelper TestOutputHelper { get; }

        public TestJobsController(WebApplicationFactory<Startup> factory, ITestOutputHelper testOutputHelper)
        {
            Environment.SetEnvironmentVariable("Config__UnitTest", "true");
            Factory = factory;
            TestOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task TestSchedulerRunner()
        {
            var client = Factory.CreateClient();

            using (var cancelSource = new CancellationTokenSource())
            {
                var runner = Factory.Services.GetRequiredService<SchedulerRunner>();
                var task = Task.Run(() => runner.RunAsync(cancelSource.Token));
                var req = new HttpRequestMessage(HttpMethod.Post, "api/jobs");
                var job = new HttpJob()
                {
                    Id = Guid.NewGuid().ToString("N"),
                    ScheduleDequeueTime = DateTimeOffset.UtcNow.AddSeconds(2).ToUnixTimeMilliseconds(),
                    Uri = new Uri("http://localhost/")
                };
                req.Content = new StringContent(JsonConvert.SerializeObject(job), Encoding.UTF8, "application/json");

                var res = await client.SendAsync(req);
                Assert.Equal(HttpStatusCode.OK, res.StatusCode);

                req = new HttpRequestMessage(HttpMethod.Get, $"api/jobs/{job.Id}");
                res = await client.SendAsync(req);
                Assert.True(res.IsSuccessStatusCode);

                await Task.Delay(3000);

                var processor = Factory.Services.GetRequiredService<IJobProcessor>() as MockJobProcessor;

                Assert.NotNull(processor);
                var processedJob = processor.Get(job.Id);
                Assert.NotNull(processedJob);

                req = new HttpRequestMessage(HttpMethod.Get, $"api/jobs/{job.Id}");
                res = await client.SendAsync(req);
                Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);

                cancelSource.Cancel();
                await task;

            }
        }

        [Fact]
        public async Task TestExpiredScheduling()
        {
            var client = Factory.CreateClient();

            var req = new HttpRequestMessage(HttpMethod.Post, "api/jobs");
            var job = new HttpJob()
            {
                Id = Guid.NewGuid().ToString("N"),
                ScheduleDequeueTime = DateTimeOffset.UtcNow.AddSeconds(-10).ToUnixTimeMilliseconds(),
                Uri = new Uri("http://localhost/")
            };
            req.Content = new StringContent(JsonConvert.SerializeObject(job), Encoding.UTF8, "application/json");

            var res = await client.SendAsync(req);
            Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
            TestOutputHelper.WriteLine(await res.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData("http://localhost:5000/")]
        public async Task TestOnlineScheduleLocalHost(string baseAddress)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(baseAddress)
            };

            using (var cancelSource = new CancellationTokenSource(3000))
            {
                try
                {
                    await client.GetAsync("", cancelSource.Token);
                }
                catch
                {
                    return;
                }
            }
            var req = new HttpRequestMessage(HttpMethod.Post, "api/jobs");
            var job = new HttpJob()
            {
                Id = Guid.NewGuid().ToString("N"),
                ScheduleDequeueTime = DateTimeOffset.UtcNow.AddSeconds(2).ToUnixTimeMilliseconds(),
                Uri = new Uri("http://localhost/")
            };
            req.Content = new StringContent(JsonConvert.SerializeObject(new { Jobs = new List<HttpJob>() { job } }), Encoding.UTF8, "application/json");

            var res = await client.SendAsync(req);
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        }

        [Fact]
        public async Task TestCancel()
        {
            var client = Factory.CreateClient();

            var job = new HttpJob()
            {
                Id = Guid.NewGuid().ToString("N"),
                ScheduleDequeueTime = DateTimeOffset.UtcNow.AddSeconds(2).ToUnixTimeMilliseconds(),
                Uri = new Uri("http://localhost/")
            };

            for (var i = 0; i < 2; i++)
            {
                var req = new HttpRequestMessage(HttpMethod.Post, "api/jobs")
                {
                    Content = new StringContent(JsonConvert.SerializeObject(job), Encoding.UTF8, "application/json")
                };
                var res = await client.SendAsync(req);

                if (i == 0)
                {
                    Assert.Equal(HttpStatusCode.OK, res.StatusCode);
                }
                else
                {
                    Assert.Equal(HttpStatusCode.Conflict, res.StatusCode);
                }
            }


            Assert.True((await client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, $"api/jobs/{job.Id}"))).IsSuccessStatusCode);

            Assert.Equal(HttpStatusCode.NotFound, (await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"api/jobs/{job.Id}"))).StatusCode);
        }
    }
}
