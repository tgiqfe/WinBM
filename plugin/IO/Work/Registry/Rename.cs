using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using System.IO;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using IO.Lib;

namespace IO.Work.Registry
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Rename : TaskJob
    {
        [TaskParameter(Mandatory = true, ResolvEnv = true)]
        [Keys("path", "registrypath", "targetpath", "key", "registrykey", "targetkey", "regkey", "target")]
        protected string _Path { get; set; }

        [TaskParameter(ResolvEnv = true)]
        [Keys("name", "name", "namae", "registryname", "regname", "paramname")]
        protected string _Name { get; set; }

        [TaskParameter(Mandatory = true, ResolvEnv = true)]
        [Keys("newname", "new")]
        protected string _NewName { get; set; }

        #region Unmanaged code

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int RegRenameKey(
            Microsoft.Win32.SafeHandles.SafeRegistryHandle hKey,
            [MarshalAs(UnmanagedType.LPWStr)] string oldname,
            [MarshalAs(UnmanagedType.LPWStr)] string newname);

        #endregion

        public override void MainProcess()
        {
            this.Success = true;

            //  _Pathはワイルドカード指定も複数指定も無し

            if (_NewName.Contains("\\"))
            {
                _NewName = Path.GetFileName(_NewName);
            }

            if (string.IsNullOrEmpty(_Name))
            {
                RenameRegistryKeyAction();
            }
            else
            {
                RenameRegistryValueAction();
            }
        }

        /// <summary>
        /// レジストリキーを名前変更
        /// advapi32.dllのRegRenameKeyを使用した名前変更
        /// </summary>
        private void RenameRegistryKeyAction()
        {
            try
            {
                string parentKey = Path.GetDirectoryName(_Path);
                string oldName = Path.GetFileName(_Path);
                using (RegistryKey regKey = RegistryControl.GetRegistryKey(parentKey, false, true))
                {
                    //  対象キーの親キーが存在しない場合
                    if (regKey == null)
                    {
                        Manager.WriteLog(LogLevel.Warn, "Parent on target is Missing. \"{0}\"", parentKey);
                        return;
                    }

                    int ret = RegRenameKey(regKey.Handle, oldName, _NewName);
                    this.Success = ret == 0;
                }
            }
            catch (Exception e)
            {
                Manager.WriteLog(LogLevel.Error, "{0} {1}", this.TaskName, e.Message);
                Manager.WriteLog(LogLevel.Debug, e.ToString());
                this.Success = false;
            }
        }

        /// <summary>
        /// レジストリ値を変更
        /// コピーしてから元を削除
        /// </summary>
        private void RenameRegistryValueAction()
        {
            try
            {
                using (RegistryKey regKey = RegistryControl.GetRegistryKey(_Path, false, true))
                {
                    //  対象のキーが存在しない場合
                    if (regKey == null)
                    {
                        Manager.WriteLog(LogLevel.Warn, "Target is Missing. \"{0}\"", _Path);
                        return;
                    }

                    RegistryValueKind valueKind = regKey.GetValueKind(_Name);
                    object sourceValue = valueKind == RegistryValueKind.ExpandString ?
                        regKey.GetValue(_Name, null, RegistryValueOptions.DoNotExpandEnvironmentNames) :
                        regKey.GetValue(_Name);
                    regKey.SetValue(_NewName, sourceValue, valueKind);
                    regKey.DeleteValue(_Name);
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
