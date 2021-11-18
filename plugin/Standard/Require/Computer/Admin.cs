using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using System.Security.Principal;

namespace Standard.Require.Computer
{
    /// <summary>
    /// 管理者実行しているかどうかのチェック
    /// </summary>
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Admin : TaskJob
    {
        [TaskParameter]
        [Keys("invert", "not", "no", "none")]
        protected bool _Invert { get; set; }

        public override void MainProcess()
        {
            AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);
            WindowsPrincipal wp = (WindowsPrincipal)System.Threading.Thread.CurrentPrincipal;
            bool isAdmin = wp.IsInRole(WindowsBuiltInRole.Administrator);

            if (isAdmin)
            {
                Manager.WriteLog(LogLevel.Debug, "The process is running as a Trusted user.");
                this.Success = true;
            }
            else
            {
                Manager.WriteLog(LogLevel.Info, "The process is not running as a Trusted user.");
            }

            this.Success ^= this._Invert;
        }
    }
}
