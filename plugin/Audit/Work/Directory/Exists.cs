using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;

namespace Audit.Work.Directory
{
    internal class Exists : AuditTaskWork
    {
        [TaskParameter(Mandatory = true, Resolv = true, Delimiter = ';')]
        [Keys("path", "directorypath", "folderpath", "dirpath", "target", "targetpath")]
        protected string[] _Path { get; set; }

        [TaskParameter]
        [Keys("invert", "not", "no", "none")]
        protected bool _Invert { get; set; }

        public override void MainProcess()
        {
            this.Success = true;
            int count = 0;

            //  結果情報
            var dictionary = new Dictionary<string, string>();

            foreach (string path in _Path)
            {
                ExistsDirectoryAction(path, dictionary, ++count);
            }

            AddAudit(dictionary, this._Invert);
        }

        private void ExistsDirectoryAction(string target, Dictionary<string, string> dictionary, int count)
        {
            if (System.IO.Directory.Exists(target))
            {
                dictionary[$"directory_{count}_Exists"] = target;
                return;
            }
            dictionary[$"directory_{count}_NotExists"] = target;
            this.Success = false;
        }
    }
}
