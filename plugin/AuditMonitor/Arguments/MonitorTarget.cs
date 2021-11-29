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
            string workDir = Environment.UserName == "SYSTEM" ?
                Path.Combine(Environment.GetEnvironmentVariable("ProgramData"), "WinBM") :
                Path.Combine(Path.GetTempPath(), "WinBM");
            this.File = Path.Combine(workDir, "Audit", "AuditMonitor.json");
        }
        public MonitorTarget(string targetFile)
        {
            this.File = targetFile;
        }

        public override string ToString()
        
            return File;
        }
    }
}
