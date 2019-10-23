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

namespace s3iWorker
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            var exeFilePath = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).AbsolutePath;
            Worker.ProcessFileName = $"{Path.GetDirectoryName(exeFilePath)}{Path.DirectorySeparatorChar}s3i.exe";
            Worker.CommandLineArguments = args.Aggregate("", (a, s) => { return $"{a} {s}"; });
            //
            var isService = (!Debugger.IsAttached || args.Contains("--console"));
            var builder = CreateHostBuilder(args);
            var task = isService ? builder.UseWindowsService().Build().RunAsync() : builder.RunConsoleAsync();
            await task.ConfigureAwait(false);
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureLogging(options => options.AddFilter<EventLogLoggerProvider>(level => LogLevel.Warning <= level))
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<Worker>()
                .Configure<EventLogSettings>(config =>
                {
                    //config.LogName = $"Application";
                    config.SourceName = "s3i";
                });
            });
    }
}
