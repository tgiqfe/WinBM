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

namespace IO.Require.File
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Owner : RequireFile
    {
        [TaskParameter(Mandatory = true, Resolv = true, Delimiter = ';')]
        [Keys("path", "filepath", "target", "targetpath")]
        protected string[] _Path { get; set; }

        [TaskParameter(Mandatory = true, Resolv = true)]
        [Keys("account", "acount", "owner", "own")]
        protected string _Account { get; set; }

        [TaskParameter]
        [Keys("invert", "not", "no", "none")]
        protected bool _Invert { get; set; }

        private UserAccount _ownerAccount = null;

        public override void MainProcess()
        {
            this.Success = true;

            //  Owner情報セット
            _ownerAccount = new UserAccount(_Account);

            TargetFileProcess(_Path, OwnerFileAction);

            this.Success ^= this._Invert;
        }

        private void OwnerFileAction(string target)
        {
            try
            {
                var security = new System.IO.FileInfo(target).GetAccessControl();

                //  所有者チェック
                string targetOwner = security.GetOwner(typeof(NTAccount)).Value;
                if (_ownerAccount.IsMatch(targetOwner))
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
