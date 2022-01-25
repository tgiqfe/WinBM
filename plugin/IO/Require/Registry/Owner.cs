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
using System.IO;
using IO.Lib;

namespace IO.Require.Registry
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Owner : RequireRegistry
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

        [TaskParameter]
        [Keys("invert", "not", "no", "none")]
        protected bool _Invert { get; set; }

        private UserAccount _ownerAccount = null;

        public override void MainProcess()
        {
            this.Success = true;

            //  Owner情報セット
            _ownerAccount = new UserAccount(_Account);

            TargetRegistryKeyProcess(_Path, writable: false, OwnerRegistryKeyAction);

            this.Success ^= this._Invert;
        }

        private void OwnerRegistryKeyAction(RegistryKey target)
        {
            try
            {
                RegistrySecurity security = target.GetAccessControl();

                //  必要に応じて再帰的に所有者チェック。
                //  不一致を確認した時点で再起チェックは終了。
                //  不一致の場合は所有者名を返し、一致の場合はnullを返す。
                Func<RegistryKey, RegistrySecurity, string> recurseCheck = null;
                recurseCheck = (targetKey, targetSecurity) =>
                {
                    string owner = targetSecurity.GetOwner(typeof(NTAccount)).Value;
                    if (_ownerAccount.IsMatch(owner))
                    {
                        if (_Recurse)
                        {
                            foreach (string child in targetKey.GetSubKeyNames())
                            {
                                using (RegistryKey childKey = targetKey.OpenSubKey(child, false))
                                {
                                    string childOwner = recurseCheck(childKey, childKey.GetAccessControl());
                                    if (childOwner != null)
                                    {
                                        return childOwner;
                                    }
                                }
                            }
                        }
                        return null;
                    }
                    return owner;
                };

                string targetOwner = recurseCheck(target, security);
                if (targetOwner == null)
                {
                    Manager.WriteLog(LogLevel.Info, "Owner match: {0}", targetOwner);
                }
                else
                {
                    Manager.WriteLog(LogLevel.Attention, "Owner not match: {0}", targetOwner);
                    this.Success = false;
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
