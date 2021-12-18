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

namespace IO.Work.File
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Owner : IOTaskWork
    {
        [TaskParameter(Mandatory = true, ResolvEnv = true, Delimiter = ';')]
        [Keys("path", "filepath", "target", "targetpath")]
        protected string[] _Path { get; set; }

        [TaskParameter(Mandatory = true, ResolvEnv = true)]
        [Keys("account", "acount", "owner", "own")]
        protected string _Account { get; set; }

        public override void MainProcess()
        {
            this.Success = true;

            //  事前定義アカウントチェック
            _Account = PredefinedAccount.Resolv(_Account);

            TargetFileProcess(_Path, OwnerFileAction);
        }

        private void OwnerFileAction(string target)
        {
            System.IO.FileInfo fInfo = new System.IO.FileInfo(target);

            try
            {
                FileSecurity security = fInfo.GetAccessControl();
                security.SetOwner(new NTAccount(_Account));
                fInfo.SetAccessControl(security);
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

                TokenManipulator.AddPrivilege(TokenManipulator.SE_RESTORE_NAME);
                FileSecurity security = fInfo.GetAccessControl();
                security.SetOwner(new NTAccount(_Account));
                fInfo.SetAccessControl(security);
                TokenManipulator.RemovePrivilege(TokenManipulator.SE_RESTORE_NAME);
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
