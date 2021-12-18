using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using Microsoft.Win32;
using System.Security.Principal;
using System.Security.AccessControl;
using IO.Lib;

namespace IO.Work.Registry
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Owner : IOTaskWorkRegistry
    {
        [TaskParameter(Mandatory = true, ResolvEnv = true, Delimiter = ';')]
        [Keys("path", "registrypath", "targetpath", "key", "registrykey", "targetkey", "regkey", "target")]
        protected string[] _Path { get; set; }

        [TaskParameter(Mandatory = true, ResolvEnv = true)]
        [Keys("account", "acount", "owner", "own")]
        protected string _Account { get; set; }

        [TaskParameter]
        [Keys("recurse", "recursive", "rec", "recurs")]
        protected bool _Recurse { get; set; }

        public override void MainProcess()
        {
            this.Success = true;

            //  事前定義アカウントチェック
            _Account = PredefinedAccount.Resolv(_Account);

            TargetRegistryKeyProcess(_Path, writable: true, OwnerRegistryAction);
        }

        private void OwnerRegistryAction(RegistryKey target)
        {
            bool hasToken = false;

            var account = new NTAccount(_Account);

            Action<RegistryKey> takeOwnerKey = null;
            takeOwnerKey = (targetKey) =>
            {
                try
                {
                    RegistrySecurity security = targetKey.GetAccessControl();
                    security.SetOwner(account);
                    targetKey.SetAccessControl(security);
                }
                catch (InvalidOperationException ioe)
                {
                    //  一度所有者変更を失敗した後、管理者権限で動作していないならば終了
                    AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);
                    WindowsPrincipal wp = (WindowsPrincipal)System.Threading.Thread.CurrentPrincipal;
                    bool isAdmin = wp.IsInRole(WindowsBuiltInRole.Administrator);
                    if (!isAdmin)
                    {
                        Manager.WriteLog(LogLevel.Info, "The process is not running as a Trusted user.");
                        return;
                    }

                    Manager.WriteLog(LogLevel.Debug, "{0} {1}", this.TaskName, ioe.Message);
                    Manager.WriteLog(LogLevel.Info, "Get TokenManipulator SE_RESTORE_NAME.");

                    hasToken = true;
                    TokenManipulator.AddPrivilege(TokenManipulator.SE_RESTORE_NAME);
                    RegistrySecurity security = targetKey.GetAccessControl();
                    security.SetOwner(account);
                    targetKey.SetAccessControl(security);
                }
                catch (Exception e)
                {
                    Manager.WriteLog(LogLevel.Error, "{0} {1}", this.TaskName, e.Message);
                    Manager.WriteLog(LogLevel.Debug, e.ToString());
                    this.Success = false;
                }

                //  再帰処理有りの場合
                if (_Recurse)
                {
                    foreach (string keyName in targetKey.GetSubKeyNames())
                    {
                        using (RegistryKey subKey = targetKey.OpenSubKey(keyName, true))
                        {
                            takeOwnerKey(subKey);
                        }
                    }
                }
            };

            takeOwnerKey(target);

            if (hasToken)
            {
                TokenManipulator.RemovePrivilege(TokenManipulator.SE_RESTORE_NAME);
            }


            /*
            //  ここに再帰処理を実装する予定

            try
            {
                RegistrySecurity security = target.GetAccessControl();
                security.SetOwner(new NTAccount(_Account));
                target.SetAccessControl(security);
            }
            catch (InvalidOperationException ioe)
            {
                Manager.WriteLog(LogLevel.Debug, "{0} {1}", this.TaskName, ioe.Message);
                Manager.WriteLog(LogLevel.Info, "Get TokenManipulator SE_RESTORE_NAME.");

                TokenManipulator.AddPrivilege(TokenManipulator.SE_RESTORE_NAME);
                RegistrySecurity security = target.GetAccessControl();
                security.SetOwner(new NTAccount(_Account));
                target.SetAccessControl(security);
                TokenManipulator.RemovePrivilege(TokenManipulator.SE_RESTORE_NAME);
            }
            catch (Exception e)
            {
                Manager.WriteLog(LogLevel.Error, "{0} {1}", this.TaskName, e.Message);
                Manager.WriteLog(LogLevel.Debug, e.ToString());
                this.Success = false;
            }
            */
        }
    }
}
