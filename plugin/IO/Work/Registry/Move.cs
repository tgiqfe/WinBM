using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using Microsoft.Win32;
using IO.Lib;

namespace IO.Work.Registry
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Move : IOTaskWorkRegistry
    {
        //  SourcePath + DesrintaionPath ⇒ キーを移動
        //  SourcePath + SourceName + DestinationName ⇒ 同じキー内で値を移動
        //  SourcePath + DestinationPath + SourceName ⇒ 別キーの中に元と同じ名前の値を移動

        [TaskParameter(Mandatory = true, ResolvEnv = true)]
        [Keys("sourcepath", "srcpath", "src", "source", "sourcekey", "srckey", "path", "keypath")]
        protected string _SourcePath2 { get; set; }

        [TaskParameter(Mandatory = true, ResolvEnv = true, Delimiter = ';')]
        [Keys("sourcepath", "srcpath", "src", "source", "sourcekey", "srckey", "path", "keypath")]
        protected string[] _SourcePath { get; set; }

        [TaskParameter(ResolvEnv = true)]
        [Keys("destinationpath", "dstpath", "dst", "destination", "destinationkey", "dstkey")]
        protected string _DestinationPath { get; set; }

        [TaskParameter(ResolvEnv = true, Delimiter = '\n')]
        [Keys("sourcename", "srcname", "name", "name", "namae", "registryname", "regname", "paramname")]
        protected string[] _SourceName { get; set; }

        [TaskParameter(ResolvEnv = true)]
        [Keys("destinationname", "dstname")]
        protected string _DestinationName { get; set; }

        [TaskParameter]
        [Keys("force", "forse")]
        protected bool _Force { get; set; }

        public override void MainProcess()
        {
            this.Success = true;

            if (_SourceName?.Length > 0)
            {
                //  sourceNameが複数の場合は、destinationNameの指定は無視
                if (_SourceName.Length > 1) { _DestinationName = null; }

                //  SourceとDestinationのキーが同じ場合(Destinationキーがnullの場合も含む)、destinationNameの設定が必須。
                //  ※SourceとDestinationのキーが同じ場合、sourceNameは1つのみ指定可能。
                if ((_SourcePath[0].Equals(_DestinationPath, StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(_DestinationPath))
                    && _DestinationName == null)
                {
                    Manager.WriteLog(LogLevel.Error, "SourceKeyPath = DestinationKeyPath, and not destinationName.", _SourcePath[0]);
                    return;
                }

                //  移動先が存在し、force=falseの場合

                SrcDstRegistryValueProcess(_SourcePath[0], _DestinationPath, _SourceName, _DestinationName, false, MoveRegistryValueAction);
            }
            else
            {
                //  キー移動の為、DestinationPath必須
                if (string.IsNullOrEmpty(_DestinationPath))
                {
                    Manager.WriteLog(LogLevel.Error, "DestinationKeyPath is unset.");
                    return;
                }

                //  移動先が存在し、force=falseの場合
                bool registryKeyExists(string keyPath)
                {
                    using (RegistryKey checkingKey = RegistryControl.GetRegistryKey(keyPath, false, false))
                    {
                        return checkingKey != null;
                    }
                }
                if (registryKeyExists(_DestinationPath) && !_Force)
                {
                    Manager.WriteLog(LogLevel.Warn, "Destination path is already exists. \"{0}\"", _DestinationPath);
                    return;
                }

                SrcDstRegistryKeyProcess(_SourcePath, _DestinationPath, false, MoveRegistryKeyAction);
            }

            /*
            if (_SourceName?.Length > 0)
            {
                MoveRegistryValuceAction();
            }
            else
            {
                MoveRegistryKeyAction();
            }
            */
        }

        private void MoveRegistryValueAction(RegistryKey sourceKey, RegistryKey destinationKey, string sourceName, string destinationName)
        {
            try
            {
                //  コピー先が存在し、force=falseの場合
                if (destinationKey.GetValueNames().Any(x => x.Equals(destinationName, StringComparison.OrdinalIgnoreCase)) && !_Force)
                {
                    Manager.WriteLog(LogLevel.Warn, "Destination name is already exists. \"{0}\"", destinationName);
                    return;
                }

                RegistryValueKind valueKind = sourceKey.GetValueKind(sourceName);
                object srcValue = valueKind == RegistryValueKind.ExpandString ?
                    sourceKey.GetValue(sourceName, null, RegistryValueOptions.DoNotExpandEnvironmentNames) :
                    sourceKey.GetValue(sourceName);
                destinationKey.SetValue(destinationName, srcValue, valueKind);

                sourceKey.DeleteValue(sourceName);
            }
            catch (Exception e)
            {
                Manager.WriteLog(LogLevel.Error, "{0} {1}", this.TaskName, e.Message);
                Manager.WriteLog(LogLevel.Debug, e.ToString());
                this.Success = false;
            }
        }

        private void MoveRegistryKeyAction(RegistryKey sourceKey, RegistryKey destinationKey)
        {
            try
            {
                RegistryControl.CopyRegistryKey(sourceKey, destinationKey, null);
                sourceKey.DeleteSubKeyTree("");
            }
            catch (Exception e)
            {
                Manager.WriteLog(LogLevel.Error, "{0} {1}", this.TaskName, e.Message);
                Manager.WriteLog(LogLevel.Debug, e.ToString());
                this.Success = false;
            }
        }

        private void MoveRegistryKeyAction()
        {
            using (var sourceKey = RegistryControl.GetRegistryKey(_SourcePath2, false, true))
            {
                //  コピー元のキーが存在しない場合
                if (sourceKey == null)
                {
                    Manager.WriteLog(LogLevel.Error, "Target path is missing. \"{0}\"", _SourcePath2);
                    return;
                }
                try
                {
                    using (var destinationKey = RegistryControl.GetRegistryKey(_DestinationPath, false, false))
                    {
                        //  コピー先が存在し、force=falseの場合
                        if (destinationKey != null && !_Force)
                        {
                            Manager.WriteLog(LogLevel.Warn, "Destination path is already exists. \"{0}\"", _DestinationPath);
                            return;
                        }
                    }
                    using (var destinationKey = RegistryControl.GetRegistryKey(_DestinationPath, true, true))
                    {
                        RegistryControl.CopyRegistryKey(sourceKey, destinationKey, null);
                    }

                    //  コピー完了後に移動元を削除
                    sourceKey.DeleteSubKeyTree("");
                }
                catch (Exception e)
                {
                    Manager.WriteLog(LogLevel.Error, "{0} {1}", this.TaskName, e.Message);
                    Manager.WriteLog(LogLevel.Debug, e.ToString());
                    this.Success = false;
                }
            }
        }

        private void MoveRegistryValuceAction()
        {
            using (var sourceKey = RegistryControl.GetRegistryKey(_SourcePath2, false, true))
            {
                //  コピー元のキーが存在しない場合
                if (sourceKey == null)
                {
                    Manager.WriteLog(LogLevel.Error, "Target path is missing. \"{0}\"", _SourcePath2);
                    return;
                }

                //  名前を複数指定する場合は、コピー先名をnullに変更して、コピー元名と同じになるようにする。
                if (_SourceName.Length > 1)
                {
                    _DestinationName = null;
                }

                //  値コピー
                _DestinationPath ??= _SourcePath2;
                try
                {
                    using (var destinationKey = RegistryControl.GetRegistryKey(_DestinationPath, true, true))
                    {
                        Action<string, string> registryValueCopy = (srcName, dstName) =>
                        {
                            //  コピー先が存在し、force=falseの場合
                            if (destinationKey.GetValueNames().Any(x => x.Equals(dstName, StringComparison.OrdinalIgnoreCase)) && !_Force)
                            {
                                Manager.WriteLog(LogLevel.Warn, "Destination name is already exists. \"{0}\"", dstName);
                                return;
                            }
                            RegistryValueKind valueKind = sourceKey.GetValueKind(srcName);
                            object srcValue = valueKind == RegistryValueKind.ExpandString ?
                                sourceKey.GetValue(srcName, null, RegistryValueOptions.DoNotExpandEnvironmentNames) :
                                sourceKey.GetValue(srcName);
                            destinationKey.SetValue(dstName, srcValue, valueKind);

                            //  コピー完了後に移動元を削除
                            sourceKey.DeleteValue(srcName);
                        };

                        foreach (string sourceName in _SourceName)
                        {
                            if (sourceName.Contains("*"))
                            {
                                System.Text.RegularExpressions.Regex wildcard = Wildcard.GetPattern(sourceName);
                                sourceKey.GetValueNames().
                                    Where(x => wildcard.IsMatch(x)).
                                    ToList().
                                    ForEach(x => registryValueCopy(x, x));
                            }
                            else
                            {
                                registryValueCopy(sourceName, _DestinationName ?? sourceName);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Manager.WriteLog(LogLevel.Error, "{0} {1}", this.TaskName, e.Message);
                    Manager.WriteLog(LogLevel.Debug, e.ToString());
                    this.Success = false;
                }
            }
        }
    }
}
