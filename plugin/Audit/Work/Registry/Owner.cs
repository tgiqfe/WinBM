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

namespace Audit.Work.Registry
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Owner : WorkRegistry
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
            var dictionary = new Dictionary<string, string>();
            this.Success = true;

            _ownerAccount = new UserAccount(_Account);
            dictionary["Check_Owner"] = _ownerAccount.ToString();

            TargetKeySequence(_Path, false, dictionary, SecurityRegistryKeyAction);

            AddAudit(dictionary, this._Invert);
        }

        private void SecurityRegistryKeyAction(RegistryKey target, Dictionary<string, string> dictionary, int count)
        {
            try
            {
                dictionary[$"registryKey_{count}"] = target.Name;

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
                    dictionary[$"registryKey_{count}_Owner_Match"] = _ownerAccount.ToString();
                }
                else
                {
                    dictionary[$"registryKey_{count}_Owner_NotMatch"] = targetOwner;
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
