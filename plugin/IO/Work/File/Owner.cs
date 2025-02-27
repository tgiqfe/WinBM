﻿using System;
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

namespace IO.Work.File
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

        private TrustedUser _trustedUser = null;
        private UserAccount _ownerAccount = null;
        private bool _abortRecurse = false;

        public override void MainProcess()
        {
            this.Success = true;

            //  事前定義アカウントチェック
            _ownerAccount = new UserAccount(_Account);

            TargetSequence(_Path, OwnerFileAction);
            
            _trustedUser?.RemovePrivilege();
        }

        private void OwnerFileAction(string target)
        {
            TakeOwnerFile(target);
        }

        private void TakeOwnerFile(string targetFile)
        {
            if (_abortRecurse) { return; }

            System.IO.FileInfo info = new System.IO.FileInfo(targetFile);
            try
            {
                FileSecurity security = info.GetAccessControl();
                security.SetOwner(_ownerAccount.NTAccount);
                info.SetAccessControl(security);
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

                FileSecurity security = info.GetAccessControl();
                security.SetOwner(_ownerAccount.NTAccount);
                info.SetAccessControl(security);
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
