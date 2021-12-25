using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using Standard.Lib;

namespace Standard.Work.Prepare
{
    internal class Env : TaskJob
    {
        [TaskParameter(Mandatory = true, Delimiter = '\n', EqualSign = '=')]
        [Keys("set", "envset", "envs", "environment", "environments")]
        protected Dictionary<string, string> _EnvSet { get; set; }

        /// <summary>
        /// 環境変数の適用範囲のスコープ。無指定の場合は[Process]
        /// </summary>
        [TaskParameter]
        [Keys("target", "envtarget", "targetenv", "scope", "targetscope", "envscope")]
        [Values("process,proc,proces", "user,usr", "machine,mashine,masin,computer", "file,recipefile", "page,pag,pege")]
        protected EnvironmentScope _Target { get; set; }

        public override void MainProcess()
        {
            try
            {
                foreach (KeyValuePair<string, string> pair in _EnvSet)
                {
                    if (this._Target == EnvironmentScope.File)
                    {
                        WinBM.Lib.FileScope.Add(this.FilePath, pair.Key, pair.Value);
                        //this.Manager.FseCollection ??= new WinBM.FileScopeCollection();
                        //this.Manager.FseCollection.Add(this.FilePath, pair.Key, pair.Value);
                    }
                    else
                    {
                        Environment.SetEnvironmentVariable(
                            pair.Key,
                            pair.Value,
                            _Target switch
                            {
                                EnvironmentScope.Process => EnvironmentVariableTarget.Process,
                                EnvironmentScope.User => EnvironmentVariableTarget.User,
                                EnvironmentScope.Machine => EnvironmentVariableTarget.Machine,
                                EnvironmentScope.Page => EnvironmentVariableTarget.Process,
                                _ => EnvironmentVariableTarget.Process,
                            });
                    }
                }
                this.Success = true;
            }
            catch (Exception e)
            {
                Manager.WriteLog(LogLevel.Error, "{0} {1}", this.TaskName, e.Message);
                Manager.WriteLog(LogLevel.Debug, e.ToString());
            }

            if (this._Target == EnvironmentScope.Page)
            {
                this.IsPostSpec = true;
            }
            if(this._Target == EnvironmentScope.Process)
            {
                this.IsPostPage = true;
            }
        }

        /// <summary>
        /// スコープ[Page]でセットした場合、Page内の処理終了後に削除
        /// </summary>
        public override void PostSpec()
        {
            foreach (string key in _EnvSet.Keys)
            {
                Environment.SetEnvironmentVariable(key, null, EnvironmentVariableTarget.Process);
            }
        }

        /// <summary>
        /// スコープ[Process]でセットした場合、全Page終了後に削除
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
