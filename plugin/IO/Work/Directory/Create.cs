using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;

namespace IO.Work.Directory
{
    internal class Create : TaskJob
    {
        [TaskParameter(Mandatory = true, ResolvEnv = true, Delimiter = ';')]
        [Keys("path", "filepath", "target", "targetpath", "dirpath", "directorypath")]
        protected string[] _Path { get; set; }

        public override void MainProcess()
        {
            this.Success = true;

            _Path.ToList().ForEach(x => CreateDirectoryAction(x));
        }

        private void CreateDirectoryAction(string target)
        {
            try
            {
                //  ワイルドカード指定は対応しない方針
                if (target.Contains("*"))
                {
                    Manager.WriteLog(LogLevel.Warn, "The path contains a wildcard.");
                    return;
                }

                //  新規作成の対象がすでに存在する場合
                if (System.IO.Directory.Exists(target))
                {
                    Manager.WriteLog(LogLevel.Info, "New target is already exists. \"{0}\"", target);
                    return;
                }

                System.IO.Directory.CreateDirectory(target);
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
