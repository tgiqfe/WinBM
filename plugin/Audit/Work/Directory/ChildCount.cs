using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using Audit.Lib;
using System.IO;
using IO.Lib;

namespace Audit.Work.Directory
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class ChildCount : AuditTaskWork
    {
        [TaskParameter(Mandatory = true, Resolv = true, Delimiter = ';')]
        [Keys("path", "directorypath", "folderpath", "dirpath", "target", "targetpath")]
        protected string[] _Path { get; set; }

        [TaskParameter(MandatoryAny = 1, Unsigned = true)]
        [Keys("directorycount", "directories", "directoryquantity", "foldercount", "folders", "folderquantity")]
        protected int? _DirectoryCount { get; set; }

        [TaskParameter(MandatoryAny = 2, Unsigned = true)]
        [Keys("filecount", "files", "filequantity", "count", "quantity")]
        protected int? _FileCount { get; set; }

        [TaskParameter(MandatoryAny = 3)]
        [Keys("empty", "isempty", "null")]
        protected bool? _IsEmpty { get; set; }

        [TaskParameter]
        [Keys("invert", "not", "no", "none")]
        protected bool _Invert { get; set; }

        public override void MainProcess()
        {
            var dictionary = new Dictionary<string, string>();
            this.Success = true;
            int count = 0;

            foreach (string path in _Path)
            {
                if (Path.GetFileName(path).Contains("*"))
                {
                    Manager.WriteLog(LogLevel.Info, "{0} Wildcard Copy.", this.TaskName);

                    //  対象ファイルの親フォルダーが存在しない場合
                    string parent = Path.GetDirectoryName(path);
                    if (!System.IO.Directory.Exists(parent))
                    {
                        Manager.WriteLog(LogLevel.Warn, "Parent on target is Missing. \"{0}\"", parent);
                        return;
                    }

                    //  ワイルドカード指定
                    System.Text.RegularExpressions.Regex wildcard = Wildcard.GetPattern(path);
                    System.IO.Directory.GetDirectories(parent).
                        Where(x => wildcard.IsMatch(x)).
                        ToList().
                        ForEach(x => ChildCountDirectoryAction(x, dictionary, ++count));
                }
                else
                {
                    //  対象ファイルが存在しない場合
                    if (!System.IO.Directory.Exists(path))
                    {
                        Manager.WriteLog(LogLevel.Warn, "Target is Missing. \"{0}\"", path);
                        return;
                    }

                    ChildCountDirectoryAction(path, dictionary, ++count);
                }
            }

            AddAudit(dictionary, this._Invert);
        }

        private void ChildCountDirectoryAction(string target, Dictionary<string, string> dictionary, int count)
        {
            dictionary[$"directory_{count}"] = target;
            string targetName = "directory";

            int[] ret = Audit.Lib.Monitor.MonitorFunctions.GetDirectoryChildCount(target);

            if (_DirectoryCount != null)
            {
                if (_DirectoryCount == ret[0])
                {
                    dictionary[$"{targetName}_{count}_DirectoryCount_Match"] = ret[0].ToString();
                }
                else
                {
                    Success = false;
                    dictionary[$"{targetName}_{count}_DirectoryCount_NotMatch"] = $"Check:{_DirectoryCount} != Result:{ret[0]}";
                }
            }
            if (_FileCount != null)
            {
                if (_FileCount == ret[1])
                {
                    dictionary[$"{targetName}_{count}_FileCount_Match"] = ret[1].ToString();
                }
                else
                {
                    Success = false;
                    dictionary[$"{targetName}_{count}_FileCount_NotMatch"] = $"Check:{_FileCount} != Result:{ret[1]}";
                }
            }
            if (_IsEmpty != null)
            {
                bool retIsEmpty = ret[0] == 0 && ret[1] == 0;
                if (retIsEmpty && (bool)_IsEmpty)
                {
                    dictionary[$"{targetName}_{count}_Match_IsEmpty"] = retIsEmpty ? "Empty" : "NotEmpty";
                }
                else
                {
                    Success = false;
                    dictionary[$"{targetName}_{count}_NotMatch_IsEmpty"] = retIsEmpty ? "Empty" : "NotEmpty";
                }
            }
        }
    }
}
