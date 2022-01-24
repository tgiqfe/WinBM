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
            //  PathがHKEY_USERSで始まらない場合、修正
            if (_Path.Contains("\\"))
            {
                if (!_Path.StartsWith("HKEY_USERS\\", StringComparison.OrdinalIgnoreCase))
                {
                    _Path = "HKEY_USERS\\" + _Path.Substring(_Path.LastIndexOf("\\") + 1);
                }
            }
            else
            {
                _Path = "HKEY_USERS\\" + _Path;
            }

            using (var regKey = RegistryControl.GetRegistryKey(_Path, false, false))
            {
                if (regKey == null)
                {
                    Manager.WriteLog(LogLevel.Error, "Target path is Missing. \"{0}\"", _Path);
                    return;
                }
            }

            Manager.WriteLog(LogLevel.Info, "Unload Hive. \"{0}\"", _Path);
            this.Success = IO.Lib.RegistryHive.UnloadHive(_Path);
        }
    }
}
