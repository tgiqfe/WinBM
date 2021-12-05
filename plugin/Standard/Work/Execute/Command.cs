using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM.Task;
using System.Diagnostics;
using System.IO;
using WinBM;

namespace Standard.Work.Execute
{
    internal class Command : TaskJob
    {
        [TaskParameter(Mandatory = true)]
        [Keys("command", "cmd")]
        protected string _Command { get; set; }

        [TaskParameter]
        [Keys("arguments", "argument", "args", "arg")]
        protected string _Arguments { get; set; }

        [TaskParameter]
        [Keys("work", "workdirectory", "workfolder")]
        protected string _Work { get; set; }

        [TaskParameter]
        [Keys("views", "view")]
        protected bool _IsView { get; set; }

        [TaskParameter]
        [Keys("background", "bk", "back")]
        protected bool _Isbackground { get; set; }

        /// <summary>
        /// コマンド実行
        /// </summary>
        public override void MainProcess()
        {
            using (var proc = new Process())
            {
                if (_Command.Contains(" "))
                {
                    if (_Command.StartsWith("\""))
                    {
                        _Arguments = _Command.Substring(_Command.IndexOf("\"", 1) + 1) + (_Arguments ?? "");
                        _Command = _Command.Substring(0, _Command.IndexOf("\"", 1));
                    }
                    else
                    {
                        _Arguments = _Command.Substring(_Command.IndexOf(" ") + 1) + (_Arguments ?? "");
                        _Command = _Command.Substring(0, _Command.IndexOf(" "));
                    }
                }
                proc.StartInfo.FileName = _Command;
                if (!string.IsNullOrEmpty(_Arguments))
                {
                    if (_Arguments.Contains("^\r\n")) { _Arguments = _Arguments.Replace("^\r\n", ""); }
                    if (_Arguments.Contains("^\n")) { _Arguments = _Arguments.Replace("^\n", ""); }
                    if (_Arguments.Contains("%")) { _Arguments = ExpandEnvironment(_Arguments); }
                    proc.StartInfo.Arguments = _Arguments;
                }
                if (System.IO.Directory.Exists(_Work))
                {
                    proc.StartInfo.WorkingDirectory = _Work;
                }

                //  表示/非表示
                if (_IsView)
                {
                    proc.StartInfo.CreateNoWindow = false;
                    proc.StartInfo.UseShellExecute = true;
                }
                else
                {
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
                }

                proc.Start();

                //  終了待ち
                if (!_Isbackground)
                {
                    proc.BeginOutputReadLine();
                    proc.BeginErrorReadLine();
                    proc.WaitForExit();
                    this.Success = proc.ExitCode == 0;
                }
            }
        }
    }
}
