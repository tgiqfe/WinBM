using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using System.Diagnostics;
using IO.Lib;
using Microsoft.Win32;

namespace IO.Work.Registry
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Unload : TaskJob
    {
        [TaskParameter(Mandatory = true, Resolv = true)]
        [Keys("path", "registrypath", "targetpath", "key", "registrykey", "targetkey", "regkey", "target")]
        protected string _Path { get; set; }

        public override void MainProcess()
        {
            using (var regKey = RegistryControl.GetRegistryKey(_Path, false, false))
            {
                if (regKey == null)
                {
                    Manager.WriteLog(LogLevel.Error, "Target path is Missing. \"{0}\"", _Path);
                    return;
                }
            }

            this.Success = IO.Lib.RegistryHive.UnloadHive(_Path);
        }
    }
}
