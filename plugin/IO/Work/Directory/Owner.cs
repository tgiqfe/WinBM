using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Principal;
using System.Security.AccessControl;
using WinBM;
using WinBM.Task;
using IO.Lib;

namespace IO.Work.Directory
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Owner : IOTaskWork
    {
        [TaskParameter(Mandatory = true, Resolv = true, Delimiter = ';')]
        [Keys("path", "filepath", "target", "targetpath", "dirpath", "directorypath")]
        protected string[] _Path { get; set; }

        [TaskParameter(Mandatory = true, Resolv = true)]
        [Keys("account", "acount", "owner", "own")]
        protected string _Account { get; set; }

        [TaskParameter]
        [Keys("recurse", "recursive", "rec", "recurs")]
        protected bool _Recurse { get; set; }

        private TrustedUser _trustedUser = null;
        private bool _abortRecurse = false;

        public override void MainProcess()
        {
            this.Success = true;

            //  事前定義アカウントチェック
            _Account = PredefinedAccount.Resolv(_Account);

            TargetDirectoryProcess(_Path, OwnerDirectoryAction);

            _trustedUser?.RemovePrivilege();
        }

        private void OwnerDirectoryAction(string target)
        {
            var account = new NTAccount(_Account);

            if (_Recurse)
            {
                //  再帰処理有り
                TakeOwnerDirectory(target, account);
                System.IO.Directory.GetDirectories(target, "*", SearchOption.AllDirectories).
                    ToList().
                    ForEach(x => TakeOwnerDirectory(x, account));
                System.IO.Directory.GetFiles(target, "*", SearchOption.AllDirectories).
                    ToList().
                    ForEach(x => TakeOwnerFile(x, account));
            }
            else
            {
                //  再帰処理無し
                TakeOwnerDirectory(target, account);
            }
        }

        private void TakeOwnerDirectory(string targetDir, NTAccount account)
        {
            if (_abortRecurse) { return; }

            System.IO.DirectoryInfo info = new System.IO.DirectoryInfo(targetDir);
            try
            {
                DirectorySecurity security = info.GetAccessControl();
                security.SetOwner(account);
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

                DirectorySecurity security = info.GetAccessControl();
                security.SetOwner(account);
                info.SetAccessControl(security);
            }
            catch (Exception e)
            {
                Manager.WriteLog(LogLevel.Error, "{0} {1}", this.TaskName, e.Message);
                Manager.WriteLog(LogLevel.Debug, e.ToString());
                this.Success = false;
            }
        }

        private void TakeOwnerFile(string targetFile, NTAccount account)
        {
            if (_abortRecurse) { return; }

            System.IO.FileInfo info = new System.IO.FileInfo(targetFile);
            try
            {
                FileSecurity security = info.GetAccessControl();
                security.SetOwner(account);
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
                security.SetOwner(account);
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
