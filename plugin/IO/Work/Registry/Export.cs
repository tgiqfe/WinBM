using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using System.Diagnostics;
using Microsoft.Win32;
using IO.Lib;
using System.IO;

namespace IO.Work.Registry
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Export : TaskJob
    {
        [TaskParameter(Mandatory = true, Resolv = true)]
        [Keys("path", "registrypath", "targetpath", "key", "registrykey", "targetkey", "regkey", "target")]
        protected string _Path { get; set; }

        [TaskParameter(Mandatory = true, Resolv = true)]
        [Keys("filepath", "file", "outputpath", "outputfile", "output")]
        protected string _FilePath { get; set; }

        [TaskParameter]
        [Keys("force", "forse")]
        protected bool _Force { get; set; }

        public override void MainProcess()
        {
            this.Success = true;

            using (var regKey = RegistryControl.GetRegistryKey(_Path, false, false))
            {
                //  対象のキーが存在しない場合
                if (regKey == null)
                {
                    Manager.WriteLog(LogLevel.Error, "Target path is missing. \"{0}\"", _Path);
                    return;
                }
            }

            //  出力先の親フォルダーが無い場合、作成
            string parent = Path.GetDirectoryName(_FilePath);
            if (!System.IO.Directory.Exists(parent))
            {
                Manager.WriteLog(LogLevel.Info, "Create parent folder. \"{0}\"", parent);
                System.IO.Directory.CreateDirectory(parent);
            }

            //  出力先が存在し、force=falseの場合
            if (System.IO.File.Exists(_FilePath) && !_Force)
            {
                Manager.WriteLog(LogLevel.Warn, "Output file is already exists. \"{0}\"", _FilePath);
                return;
            }

            using (var proc = new Process())
            {
                proc.StartInfo.FileName = "reg.exe";
                proc.StartInfo.Arguments = $"export \"{_Path}\" \"{_FilePath}\" /y";
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.UseShellExecute = false;
                proc.Start();
                proc.WaitForExit();
            }
        }
    }
}
