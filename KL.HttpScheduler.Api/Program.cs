using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace KL.HttpScheduler.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((ctx, builder) =>
                {
                    var config = builder.Build();
                    var endpoint = config["KeyVaultEndpoint"];
                    if (!string.IsNullOrEmpty(endpoint))
                    {
                        builder.AddAzureKeyVault(
                            config["KeyVaultEndpoint"],
                            config["KeyVaultClient"],
                            config["KeyVaultSecret"]
                            );
                    }
                })
                .UseApplicationInsights()
                .UseStartup<Startup>();
    }
}
