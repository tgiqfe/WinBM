using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM.Task;
using IO.Lib;

namespace IO.Require.Registry
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Exists : TaskJob
    {
        [TaskParameter(Mandatory = true, Resolv = true, Delimiter = ';')]
        [Keys("path", "registrypath", "targetpath", "key", "registrykey", "targetkey", "regkey", "target")]
        protected string[] _Path { get; set; }

        [TaskParameter(Resolv = true, Delimiter = '\n')]
        [Keys("name", "name", "namae", "registryname", "regname", "paramname")]
        protected string[] _Name { get; set; }

        [TaskParameter]
        [Keys("invert", "not", "no", "none")]
        protected bool _Invert { get; set; }

        public override void MainProcess()
        {
            this.Success = true;

            if (_Name?.Length > 0)
            {
                foreach (var name in _Name)
                {
                    bool ret = RegistryControl.Exists(_Path[0], name);
                    if (!ret)
                    {
                        Manager.WriteLog(WinBM.LogLevel.Attention, "Target is missing. \"{0}\" {1}", _Path[0], name);
                    }
                    Success &= ret;
                }
            }
            else
            {
                foreach (var path in _Path)
                {
                    bool ret = RegistryControl.Exists(path);
                    if (!ret)
                    {
                        Manager.WriteLog(WinBM.LogLevel.Attention, "Target is missing. \"{0}\"", path);
                    }
                    Success &= ret;
                }
            }

            this.Success ^= this._Invert;
        }
    }
}
