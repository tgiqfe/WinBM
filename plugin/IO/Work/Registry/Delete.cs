using System;
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
    internal class Delete : IOTaskWorkRegistry
    {
        [TaskParameter(Mandatory = true, ResolvEnv = true, Delimiter = ';')]
        [Keys("path", "registrypath", "targetpath", "key", "registrykey", "targetkey", "regkey", "target")]
        protected string[] _Path { get; set; }

        [TaskParameter(ResolvEnv = true, Delimiter = '\n')]
        [Keys("name", "name", "namae", "registryname", "regname", "paramname")]
        protected string[] _Name { get; set; }

        [TaskParameter]
        [Keys("clear", "crear", "claer")]
        protected bool _Clear { get; set; }

        public override void MainProcess()
        {
            this.Success = true;

            if (_Name?.Length > 0)
            {
                //  レジストリ値の削除。キーを複数指定していた場合、2つ目以降を無視
                TargetRegistryValueProcess(_Path[0], _Name, writable: true, DeleteRegistryValueAction);
            }
            else
            {
                //  レジストリキーの削除
                TargetRegistryKeyProcess(_Path, writable: true, DeleteRegistryKeyAction);
            }

            /*
            using (var regKey = RegistryControl.GetRegistryKey(_Path[0], false, true))
            {
                //  対象のキーが存在しない場合
                if (regKey == null)
                {
                    Manager.WriteLog(LogLevel.Error, "Target path is missing. \"{0}\"", _Path);
                    return;
                }

                if (_Name == null)
                {
                    Action<RegistryKey> deleteRegistryKey = (targetKey) =>
                    {
                        try
                        {
                            targetKey.DeleteSubKeyTree("");
                        }
                        catch (Exception e)
                        {
                            if (e is System.Security.SecurityException ||
                                e is UnauthorizedAccessException)
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
                        foreach (string name in regKey.GetValueNames())
                        {
                            regKey.DeleteValue(name);
                        }
                        foreach (string keyName in regKey.GetSubKeyNames())
                        {
                            using (var subRegKey = regKey.OpenSubKey(keyName, true))
                            {
                                deleteRegistryKey(subRegKey);
                            }
                        }
                    }
                    else
                    {
                        deleteRegistryKey(regKey);
                    }
                }
                else
                {
                    //  値削除
                    foreach (string name in _Name)
                    {
                        if (name.Contains("*"))
                        {
                            //  ワイルドカード指定
                            System.Text.RegularExpressions.Regex wildcard = Wildcard.GetPattern(name);
                            regKey.GetValueNames().
                                Where(x => wildcard.IsMatch(x)).
                                ToList().
                                ForEach(x => regKey.DeleteValue(x));
                        }
                        else
                        {
                            regKey.DeleteValue(name);
                        }
                    }
                }
            }
            */
        }

        private void DeleteRegistryKeyAction(RegistryKey target)
        {
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
