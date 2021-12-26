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

        public override void MainProcess()
        {
            this.Success = true;

            //  事前定義アカウントチェック
            _Account = PredefinedAccount.Resolv(_Account);

            TargetDirectoryProcess(_Path, OwnerDirectoryAction);
        }

        private void OwnerDirectoryAction(string target)
        {
            bool hasToken = false;

            var account = new NTAccount(_Account);

            //  対象のフォルダーに対して所有者変更
            Action<string> takeOwnerDirectory = (targetDir) =>
            {
                System.IO.DirectoryInfo info = new System.IO.DirectoryInfo(targetDir);
                try
                {
                    DirectorySecurity security = info.GetAccessControl();
                    security.SetOwner(account);
                    info.SetAccessControl(security);
                }
                catch (InvalidOperationException ioe)
                {
                    Manager.WriteLog(LogLevel.Debug, "{0} {1}", this.TaskName, ioe.Message);
                    Manager.WriteLog(LogLevel.Info, "Get TokenManipulator SE_RESTORE_NAME.");

                    hasToken = true;
                    TokenManipulator.AddPrivilege(TokenManipulator.SE_RESTORE_NAME);
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
            };

            //  対象のファイルに対して所有者変更
            Action<string> takeOwnerFile = (targetFile) =>
            {
                System.IO.FileInfo info = new System.IO.FileInfo(targetFile);
                try
                {
                    FileSecurity security = info.GetAccessControl();
                    security.SetOwner(account);
                    info.SetAccessControl(security);
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
            };

            if (_Recurse)
            {
                //  再帰処理有り
                takeOwnerDirectory(target);
                System.IO.Directory.GetDirectories(target, "*", SearchOption.AllDirectories).
                    ToList().
                    ForEach(x => takeOwnerDirectory(x));
                System.IO.Directory.GetFiles(target, "*", SearchOption.AllDirectories).
                    ToList().
                    ForEach(x => takeOwnerFile(x));
            }
            else
            {
                //  再帰処理無し
                takeOwnerDirectory(target);
            }

            if (hasToken)
            {
                TokenManipulator.RemovePrivilege(TokenManipulator.SE_RESTORE_NAME);
            }



            /*
            System.IO.DirectoryInfo dInfo = new System.IO.DirectoryInfo(target);

            try
            {
                DirectorySecurity security = dInfo.GetAccessControl();
                security.SetOwner(new NTAccount(_Account));
                dInfo.SetAccessControl(security);
            }
            catch (InvalidOperationException ioe)
            {
                Manager.WriteLog(LogLevel.Debug, "{0} {1}", this.TaskName, ioe.Message);
                Manager.WriteLog(LogLevel.Info, "Get TokenManipulator SE_RESTORE_NAME.");

                TokenManipulator.AddPrivilege(TokenManipulator.SE_RESTORE_NAME);
                DirectorySecurity security = dInfo.GetAccessControl();
                security.SetOwner(new NTAccount(_Account));
                dInfo.SetAccessControl(security);
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
