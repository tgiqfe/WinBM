using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using IO.Lib;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace IO.Require.File
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Owner : IOTaskRequire
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

        private bool _abortRecurse = false;

        public override void MainProcess()
        {
            Success = true;

            _Account = PredefinedAccount.Resolv(_Account);

            TargetFileProcess(_Path, OwnerFileAction);
        }

        private void OwnerFileAction(string target)
        {
            if (_abortRecurse) { return; }

            bool ret = true;
            var info = new System.IO.FileInfo(target);
            try
            {
                FileSecurity security = info.GetAccessControl();
                string owner = security.GetOwner(typeof(NTAccount)).Value;
                ret &= owner.Equals(_Account, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception e)
            {
                Manager.WriteLog(LogLevel.Error, "{0} {1}", this.TaskName, e.Message);
                Manager.WriteLog(LogLevel.Debug, e.ToString());
                this.Success = false;
            }

            Success &= ret;
        }
    }
}
