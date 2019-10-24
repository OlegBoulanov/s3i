using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

using s3iLib;

namespace s3iWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
#pragma warning disable CA1303 // literal string, use resource...
            if (string.IsNullOrWhiteSpace(ProcessFileName))
            {
                _logger.LogWarning($"No ProcessFileName set, exiting");
            }
            else
            {
                _logger.LogInformation($"Start: {ProcessFileName}");
                var process = StartProcess(ProcessFileName, "");
                var exited = null != process ? process.WaitForExit((int)ProcessTimeout.TotalMilliseconds) : false;
                _logger.LogInformation($"{ProcessFileName}: {(null == process ? $"failed" : exited ? $"{Win32Helper.ErrorMessage(process.ExitCode)}" : $"timed out")}");
            }
            await Task.CompletedTask.ConfigureAwait(false);
#pragma warning restore CA1303
        }

        public static string ProcessFileName { get; set; }
        public static TimeSpan ProcessTimeout { get; set; } = TimeSpan.FromMinutes(5);
        protected Process StartProcess(string path, string commandLineArgs)
        {
            try
            {
                return Process.Start(path, commandLineArgs);
            }
#pragma warning disable CA1031    // catch specific
            catch (Exception x)
#pragma warning restore CA1031
            {
                _logger.LogError($"Worker.StartProcess({path}, {commandLineArgs}) error:{Environment.NewLine}{x.Message}{Environment.NewLine}{x.StackTrace}");
            }
            return null;
        }
    }
}
