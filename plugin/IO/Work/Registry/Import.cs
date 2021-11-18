using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using System.Diagnostics;
using Microsoft.Win32;

namespace IO.Work.Registry
{
    internal class Import : TaskJob
    {
        [TaskParameter(Mandatory = true, ResolvEnv = true)]
        [Keys("filepath", "file", "outputpath", "outputfile", "output")]
        protected string _FilePath { get; set; }

        public override void MainProcess()
        {
            this.Success = true;

            //  インポートファイルが存在しない場合
            if (!System.IO.File.Exists(_FilePath))
            {
                Manager.WriteLog(LogLevel.Error, "Target file is Missing. \"{0}\"", _FilePath);
                return;
            }

            using (var proc = new Process())
            {
                proc.StartInfo.FileName = "reg.exe";
                proc.StartInfo.Arguments = $"import \"{_FilePath}\"";
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.UseShellExecute = false;
                proc.Start();
                proc.WaitForExit();
            }
        }
    }
}
