using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

using s3i_lib;

namespace s3i_service
{
    public partial class ServiceS3i : ServiceBase
    {
        Process s3i_process;
        public ServiceS3i()
        {
            InitializeComponent();
            ServiceName = "s3i";
        }

        protected override void OnStart(string[] args)
        {
            var exeFilePath = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).AbsolutePath;
            var s3i_path = $"{Path.GetDirectoryName(exeFilePath)}{Path.DirectorySeparatorChar}s3i.exe";
            s3i_process = Process.Start(s3i_path, args.Aggregate("", (a, s) => { return $"{a} {s}"; }));
        }

        protected override void OnStop()
        {
        }
    }
}
