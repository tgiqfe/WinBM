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
        [TaskParameter(Mandatory = true, Resolv = true, Delimiter = ';')]
        [Keys("path", "registrypath", "targetpath", "key", "registrykey", "targetkey", "regkey", "target")]
        protected string[] _Path { get; set; }

        [TaskParameter(Mandatory = true, Resolv = true)]
        [Keys("account", "acount", "owner", "own")]
        protected string _Account { get; set; }

        [TaskParameter]
        [Keys("recurse", "recursive", "rec", "recurs")]
        protected bool _Recurse { get; set; }

        private TrustedUser _trustedUser = null;
        private UserAccount _ownerAccount = null;
        private bool _abortRecurse = false;

        public override void MainProcess()
        {
            this.Success = true;

            //  事前定義アカウントチェック
            //_Account = PredefinedAccount.Resolv(_Account);
            _ownerAccount = new UserAccount(_Account);

            TargetRegistryKeyProcess(_Path, writable: true, OwnerRegistryAction);

            _trustedUser?.RemovePrivilege();
        }

        private void OwnerRegistryAction(RegistryKey targetKey)
        {
            var account = new NTAccount(_Account);

            if (_Recurse)
            {
                //  再起処理有り
                Action<RegistryKey> recursiveTree = null;
                recursiveTree = (target) =>
                {
                    TakeOwnerRegistryKey(target);
                    foreach (string child in target.GetSubKeyNames())
                    {
                        using (RegistryKey childKey = target.OpenSubKey(child, true))
                        {
                            recursiveTree(childKey);
                        }
                    }
                };
                recursiveTree(targetKey);
            }
            else
            {
                //  再起処理無し
                TakeOwnerRegistryKey(targetKey);
            }
        }

        private void TakeOwnerRegistryKey(RegistryKey targetKey)
        {
            if (_abortRecurse) { return; }

            try
            {
                RegistrySecurity security = targetKey.GetAccessControl();
                security.SetOwner(_ownerAccount.NTAccount);
                targetKey.SetAccessControl(security);
            }
            catch (InvalidOperationException ioe)
            {
                //  一度所有者変更を失敗した場合、特権Token取得して再チャレンジ
                _trustedUser ??= new TrustedUser();
                if (!_trustedUser.Enabled)
                {
                    Manager.WriteLog(LogLevel.Info, "The process is not running as a Trusted user.");
                    this.Success = false;
                    _abortRecurse = true;
                    return;
                }

                Manager.WriteLog(LogLevel.Debug, "{0} {1}", this.TaskName, ioe.Message);
                Manager.WriteLog(LogLevel.Info, "Get TokenManipulator SE_RESTORE_NAME.");

                RegistrySecurity security = targetKey.GetAccessControl();
                security.SetOwner(_ownerAccount.NTAccount);
                targetKey.SetAccessControl(security);
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
