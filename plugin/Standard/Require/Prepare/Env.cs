using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using Standard.Lib;

namespace Standard.Require.Prepare
{
    /// <summary>
    /// 環境変数がセットされているかどうかのチェック
    /// </summary>
    internal class Env : TaskJob
    {
        [TaskParameter(Mandatory = true)]
        [Keys("set", "envset", "envs", "environment", "environments", "name", "key")]
        protected string[] _Key { get; set; }

        [TaskParameter(Resolv = true)]
        [Keys("value", "val", "valu", "registryvalue", "regvalue")]
        protected string _Value { get; set; }

        /// <summary>
        /// 環境変数の適用範囲のスコープ。無指定の場合は全スコープ
        /// </summary>
        [TaskParameter]
        [Keys("target", "envtarget", "targetenv", "scope", "targetscope", "envscope")]
        [Values("process,proc,proces", "user,usr", "machine,mashine,masin,computer", "file,recipefile", "page,pag,pege")]
        protected TargetScope _Scope { get; set; }

        public override void MainProcess()
        {
            this.Success = true;

            try
            {
                foreach (string key in _Key)
                {
                    //  環境変数を取得
                    string val = _Scope switch
                    {
                        TargetScope.File => WinBM.Lib.FileScope.GetValue(key),
                        TargetScope.User => Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.User),
                        TargetScope.Machine => Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Machine),
                        _ => Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process),
                    };

                    if (string.IsNullOrEmpty(val))
                    {
                        Manager.WriteLog(LogLevel.Attention, "Env var is missing. {0}", val);
                        Success = false;
                        return;
                    }
                    if (!string.IsNullOrEmpty(_Value) &&
                        !val.Equals(_Value, StringComparison.OrdinalIgnoreCase))
                    {
                        Manager.WriteLog(LogLevel.Attention, "Env var is not match. {0} : {1}", val, key);
                        Success = false;
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Manager.WriteLog(LogLevel.Error, "{0} {1}", this.TaskName, e.Message);
                Manager.WriteLog(LogLevel.Debug, e.ToString());
            }
        }
    }
}
