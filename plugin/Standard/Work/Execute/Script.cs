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

        public override void MainProcess()
        {
            var collection = LanguageCollection.Load(Item.GetScriptLanguageDBFile());

            foreach (string path in _Path)
            {
                using (Process proc = collection.GetProcess(path))
                {
                    proc.StartInfo.CreateNoWindow = true;
                    proc.StartInfo.UseShellExecute = true;
                    proc.Start();
                    proc.WaitForExit();
                }
            }
        }
    }
}
