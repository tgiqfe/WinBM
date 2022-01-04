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
using Microsoft.Win32;

namespace Audit.Work.Registry
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class ChildCount : AuditTaskWork
    {
        [TaskParameter(Mandatory = true, Resolv = true, Delimiter = ';')]
        [Keys("path", "registrypath", "targetpath", "key", "registrykey", "targetkey", "regkey", "target")]
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
                string keyName = System.IO.Path.GetFileName(path);
                if (keyName.Contains("*"))
                {
                    string parent = System.IO.Path.GetDirectoryName(path);
                    using (RegistryKey parentKey = RegistryControl.GetRegistryKey(parent, false, false))
                    {
                        //  対象キーの親キーが存在しない場合
                        if (parentKey == null)
                        {
                            Manager.WriteLog(LogLevel.Warn, "Parent on target is Missing. \"{0}\"", parent);
                            return;
                        }

                        //  ワイルドカード指定
                        System.Text.RegularExpressions.Regex wildcard = Wildcard.GetPattern(keyName);
                        foreach (var childKeyName in
                            parentKey.GetSubKeyNames().Where(x => wildcard.IsMatch(x)))
                        {
                            using (RegistryKey childKey = parentKey.OpenSubKey(childKeyName, false))
                            {
                                ChildCountRegistryKeyAction(childKey, dictionary, ++count);
                            }
                        }
                    }
                }
                else
                {
                    using (RegistryKey regKey = RegistryControl.GetRegistryKey(path, false, false))
                    {
                        //  対象のキーが存在しない場合
                        if (regKey == null)
                        {
                            Manager.WriteLog(LogLevel.Warn, "Target is Missing. \"{0}\"", path);
                            return;
                        }

                        ChildCountRegistryKeyAction(regKey, dictionary, ++count);
                    }
                }
            }

            AddAudit(dictionary, this._Invert);
        }

        private void ChildCountRegistryKeyAction(RegistryKey target, Dictionary<string, string> dictionary, int count)
        {
            dictionary[$"directory_{count}"] = target.Name;
            string targetName = "registryKey";

            int[] ret = Audit.Lib.Monitor.MonitorFunctions.GetRegistryKeyChildCount(target);

            if (_DirectoryCount != null)
            {
                if (_DirectoryCount == ret[0])
                {
                    dictionary[$"{targetName}_{count}_DirectoryCount_Match"] = ret[0].ToString();
                }
                else
                {
                    Success = false;
                    dictionary[$"directory_{count}_DirectoryCount_NotMatch"] = $"Check:{_DirectoryCount} != Result:{ret[0]}";
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
