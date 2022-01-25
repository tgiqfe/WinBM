using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using System.Security.Principal;
using System.Security.AccessControl;
using System.IO;
using IO.Lib;

namespace IO.Require.Directory
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Owner : RequireDirectory
    {
        [TaskParameter(Mandatory = true, Resolv = true, Delimiter = ';')]
        [Keys("path", "filepath", "target", "targetpath")]
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

            TargetDirectoryProcess(_Path, OwnerDirectoryAction);

            this.Success ^= this._Invert;
        }

        private void OwnerDirectoryAction(string target)
        {
            try
            {
                var security = new System.IO.DirectoryInfo(target).GetAccessControl();

                //  必要に応じて再帰的に所有者チェック。
                //  不一致を確認した時点で再起チェックは終了。
                //  不一致の場合は所有者名を返し、一致の場合はnullを返す。
                Func<string, DirectorySecurity, string> recurseCheck = null;
                recurseCheck = (targetPath, targetSecurity) =>
                {
                    string owner = targetSecurity.GetOwner(typeof(NTAccount)).Value;
                    if (_ownerAccount.IsMatch(owner))
                    {
                        if (_Recurse)
                        {
                            foreach (string child in System.IO.Directory.GetFiles(targetPath, "*", SearchOption.AllDirectories))
                            {
                                var childSecurity = new System.IO.FileInfo(child).GetAccessControl();
                                string childOwner = childSecurity.GetOwner(typeof(NTAccount)).Value;
                                if (!_ownerAccount.IsMatch(childOwner))
                                {
                                    return childOwner;
                                }
                            }
                            foreach (string child in System.IO.Directory.GetDirectories(targetPath, "*", SearchOption.AllDirectories))
                            {
                                var childSecurity = new System.IO.DirectoryInfo(child).GetAccessControl();
                                string childOwner = childSecurity.GetOwner(typeof(NTAccount)).Value;
                                if (!_ownerAccount.IsMatch(childOwner))
                                {
                                    return childOwner;
                                }
                            }
                        }
                        return null;
                    }
                    return owner;
                };

                string notMatchOwner = recurseCheck(target, security);
                if (notMatchOwner == null)
                {
                    Manager.WriteLog(LogLevel.Info, "Owner match: {0}", _ownerAccount);
                }
                else
                {
                    Manager.WriteLog(LogLevel.Attention, "Owner not match: {0}", notMatchOwner);
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
