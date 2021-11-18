using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;

namespace Standard.Config.Prepare
{
    /// <summary>
    /// Page全体で使用するProcess環境変数をセット。
    /// Machine,User環境変数は、WorkのEnvで実装する
    /// </summary>
    internal class Env : TaskConfig
    {
        [TaskParameter(Mandatory = true, EqualSign = '=', Delimiter = '\n')]
        [Keys("set", "envset", "envs", "environment", "environments")]
        protected Dictionary<string, string> _EnvSet { get; set; }

        public override void MainProcess()
        {
            try
            {
                foreach (KeyValuePair<string, string> pair in _EnvSet)
                {
                    Environment.SetEnvironmentVariable(pair.Key, pair.Value, EnvironmentVariableTarget.Process);
                }
                this.Success = true;
            }
            catch (Exception e)
            {
                Manager.WriteLog(LogLevel.Error, "{0} {1}", this.TaskName, e.Message);
                Manager.WriteLog(LogLevel.Debug, e.ToString());
            }

            this.IsPostPage = true;
        }

        /// <summary>
        /// 全Page終了後の処理で削除
        /// </summary>
        public override void PostPage()
        {
            foreach (string key in _EnvSet.Keys)
            {
                Environment.SetEnvironmentVariable(key, null, EnvironmentVariableTarget.Process);
            }
        }
    }
}
