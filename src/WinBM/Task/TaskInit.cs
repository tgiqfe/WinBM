using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM.Lib;

namespace WinBM.Task
{
    public class TaskInit : TaskBase
    {
        protected enum TargetScope
        {
            Process, File
        }

        /// <summary>
        /// 環境変数の適用範囲のスコープ。無指定の場合は[Process]
        /// </summary>
        [TaskParameter]
        [Keys("target", "envtarget", "targetenv", "scope", "targetscope", "envscope")]
        [Values("process,proc,proces", "file,recipefile")]
        protected TargetScope _Scope { get; set; }

        [TaskParameter(EqualSign = '=', Delimiter = '\n')]
        [Keys("set", "envset", "envs", "env", "environment", "environments")]
        protected Dictionary<string, string> _EnvSet { get; set; }

        [TaskParameter(Resolv = true, Delimiter = ';')]
        [Keys("plugins", "pluginfiles")]
        protected string[] _PluginFiles { get; set; }

        [TaskParameter(Resolv = true)]
        [Keys("plugindir", "plugindirectory")]
        protected string _PluginDirectory { get; set; }

        //  ########################

        [TaskParameter]
        [Keys("configstep", "stepconfig")]
        protected bool? _StepConfig { get; set; }

        [TaskParameter]
        [Keys("outputstep", "stepoutput")]
        protected bool? _StepOutput { get; set; }

        [TaskParameter]
        [Keys("requirestep", "steprequire")]
        protected bool? _StepRequire { get; set; }

        [TaskParameter]
        [Keys("workstep", "stepwork")]
        protected bool? _StepWork { get; set; }

        public override void MainProcess()
        {
            this.Success = true;

            try
            {
                //  環境変数のセット
                if (_EnvSet?.Count > 0)
                {
                    foreach (KeyValuePair<string, string> pair in _EnvSet)
                    {
                        string val = ExpandEnvironment(pair.Value);

                        if (this._Scope == TargetScope.File)
                        {
                            //FileScope.Add(this.FilePath, pair.Key, pair.Value);
                            WinBM.Lib.FileScope.Add(this.FilePath, pair.Key, val);
                        }
                        else
                        {
                            Environment.SetEnvironmentVariable(
                                pair.Key,
                                //pair.Value,
                                val,
                                EnvironmentVariableTarget.Process);
                        }
                    }
                    if (this._Scope == TargetScope.Process)
                    {
                        this.IsPostRecipe = true;
                    }
                }

                //  プラグインファイルのパスのセット
                if (_PluginFiles?.Length > 0)
                {
                    Manager.PluginFiles = this._PluginFiles;
                }

                //  プラグインファイルの保存先ディレクトリのセット
                if (!string.IsNullOrEmpty(_PluginDirectory))
                {
                    Manager.PluginDirectory = this._PluginDirectory;
                }

                //  Step設定のセット
                if (_StepConfig != null) { Manager.StepConfig = (bool)_StepConfig; }
                if (_StepOutput != null) { Manager.StepOutput = (bool)_StepOutput; }
                if (_StepRequire != null) { Manager.StepRequire = (bool)_StepRequire; }
                if (_StepWork != null) { Manager.StepWork = (bool)_StepWork; }
            }
            catch (Exception e)
            {
                GlobalLog.WriteLog(LogLevel.Error, "{0} {1}", this.TaskName, e.Message);
                GlobalLog.WriteLog(LogLevel.Debug, e.ToString());
            }
        }

        /// <summary>
        /// 全Page終了後の処理で削除
        /// </summary>
        public override void PostRecipe()
        {
            foreach (string key in _EnvSet.Keys)
            {
                Environment.SetEnvironmentVariable(key, null, EnvironmentVariableTarget.Process);
            }
        }
    }
}
