using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using LocalAccount.Lib;
using System.Management;

namespace LocalAccount.Work.Profile
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Delete : TaskJob
    {
        [TaskParameter(Resolv = true)]
        [Keys("account", "acount", "username", "user")]
        protected string[] _UserName { get; set; }

        [TaskParameter]
        [Keys("all")]
        protected bool? _All { get; set; }

        [TaskParameter]
        [Keys("excludeuser", "excludeusers", "exuser", "exusers", "exclude", "ex")]
        protected string[] _ExcludeUser { get; set; }

        public override void MainProcess()
        {
            try
            {
                var logonUsers = ProfileFunctions.GetLoggedOnUsers();

                if (_All ?? false)
                {
                    //  全プロファイル削除
                    //  削除対象外
                    //  - ローカルAdministrator
                    //  - C:\Windows配下にプロファイルパスを作っているユーザー(SSystem,LocalService,NetworkService、等) ※ProfileFunctions側で除外
                    //  - ExcludeUserで指定したユーザー
                    var list = new List<string>();
                    if (_ExcludeUser?.Length > 0) { list.AddRange(_ExcludeUser); }
                    list.AddRange(logonUsers);
                    list.Add(Environment.MachineName + "\\Administrator");
                    _ExcludeUser = list.ToArray();

                    ProfileFunctions.DeleteProfileAll(_ExcludeUser);
                }
                else if (_UserName?.Length > 0)
                {
                    //  ユーザー名指定でのプロファイル削除
                    foreach (string userName in _UserName)
                    {
                        if (logonUsers.All(x => x != userName))
                        {
                            ProfileFunctions.DeleteProfile(userName);
                        }
                    }
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
