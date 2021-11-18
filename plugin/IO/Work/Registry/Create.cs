using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using Microsoft.Win32;
using IO.Lib;

namespace IO.Work.Registry
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Create : TaskJob
    {
        [TaskParameter(Mandatory = true, ResolvEnv = true, Delimiter = ';')]
        [Keys("path", "registrypath", "targetpath", "key", "registrykey", "targetkey", "regkey", "target")]
        protected string[] _Path { get; set; }

        public override void MainProcess()
        {
            this.Success = true;

            _Path.ToList().ForEach(x => CreateRegistryAction(x));
        }

        private void CreateRegistryAction(string target)
        {
            try
            {
                //  ワイルドカード指定は対応しない方針
                if (target.Contains("*"))
                {
                    Manager.WriteLog(LogLevel.Warn, "The path contains a wildcard.");
                    return;
                }

                using (RegistryKey regKey = RegistryControl.GetRegistryKey(target, true, true)) { }
            }
            catch (Exception e)
            {
                Manager.WriteLog(LogLevel.Error, "{0} {1}", this.TaskName, e.Message);
                Manager.WriteLog(LogLevel.Debug, e.ToString());
                this.Success = false;
            }
        }
    }

    internal class New : Create
    {
        protected override bool IsAlias { get { return true; } }
    }
}
