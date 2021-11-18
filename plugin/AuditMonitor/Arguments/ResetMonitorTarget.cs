using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuditMonitor.Arguments
{
    class ResetMonitorTarget
    {
        public bool Enabled { get; set; }

        public ResetMonitorTarget() { }
        public ResetMonitorTarget(bool enabled)
        {
            this.Enabled = enabled;
        }
    }
}
