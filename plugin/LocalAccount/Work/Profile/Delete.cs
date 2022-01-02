using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using LocalAccount.Lib;

namespace LocalAccount.Work.Profile
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Delete : TaskJob
    {
        [TaskParameter(Mandatory = true, Resolv = true)]
        [Keys("account", "acount", "username", "user")]
        protected string _UserName { get; set; }

        public override void MainProcess()
        {
            try
            {
                if (!string.IsNullOrEmpty(_UserName) &&
                    ProfileFunctions.GetLoggedOnUsers().All(x => x != _UserName))
                {
                    ProfileFunctions.DeleteProfile(_UserName);
                }
            }
            catch (Exception e)
            {
                GlobalLog.WriteLog(LogLevel.Error, "{0} {1}", this.TaskName, e.Message);
                GlobalLog.WriteLog(LogLevel.Debug, e.ToString());
            }
        }
    }

    internal class Remove : Delete
    {
        protected override bool IsAlias { get { return true; } }
    }
}
