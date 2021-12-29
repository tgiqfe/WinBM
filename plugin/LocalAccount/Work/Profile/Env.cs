using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using Microsoft.Win32;
using System.Management;
using System.Security.Principal;

namespace LocalAccount.Work.Profile
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Env : TaskJob
    {
        protected enum TargetScope
        {
            Process, File
        }

        [TaskParameter(Resolv = true)]
        [Keys("account", "acount", "username", "user")]
        protected string _Account { get; set; }

        [TaskParameter]
        [Keys("name", "name", "namae")]
        protected string _Name { get; set; }

        [TaskParameter]
        [Keys("sid")]
        protected string _Sid { get; set; }

        [TaskParameter]
        [Keys("profilepath", "profile")]
        protected string _ProfilePath { get; set; }

        /// <summary>
        /// 環境変数の適用範囲のスコープ。無指定の場合は[Process]
        /// </summary>
        [TaskParameter]
        [Keys("target", "envtarget", "targetenv", "scope", "targetscope", "envscope")]
        [Values("process,proc,proces", "file,recipefile")]
        protected TargetScope _Scope { get; set; }

        public override void MainProcess()
        {
            this.Success = true;

            try
            {
                //  ユーザー名を環境変数にセット
                if (!string.IsNullOrEmpty(_Name))
                {
                    SetEnv(_Name, _Account);
                }

                //  SIDを環境変数にセット
                if (!string.IsNullOrEmpty(_Sid) && !string.IsNullOrEmpty(_Account))
                {
                    string sid = new NTAccount(_Account).Translate(typeof(SecurityIdentifier)).Value;
                    SetEnv(_Sid, sid);
                }

                //  プロファイルフォルダーパスを環境変数にセット
                if (!string.IsNullOrEmpty(_ProfilePath))
                {
                    string profilePath = _Account == "" ?
                        Registry.GetValue(
                            @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList",
                            "Default",
                            "") as string :
                        GetUserProfilePath(_Account);
                    SetEnv(_ProfilePath, profilePath);
                }

                if (this._Scope == TargetScope.Process)
                {
                    this.IsPostPage = true;
                }
            }
            catch (Exception e)
            {
                GlobalLog.WriteLog(LogLevel.Error, "{0} {1}", this.TaskName, e.Message);
                GlobalLog.WriteLog(LogLevel.Debug, e.ToString());
            }
        }

        /// <summary>
        /// 環境変数にセット
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        private void SetEnv(string key, string val)
        {
            if (this._Scope == TargetScope.File)
            {
                WinBM.Lib.FileScope.Add(this.FilePath, key, val);
            }
            else
            {
                Environment.SetEnvironmentVariable(key, val, EnvironmentVariableTarget.Process);
            }
        }

        /// <summary>
        /// ユーザー
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        private string GetUserProfilePath(string userName)
        {
            string sid = new NTAccount(userName).Translate(typeof(SecurityIdentifier)).Value;
            var mo = new ManagementClass("Win32_UserProfile").
                GetInstances().
                OfType<ManagementObject>().
                FirstOrDefault(x => x["SID"] as string == sid);
            return mo["LocalPath"] as string;
        }

        /// <summary>
        /// 全Page終了後の処理で削除
        /// </summary>
        public override void PostPage()
        {
            if (!string.IsNullOrEmpty(_Name))
            {
                Environment.SetEnvironmentVariable(_Name, null, EnvironmentVariableTarget.Process);
            }
            if (!string.IsNullOrEmpty(_Sid) && !string.IsNullOrEmpty(_Account))
            {
                Environment.SetEnvironmentVariable(_Sid, null, EnvironmentVariableTarget.Process);
            }
            if (!string.IsNullOrEmpty(_ProfilePath))
            {
                Environment.SetEnvironmentVariable(_ProfilePath, null, EnvironmentVariableTarget.Process);
            }
        }
    }
}
