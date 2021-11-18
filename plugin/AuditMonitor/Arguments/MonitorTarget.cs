using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace AuditMonitor.Arguments
{
    class MonitorTarget
    {
        public string File { get; set; }

        public MonitorTarget()
        {
            this.File = Path.Combine(
                Environment.GetEnvironmentVariable("ProgramData"), "WinBM", "Audit", "AuditMonitor.json");
        }
        public MonitorTarget(string targetFile)
        {
            this.File = targetFile;
        }

        public override string ToString()
        {
            return File;
        }
    }
}
