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

namespace Audit.Work.File
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Owner : WorkFile
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
            var dictionary = new Dictionary<string, string>();
            this.Success = true;

            _ownerAccount = new UserAccount(_Account);
            dictionary["Check_Owner"] = _ownerAccount.ToString();

            TargetSequence(_Path, dictionary, OwnerFileAction);

            AddAudit(dictionary, this._Invert);
        }

        private void OwnerFileAction(string target, Dictionary<string, string> dictionary, int count)
        {
            try
            {
                dictionary[$"file_{count}"] = target;
                FileSecurity security = new System.IO.FileInfo(target).GetAccessControl();

                //  所有者チェック
                string targetOwner = security.GetOwner(typeof(NTAccount)).Value;
                if (_ownerAccount.IsMatch(targetOwner))
                {
                    dictionary[$"file_{count}_Owner_Match"] = targetOwner;
                }
                else
                {
                    dictionary[$"file_{count}_Owner_NoMatch"] = targetOwner;
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
