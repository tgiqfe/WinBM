﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using Microsoft.Win32;
using IO.Lib;
using System.Diagnostics;
using System.IO;

namespace IO.Work.Registry
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Delete : WorkRegistry
    {
        [TaskParameter(Mandatory = true, Resolv = true, Delimiter = ';')]
        [Keys("path", "registrypath", "targetpath", "key", "registrykey", "targetkey", "regkey", "target")]
        protected string[] _Path { get; set; }

        [TaskParameter(Resolv = true, Delimiter = '\n')]
        [Keys("name", "name", "namae", "registryname", "regname", "paramname")]
        protected string[] _Name { get; set; }

        [TaskParameter]
        [Keys("clear", "crear", "claer")]
        protected bool _Clear { get; set; }

        [TaskParameter(Resolv = true)]
        [Keys("exclude", "excludepath", "excludes", "excludepaths", "expath", "expaths")]
        protected string[] _Exclude { get; set; }

        public override void MainProcess()
        {
            this.Success = true;

            if (_Name?.Length > 0)
            {
                //  レジストリ値の削除。キーを複数指定していた場合、2つ目以降を無視
                TargetValueSequence(_Path[0], _Name, writable: true, DeleteRegistryValueAction);
            }
            else
            {
                //  レジストリキーの削除
                TargetKeySequence(_Path, writable: true, DeleteRegistryKeyAction);
            }
        }

        private void DeleteRegistryKeyAction(RegistryKey target)
        {
            //  Excludeにキーパスorキー名が一致していたらスキップ
            if (_Exclude?.Length > 0)
            {
                if (_Exclude.Any(x =>
                     (x.Contains("\\") && x.Equals(target.Name, StringComparison.OrdinalIgnoreCase)) ||
                     (!x.Contains("\\") && x.Equals(Path.GetFileName(target.Name), StringComparison.OrdinalIgnoreCase))))
                {
                    return;
                }
            }

            Action<RegistryKey> deleteRegistryKey = (targetKey) =>
            {
                try
                {
                    targetKey.DeleteSubKeyTree("");
                }
                catch (Exception e)
                {
                    if (e is System.Security.SecurityException || e is UnauthorizedAccessException)
                    {
                        Manager.WriteLog(LogLevel.Info, "Error has occured, so execute the reg command.");
                        Manager.WriteLog(LogLevel.Debug, e.ToString());

                        using (var proc = new Process())
                        {
                            proc.StartInfo.FileName = "reg";
                            proc.StartInfo.Arguments = $"delete \"{targetKey.Name}\" /f";
                            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                            proc.Start();
                            proc.WaitForExit();
                        }
                    }
                    else
                    {
                        Manager.WriteLog(LogLevel.Error, "{0} {1}", this.TaskName, e.Message);
                        Manager.WriteLog(LogLevel.Debug, e.ToString());
                        this.Success = false;
                    }
                }
            };

            //  レジストリキー削除
            if (_Clear)
            {
                foreach (string subName in target.GetValueNames())
                {
                    target.DeleteValue(subName);
                }
                foreach (string keyName in target.GetSubKeyNames())
                {
                    using (var subRegKey = target.OpenSubKey(keyName, true))
                    {
                        deleteRegistryKey(subRegKey);
                    }
                }
            }
            else
            {
                deleteRegistryKey(target);
            }
        }

        private void DeleteRegistryValueAction(RegistryKey target, string name)
        {
            //  Excludeにレジストリ値の名前が一致していたらスキップ
            if (_Exclude?.Length > 0)
            {
                if(_Exclude.Any(x => x.Equals(name, StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }
            }

            try
            {
                target.DeleteValue(name);
            }
            catch (Exception e)
            {
                Manager.WriteLog(LogLevel.Error, "{0} {1}", this.TaskName, e.Message);
                Manager.WriteLog(LogLevel.Debug, e.ToString());
                this.Success = false;
            }
        }
    }

    internal class Remove : Delete
    {
        protected override bool IsAlias { get { return true; } }
    }
}
