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
        [TaskParameter(Mandatory = true, Resolv = true, Delimiter = ';')]
        [Keys("path", "registrypath", "targetpath", "key", "registrykey", "targetkey", "regkey", "target")]
        protected string[] _Path { get; set; }

        [TaskParameter]
        [Keys("random", "rdm", "randam")]
        protected bool? _Random { get; set; }

        public override void MainProcess()
        {
            this.Success = true;

            if (_Random ?? false)
            {
                //  Random指定の場合、Pathをフォルダーとして解釈し、その配下にランダムファイルを作成。
                _Path = _Path.Select(x => Path.Combine(x, Path.GetRandomFileName())).ToArray();
            }
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

                Manager.WriteLog(LogLevel.Debug, "Create registry key: \"{0}\"", target);
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
