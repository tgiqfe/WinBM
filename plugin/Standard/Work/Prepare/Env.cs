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
        protected TargetScope _Scope { get; set; }

        public override void MainProcess()
        {
            try
            {
                foreach (KeyValuePair<string, string> pair in _EnvSet)
                {
                    string val = ExpandEnvironment(pair.Value);

                    if (this._Scope == TargetScope.File)
                    {
                        //WinBM.Lib.FileScope.Add(this.FilePath, pair.Key, pair.Value);
                        WinBM.Lib.FileScope.Add(this.FilePath, pair.Key, val);
                    }
                    else
                    {
                        Environment.SetEnvironmentVariable(
                            pair.Key,
                            //pair.Value,
                            val,
                            _Scope switch
                            {
                                TargetScope.Process => EnvironmentVariableTarget.Process,
                                TargetScope.User => EnvironmentVariableTarget.User,
                                TargetScope.Machine => EnvironmentVariableTarget.Machine,
                                TargetScope.Page => EnvironmentVariableTarget.Process,
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

            if (this._Scope == TargetScope.Page)
            {
                this.IsPostPage = true;
            }
            if(this._Scope == TargetScope.Process)
            {
                this.IsPostRecipe = true;
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
