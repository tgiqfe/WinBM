using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using WinBM;
using WinBM.Task;
using IO.Lib;

namespace Audit.Work
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class WorkRegistry : AuditTaskWork
    {
        protected delegate void TargetRegistryKeyAction(
            RegistryKey key, Dictionary<string, string> dictaionry, int count);

        protected delegate void TargetRegistryValueAction(
            RegistryKey key, string name, Dictionary<string, string> dictaionry, int count);

        protected delegate void SrcDstRegistryKeyAction(
            RegistryKey sourceKey, RegistryKey destinationKey, Dictionary<string, string> dictaionry, int count);

        protected delegate void SrcDstRegistryValueAction(
            RegistryKey sourceKey, RegistryKey destinationKey, string sourceName, string destinationName, Dictionary<string, string> dictaionry, int count);

        /// <summary>
        /// レジストリキーに対するシーケンシャル処理
        /// </summary>
        /// <param name="paths"></param>
        /// <param name="writable"></param>
        /// <param name="dictionary"></param>
        /// <param name="targetRegistryKeyAction"></param>
        protected void TargetKeySequence(
            string[] paths, bool writable, Dictionary<string, string> dictionary, TargetRegistryKeyAction targetRegistryKeyAction)
        {
            int count = 0;

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
                                targetRegistryKeyAction(childKey, dictionary, ++count);
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

                        targetRegistryKeyAction(regKey, dictionary, ++count);
                    }
                }
            }
        }

        /// <summary>
        /// レジストリキーに対するシーケンシャル処理。src/dst指定
        /// </summary>
        /// <param name="sourcePaths"></param>
        /// <param name="destinationPath"></param>
        /// <param name="writable"></param>
        /// <param name="dictionary"></param>
        /// <param name="srcDstRegistryKeyAction"></param>
        protected void SrcDstKeySequence(
            string[] sourcePaths, string destinationPath, bool writable, Dictionary<string, string> dictionary, SrcDstRegistryKeyAction srcDstRegistryKeyAction)
        {
            int count = 0;

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
                                foreach(var item in matchItems)
                                {
                                    using (RegistryKey sourceKey = parentKey.OpenSubKey(item, writable))
                                    {
                                        srcDstRegistryKeyAction(sourceKey, destinationKey, dictionary, ++count);
                                    }
                                }
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
                            srcDstRegistryKeyAction(sourceKey, destinationKey, dictionary, ++count);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// レジストリ値に対するシーケンシャル処理
        /// </summary>
        /// <param name="path"></param>
        /// <param name="names"></param>
        /// <param name="writable"></param>
        /// <param name="dictionary"></param>
        /// <param name="targetRegistryValueAction"></param>
        protected void TargetValueSequence(
            string path, string[] names, bool writable, Dictionary<string, string> dictionary, TargetRegistryValueAction targetRegistryValueAction)
        {
            using (RegistryKey parentKey = RegistryControl.GetRegistryKey(path, false, writable))
            {
                int count = 0;

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
                            targetRegistryValueAction(parentKey, valueName, dictionary, ++count);
                        }
                    }
                    else
                    {
                        targetRegistryValueAction(parentKey, name, dictionary, ++count);
                    }
                }
            }
        }

        /// <summary>
        /// レジストリ値に対するシーケンシャル処理。src/dst指定
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destinationPath"></param>
        /// <param name="sourceNames"></param>
        /// <param name="destinationName"></param>
        /// <param name="writable"></param>
        /// <param name="dictionary"></param>
        /// <param name="srcDstRegistryValueAction"></param>
        protected void SrcDstValueSequence(
            string sourcePath, string destinationPath, string[] sourceNames, string destinationName, bool writable, Dictionary<string, string> dictionary, SrcDstRegistryValueAction srcDstRegistryValueAction)
        {
            int count = 0;

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
                                foreach(var item in matchItems)
                                {
                                    using (RegistryKey sourceKey = parentKey.OpenSubKey(item, writable))
                                    {
                                        srcDstRegistryValueAction(sourceKey, destinationKey, name, destinationName, dictionary, ++count);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        using (RegistryKey destinationKey = RegistryControl.GetRegistryKey(destinationPath, true, true))
                        {
                            srcDstRegistryValueAction(parentKey, destinationKey, name, destinationName, dictionary, ++count);
                        }
                    }
                }
            }
        }
    }
}
