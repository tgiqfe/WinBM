using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using Standard.Lib.ScriptLanguage;
using System.Diagnostics;

namespace Standard.Work.Execute
{
    internal class Script : TaskJob
    {
        [TaskParameter(Mandatory = true, Resolv = true, Delimiter = ';')]
        [Keys("path", "target", "targetpath", "script", "scriptpath")]
        protected string[] _Path { get; set; }

        [TaskParameter]
        [Keys("progress", "prog", "progressbar")]
        protected bool _ProgressBar { get; set; }

        [TaskParameter(Resolv = true)]
        [Keys("activity", "title")]
        protected string _Activity { get; set; }

        [TaskParameter(Unsigned = true)]
        [Keys("interval", "intervalmilliseconds", "intervalmillisecond", "intervalmsec")]
        protected int? _Interval { get; set; }

        [TaskParameter(Unsigned = true)]
        [Keys("intervalseconds", "intervalsecond", "intervalsec")]
        protected int? _IntervalSecond { get; set; }

        public override void MainProcess()
        {
            var collection = LanguageCollection.Load(Item.GetScriptLanguageDBFile());

            List<string> scriptList = new List<string>();
            _Path.ToList().ForEach(x =>
            {
                if (System.IO.File.Exists(x))
                {
                    scriptList.Add(x);
                }
                else if (System.IO.Directory.Exists(x))
                {
                    scriptList.AddRange(Directory.GetFiles(x));
                }
            });
            scriptList.Sort();

            //  スクリプト実行間の待ち時間
            int interval = ((_Interval ?? 0) > 0 ? (int)_Interval : 0) +
                ((_IntervalSecond ?? 0) > 0 ? (int)_IntervalSecond * 1000 : 0);
            
            int cursor = 0;
            foreach (var path in scriptList)
            {
                if (_ProgressBar)
                {
                    Manager.WriteProgressBar(
                        2,
                        _Activity ?? this.SpecName,
                        scriptList.Count,
                        cursor++,
                        System.IO.Path.GetFileName(path));
                }
                using (Process proc = collection.GetProcess(path))
                {
                    proc.StartInfo.CreateNoWindow = true;
                    proc.StartInfo.UseShellExecute = false;
                    proc.Start();
                    proc.WaitForExit();
                }
                if (interval > 0)
                {
                    System.Threading.Thread.Sleep(interval);
                }
            }

            /*
            foreach (string path in _Path)
            {
                if (System.IO.File.Exists(path))
                {
                    using (Process proc = collection.GetProcess(path))
                    {
                        proc.StartInfo.CreateNoWindow = true;
                        proc.StartInfo.UseShellExecute = false;
                        proc.Start();
                        proc.WaitForExit();
                    }
                }
                if (System.IO.Directory.Exists(path))
                {
                    foreach (string child in System.IO.Directory.GetFiles(path))
                    {
                        using (Process proc = collection.GetProcess(child))
                        {
                            proc.StartInfo.CreateNoWindow = true;
                            proc.StartInfo.UseShellExecute = false;
                            proc.Start();
                            proc.WaitForExit();
                        }
                    }
                }
            }
            */
        }
    }
}
