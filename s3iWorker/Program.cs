using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using System.Web;

namespace s3iWorker
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            commandLine.Parse(args);
            Worker.ProcessFileName = commandLine.ProcessFilePath;
            Worker.ProcessTimeout = commandLine.ProcessTimeout;
            var isService = (!Debugger.IsAttached || commandLine.RunConsole);
            var builder = CreateHostBuilder(args);
            var task = isService ? builder.UseWindowsService().Build().RunAsync() : builder.RunConsoleAsync();
            await task.ConfigureAwait(false);
        }
        static readonly CommandLine commandLine = new CommandLine();
        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureLogging(options => options.AddFilter<EventLogLoggerProvider>(level => commandLine.LogLevel <= level))
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<Worker>()
                .Configure<EventLogSettings>(config =>
                {
                    config.LogName = $"Application";    // null would do just the same
                    config.SourceName = "s3i";
                });
            });
    }
}
