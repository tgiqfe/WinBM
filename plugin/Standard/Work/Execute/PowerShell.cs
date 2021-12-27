using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using System.IO;
using System.Diagnostics;

namespace Standard.Work.Execute
{
    internal class PowerShell : TaskJob
    {
        [TaskParameter]
        [Keys("pwshpath", "pwsh", "powershell", "powershellpath", "path", "filepath")]
        protected string _PwshPath { get; set; }

        [TaskParameter(Mandatory = true)]
        [Keys("command", "cmd", "commandlet", "cmdlet")]
        protected string _Commandlet { get; set; }

        [TaskParameter]
        [Keys("arguments", "argument", "args", "arg")]
        protected string _Arguments { get; set; }

        [TaskParameter(Resolv = true)]
        [Keys("work", "workdirectory", "workfolder")]
        protected string _Work { get; set; }

        /// <summary>
        /// PowerShellコマンドの実行
        /// </summary>
        public override void MainProcess()
        {
            //  PowerShell実行用exeの場所をセット
            if (string.IsNullOrEmpty(_PwshPath))
            {
                foreach (string psCommand in new string[] { "pwsh", "pwershell" })
                {
                    _PwshPath = SearchPath(psCommand);
                    if (!string.IsNullOrEmpty(_PwshPath))
                    {
                        Manager.WriteLog(LogLevel.Debug, "Find pwsh path \"{0}\"", _PwshPath);
                        break;
                    }
                }
            }
            else
            {
                if (_PwshPath.Contains("\\"))
                {
                    if (!File.Exists(_PwshPath))
                    {
                        _PwshPath = null;
                    }
                }
                else
                {
                    _PwshPath = SearchPath(_PwshPath);
                    Manager.WriteLog(LogLevel.Debug, "Find pwsh path \"{0}\"", _PwshPath);
                }
            }
            if (string.IsNullOrEmpty(_PwshPath))
            {
                Manager.WriteLog(LogLevel.Error, "PowerShell execute path is Missing.");
                return;
            }

            using (var proc = new Process())
            {
                proc.StartInfo.FileName = _PwshPath;
                proc.StartInfo.Arguments =
                    string.Format("-ExecutionPolicy Unrestricted{0} -Command \"{1}{2}\"",
                        string.IsNullOrEmpty(_Work) ? "" : " -WorkingDirectory \"" + _Work + "\"",
                        _Commandlet,
                        string.IsNullOrEmpty(_Arguments) ? "" : " " + _Arguments);

                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.OutputDataReceived += (sender, e) =>
                {
                    Manager.WriteStandard(e.Data);
                };
                proc.ErrorDataReceived += (sender, e) =>
                {
                    Manager.WriteStandardError(e.Data);
                };

                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                proc.WaitForExit();

                this.Success = proc.ExitCode == 0;
            }
        }

        /// <summary>
        /// whereコマンドでパス調査
        /// </summary>
        /// <param name="targetFile"></param>
        /// <returns></returns>
        private string SearchPath(string targetFile)
        {
            using (var proc = new Process())
            {
                proc.StartInfo.FileName = "where.exe";
                proc.StartInfo.Arguments = targetFile;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.Start();
                string output_psCommandPath = proc.StandardOutput.ReadLine();     //最初の1行のみ取得
                proc.WaitForExit();

                return proc.ExitCode == 0 ? output_psCommandPath.Trim() : null;
            }
        }
    }
}
