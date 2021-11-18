using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using System.IO;

namespace Audit.Work.File
{
    internal class Exists : AuditTaskWork
    {
        [TaskParameter(Mandatory = true, ResolvEnv = true, Delimiter = ';')]
        [Keys("path", "filepath", "target", "targetpath")]
        protected string[] _Path { get; set; }

        [TaskParameter]
        [Keys("containsdirectory", "containsdir")]
        protected bool _ContainsDirectory { get; set; }

        [TaskParameter]
        [Keys("invert", "not", "no", "none")]
        protected bool _Invert { get; set; }

        public override void MainProcess()
        {
            this.Success = true;
            int count = 0;

            //  結果情報
            var dictionary = new Dictionary<string, string>();

            foreach(string path in _Path)
            {
                ExistsFileCheck(path, dictionary, ++count);
            }

            AddAudit(dictionary, this._Invert);
        }

        private void ExistsFileCheck(string target, Dictionary<string, string> dictionary, int count)
        {
            if (System.IO.File.Exists(target))
            {
                //  ファイルが存在する
                dictionary[$"file_{count}_Exists"] = target;
                return;
            }
            if ((_ContainsDirectory || target.EndsWith('\\')) && System.IO.Directory.Exists(target))
            {
                //  ファイルは存在せず、
                //  フォルダー指定してフォルダーが存在する場合
                dictionary[$"directory_{count}_Exists"] = target;
                return;
            }
            
            dictionary[$"file_{count}_NotExists"] = target;
            this.Success = false;
        }
    }
}
