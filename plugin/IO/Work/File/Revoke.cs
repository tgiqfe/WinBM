using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using IO.Lib;
using System.IO;
using System.Security.Principal;
using System.Security.AccessControl;

namespace IO.Work.File
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Revoke : IOTaskWork
    {
        [TaskParameter(Mandatory = true, ResolvEnv = true, Delimiter = ';')]
        [Keys("path", "filepath", "target", "targetpath")]
        protected string[] _Path { get; set; }

        [TaskParameter(MandatoryAny = 1, ResolvEnv = true, Delimiter = ';')]
        [Keys("account", "acount", "owner", "own")]
        protected string[] _Account { get; set; }

        [TaskParameter(MandatoryAny = 2)]
        [Keys("all", "any")]
        protected bool _All { get; set; }

        public override void MainProcess()
        {
            this.Success = true;

            //  ドメインアカウント/ローカルアカウントの両方に対応させる為、事前定義アカウントチェックは無し。

            TargetFileProcess(_Path, RevokeFileAction);
        }

        private void RevokeFileAction(string target)
        {
            try
            {
                bool isChange = false;
                System.IO.FileInfo fInfo = new FileInfo(target);
                FileSecurity security = fInfo.GetAccessControl();
                if (_All)
                {
                    foreach (FileSystemAccessRule rule in security.GetAccessRules(true, false, typeof(NTAccount)))
                    {
                        security.RemoveAccessRule(rule);
                        isChange = true;
                    }
                }
                else
                {
                    foreach (FileSystemAccessRule rule in security.GetAccessRules(true, false, typeof(NTAccount)))
                    {
                        string targetAccount = rule.IdentityReference.Value;

                        if (_Account.Any(x =>
                            x.Contains("\\") && targetAccount.Equals(x, StringComparison.OrdinalIgnoreCase) ||
                            !x.Contains("\\") && targetAccount.EndsWith("\\" + x, StringComparison.OrdinalIgnoreCase)))
                        {
                            security.RemoveAccessRule(rule);
                            isChange = true;
                        }

                        /*
                        if (_Account.Contains("\\") && targetAccount.Equals(_Account, StringComparison.OrdinalIgnoreCase) ||
                            !_Account.Contains("\\") && targetAccount.EndsWith("\\" + _Account, StringComparison.OrdinalIgnoreCase))
                        {
                            security.RemoveAccessRule(rule);
                            isChange = true;
                        }
                        */
                    }
                }

                if (isChange)
                {
                    fInfo.SetAccessControl(security);
                }
                else
                {
                    Manager.WriteLog(LogLevel.Info, "Revoke not change. \"{0}\"", target);
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
