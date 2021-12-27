using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;

namespace Audit.Config.Prepare
{
    internal class Setting : TaskConfig
    {
        [TaskParameter(Resolv = true)]
        [Keys("monitrofile", "monitortarget")]
        protected string _MonitorFile { get; set; }

        [TaskParameter(Resolv = true)]
        [Keys("sincedbfile", "sincedb")]
        protected string _SinceDBFile { get; set; }

        [TaskParameter(Resolv = true)]
        [Keys("watchdbdir", "watchdb", "watchdbdirectory")]
        protected string _WatchDBDir{ get;set; }

        public override void MainProcess()
        {
            Manager.PluginParam ??= new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(_MonitorFile))
            {
                Manager.PluginParam[Item.AUDIT_MONITORFILE] = this._MonitorFile;
            }
            if (!string.IsNullOrEmpty(_SinceDBFile))
            {
                Manager.PluginParam[Item.AUDIT_SINCEDBFILE] = this._SinceDBFile;
            }
            if (!string.IsNullOrEmpty(_WatchDBDir))
            {
                Manager.PluginParam[Item.AUDIT_WATCHDBDIR] = this._WatchDBDir;
            }
        }
    }
}
