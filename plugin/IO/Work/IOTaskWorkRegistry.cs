using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using IO.Lib;
using WinBM;
using WinBM.Task;

namespace IO.Work
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class IOTaskWorkRegistry : TaskJob
    {
        protected delegate void TargetRegistryKeyAction(RegistryKey key);

        protected delegate void TargetRegistryValueAction(RegistryKey key, string name);

        /// <summary>
        /// 対象レジストリキーに対するシーケンシャル処理
        /// </summary>
        /// <param name="paths"></param>
        /// <param name="targetRegistryKeyAction"></param>
        protected void TargetRegistryKeyProcess(string[] paths, bool writable, TargetRegistryKeyAction targetRegistryKeyAction)
        {
            foreach (string path in paths)
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
                            using (RegistryKey childKey = parentKey.OpenSubKey(childKeyName, writable))
                            {
                                targetRegistryKeyAction(childKey);
                            }
                        }
                    }
                }
                else
                {
                    using (RegistryKey regKey = RegistryControl.GetRegistryKey(path, false, writable))
                    {
                        //  対象のキーが存在しない場合
                        if (regKey == null)
                        {
                            Manager.WriteLog(LogLevel.Warn, "Target is Missing. \"{0}\"", path);
                            return;
                        }

                        targetRegistryKeyAction(regKey);
                    }
                }
            }
        }

        protected void TargetRegistryValueProcess(string path, string[] names, bool writable, TargetRegistryValueAction targetRegistryValueAction)
        {
            using (RegistryKey regKey = RegistryControl.GetRegistryKey(path, false, writable))
            {
                //  対象キーが存在しない場合
                if (regKey == null)
                {
                    Manager.WriteLog(LogLevel.Warn, "Parent on target is Missing. \"{0}\"", regKey.Name);
                    return;
                }

                foreach (string name in names)
                {
                    if (name.Contains("*"))
                    {
                        //  ワイルドカード指定
                        System.Text.RegularExpressions.Regex wildcard = Wildcard.GetPattern(name);
                        foreach (var valueName in regKey.GetValueNames().Where(x => wildcard.IsMatch(x)))
                        {
                            targetRegistryValueAction(regKey, valueName);
                        }
                    }
                    else
                    {
                        targetRegistryValueAction(regKey, name);
                    }
                }
            }
        }


    }
}
