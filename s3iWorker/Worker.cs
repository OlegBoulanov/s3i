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
            _logger.LogWarning($"ExecuteAsync({ProcessFileName})");
            if (string.IsNullOrWhiteSpace(ProcessFileName))
            {
                _logger.LogWarning($"No ProcessFileName set, exiting");
            }
            else
            {
                _logger.LogInformation($"Start: {ProcessFileName} {CommandLineArguments}");
                var process = StartProcess(ProcessFileName, CommandLineArguments);
                var exited = null != process ? process.WaitForExit(3 * 60 * 1000) : false;
                _logger.LogInformation($"Ran: {(null == process ? $"failed" : exited ? $"{Win32Helper.ErrorMessage(process.ExitCode)}" : $"timed out")}");
            }
            _logger.LogWarning($"ExecuteAsync() exiting");
            await Task.CompletedTask.ConfigureAwait(false);
#pragma warning restore CA1303
        }

        public static string ProcessFileName { get; set; }
        public static string CommandLineArguments { get; set; }
        protected Process StartProcess(string path, string commandLineArgs)
        {
            try
            {
                _logger.LogWarning($"StartProcess({path} {commandLineArgs})");
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
