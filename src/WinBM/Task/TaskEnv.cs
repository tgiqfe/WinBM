using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinBM.Task
{
    public class TaskEnv : TaskBase
    {
        protected enum EnvironmentScope
        {
            Process, File
        }

        [TaskParameter(Mandatory = true, EqualSign = '=', Delimiter = '\n')]
        [Keys("set", "envset", "envs", "environment", "environments")]
        protected Dictionary<string, string> _EnvSet { get; set; }

        /// <summary>
        /// 環境変数の適用範囲のスコープ。無指定の場合は[Process]
        /// </summary>
        [TaskParameter]
        [Keys("target", "envtarget", "targetenv", "scope", "targetscope", "envscope")]
        [Values("process,proc,proces", "file,recipefile")]
        protected EnvironmentScope _Target { get; set; }

        public override void MainProcess()
        {
            try
            {
                foreach (KeyValuePair<string, string> pair in _EnvSet)
                {
                    if (this._Target == EnvironmentScope.File)
                    {
                        FileScope2.Add(this.FilePath, pair.Key, pair.Value);
                        //this.Manager.FseCollection ??= new WinBM.FileScopeCollection();
                        //this.Manager.FseCollection.Add(this.FilePath, pair.Key, pair.Value);
                    }
                    else
                    {
                        Environment.SetEnvironmentVariable(
                            pair.Key,
                            pair.Value,
                            EnvironmentVariableTarget.Process);
                    }
                }
                this.Success = true;
            }
            catch (Exception e)
            {
                Manager.Setting.WriteLog(LogLevel.Error, "{0} {1}", this.TaskName, e.Message);
                Manager.Setting.WriteLog(LogLevel.Debug, e.ToString());
            }

            if (this._Target == EnvironmentScope.Process)
            {
                this.IsPostPage = true;
            }
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
