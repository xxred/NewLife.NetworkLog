using System;
using System.IO;
using System.Threading;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NewLife.Agent;
using IHost = Microsoft.Extensions.Hosting.IHost;

namespace NewLife.NetworkLog
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder1(args).Build().Run();
            //var worker = new Worker();
            //worker.Main(args);
        }

        class Worker : Agent.ServiceBase
        {
            private readonly IWebHost _host;
            private bool _stopRequestedByWindows;
            public Worker()
            {
                this.ServiceName = "NewLifeNetworkLog";
                this.DisplayName = "NewLife.NetworkLog";
                this.Description = "网络日志服务";
            }

            public Worker(IWebHost host) : this()
            {
                IWebHost webHost = host;
                if (webHost == null)
                    throw new ArgumentNullException(nameof(host));
                this._host = webHost;
            }

            protected override void StartWork(string reason)
            {
                this._host.Start();
                this._host.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping.Register((Action)(() =>
                {
                    if (this._stopRequestedByWindows)
                        return;
                    this.StopWork(null);
                }));
            }

            protected override void StopWork(string reason)
            {
                this._stopRequestedByWindows = true;
                try
                {
                    this._host.StopAsync(new CancellationToken()).GetAwaiter().GetResult();
                }
                finally
                {
                    this._host.Dispose();
                }
            }

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {

                    //if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Development")
                    //{
                    //    var config = new ConfigurationBuilder()
                    //    .SetBasePath(Directory.GetCurrentDirectory())
                    //    .AddJsonFile("hosting.json", optional: true)
                    //    .Build();

                    //    webBuilder.UseUrls(config["server.urls"]);
                    //}

                    webBuilder
                        .UseKestrel()
                        .UseStartup<Startup>();
                });

        public static IWebHostBuilder CreateHostBuilder1(string[] args)
        {
            var webBuilder = WebHost.CreateDefaultBuilder(args);
            //if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Development")
            //{
            //    var config = new ConfigurationBuilder()
            //        .SetBasePath(Directory.GetCurrentDirectory())
            //        .AddJsonFile("hosting.json", optional: true)
            //        .Build();

            //    webBuilder.UseUrls(config["server.urls"]);
            //}

            webBuilder.UseStartup<Startup>();

            return webBuilder;
        }
    }
}
