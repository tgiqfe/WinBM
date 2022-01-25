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
    internal class WorkRegistry : IOTaskWork
    {
        protected delegate void TargetRegistryKeyAction(RegistryKey key);

        protected delegate void TargetRegistryValueAction(RegistryKey key, string name);

        protected delegate void SrcDstRegistryKeyAction(RegistryKey sourceKey, RegistryKey destinationKey);

        protected delegate void SrcDstRegistryValueAction(RegistryKey sourceKey, RegistryKey destinationKey, string sourceName, string destinationName);

        #region Sequential RegistryKey

        /// <summary>
        /// 対象レジストリキーに対するシーケンシャル処理
        /// </summary>
        /// <param name="paths"></param>
        /// <param name="targetRegistryKeyAction"></param>
        protected void TargetKeySequence(string[] paths, bool writable, TargetRegistryKeyAction targetRegistryKeyAction)
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
                            Success = false;
                            continue;
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
                            Success = false;
                            continue;
                        }

                        targetRegistryKeyAction(regKey);
                    }
                }
            }
        }

        /// <summary>
        /// 対象レジストリキーに対するシーケンシャル処理。src/dst指定
        /// </summary>
        /// <param name="sourcePaths"></param>
        /// <param name="destinationPath"></param>
        /// <param name="writable"></param>
        /// <param name="srcDstRegistryKeyAction"></param>
        protected void SrcDstKeySequence(string[] sourcePaths, string destinationPath, bool writable, SrcDstRegistryKeyAction srcDstRegistryKeyAction)
        {
            foreach (string source in sourcePaths)
            {
                string sourceKeyName = Path.GetFileName(source);
                if (sourceKeyName.Contains("*"))
                {
                    Manager.WriteLog(LogLevel.Info, "{0} Wildcard path.", this.TaskName);

                    //  Sourceの親キーが存在しない場合
                    string parent = Path.GetDirectoryName(source);
                    using (RegistryKey parentKey = RegistryControl.GetRegistryKey(parent, false, false))
                    {
                        if (parentKey == null)
                        {
                            Manager.WriteLog(LogLevel.Warn, "Parent on target is Missing. \"{0}\"", parent);
                            Success = false;
                            continue;
                        }

                        System.Text.RegularExpressions.Regex wildcard = Wildcard.GetPattern(sourceKeyName);
                        var matchItems = parentKey.GetSubKeyNames().Where(x => wildcard.IsMatch(sourceKeyName)).ToList();
                        if (matchItems.Count > 0)
                        {
                            using (RegistryKey destinationKey = RegistryControl.GetRegistryKey(destinationPath, true, true))
                            {
                                matchItems.ForEach(x =>
                                {
                                    using (RegistryKey sourceKey = parentKey.OpenSubKey(x, writable))
                                    {
                                        srcDstRegistryKeyAction(sourceKey, destinationKey);
                                    }
                                });
                            }
                        }
                    }
                }
                else
                {
                    using (RegistryKey sourceKey = RegistryControl.GetRegistryKey(source, false, writable))
                    {
                        //  Sourceが存在しない場合
                        if (sourceKey == null)
                        {
                            Manager.WriteLog(LogLevel.Error, "Source target is Missing. \"{0}\"", source);
                            Success = false;
                            continue;
                        }

                        using (RegistryKey destinationKey = RegistryControl.GetRegistryKey(destinationPath, true, true))
                        {
                            srcDstRegistryKeyAction(sourceKey, destinationKey);
                        }
                    }
                }
            }
        }

        #endregion
        #region Sequential RegistryValue

        /// <summary>
        /// 対象のレジストリ値に対するシーケンシャル処理
        /// </summary>
        /// <param name="path"></param>
        /// <param name="names"></param>
        /// <param name="writable"></param>
        /// <param name="targetRegistryValueAction"></param>
        protected void TargetValueSequence(string path, string[] names, bool writable, TargetRegistryValueAction targetRegistryValueAction)
        {
            using (RegistryKey parentKey = RegistryControl.GetRegistryKey(path, false, writable))
            {
                //  対象キーが存在しない場合
                if (parentKey == null)
                {
                    Manager.WriteLog(LogLevel.Warn, "Parent on target is Missing. \"{0}\"", parentKey.Name);
                    Success = false;
                    return;
                }

                foreach (string name in names)
                {
                    if (name.Contains("*"))
                    {
                        //  ワイルドカード指定
                        System.Text.RegularExpressions.Regex wildcard = Wildcard.GetPattern(name);
                        foreach (var valueName in parentKey.GetValueNames().Where(x => wildcard.IsMatch(x)))
                        {
                            targetRegistryValueAction(parentKey, valueName);
                        }
                    }
                    else
                    {
                        targetRegistryValueAction(parentKey, name);
                    }
                }
            }
        }

        /// <summary>
        /// 対象のレジストリ値に対するシーケンシャル処理。src/dst指定
        /// </summary>
        /// <param name="sourcePaths"></param>
        /// <param name="destinationPath"></param>
        /// <param name="sourceNames"></param>
        /// <param name="destinationName"></param>
        /// <param name="srcDstRegistryValueAction"></param>
        protected void SrcDstValueSequence(string sourcePath, string destinationPath, string[] sourceNames, string destinationName, bool writable, SrcDstRegistryValueAction srcDstRegistryValueAction)
        {
            using (RegistryKey parentKey = RegistryControl.GetRegistryKey(sourcePath, false, writable))
            {
                //  Sourceの所属キーが存在しない場合
                if (parentKey == null)
                {
                    Manager.WriteLog(LogLevel.Warn, "Parent on target is Missing. \"{0}\"", sourcePath);
                    Success = false;
                    return;
                }

                destinationPath ??= sourcePath;
                foreach (string name in sourceNames)
                {
                    destinationName ??= name;
                    if (name.Contains("*"))
                    {
                        Manager.WriteLog(LogLevel.Info, "{0} Wildcard path.", this.TaskName);

                        System.Text.RegularExpressions.Regex wildcard = Wildcard.GetPattern(name);
                        var matchItems = parentKey.GetValueNames().Where(x => wildcard.IsMatch(x)).ToList();
                        if (matchItems.Count > 0)
                        {
                            using (RegistryKey destinationKey = RegistryControl.GetRegistryKey(destinationPath, true, true))
                            {
                                matchItems.ForEach(x =>
                                {
                                    using (RegistryKey sourceKey = parentKey.OpenSubKey(x, writable))
                                    {
                                        srcDstRegistryValueAction(sourceKey, destinationKey, name, destinationName);
                                    }
                                });
                            }
                        }
                    }
                    else
                    {
                        using (RegistryKey destinationKey = RegistryControl.GetRegistryKey(destinationPath, true, true))
                        {
                            srcDstRegistryValueAction(parentKey, destinationKey, name, destinationName);
                        }
                    }
                }
            }
        }

        #endregion
    }
}
