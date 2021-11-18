﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;

namespace Standard.Work.Computer
{
    internal class Env : TaskJob
    {
        [TaskParameter(Mandatory = true, Delimiter = '\n', EqualSign = '=')]
        [Keys("set", "envset", "envs", "environment", "environments")]
        protected Dictionary<string, string> _EnvSet { get; set; }

        [TaskParameter]
        [Keys("target", "envtarget", "targetenv")]
        [Values("process,proc,proces", "user,usr", "machine,mashine,masin,computer")]
        protected EnvironmentVariableTarget _Target { get; set; }

        public override void MainProcess()
        {
            try
            {
                foreach (KeyValuePair<string, string> pair in _EnvSet)
                {
                    Environment.SetEnvironmentVariable(pair.Key, pair.Value, this._Target);
                }
                this.Success = true;
            }
            catch (Exception e)
            {
                Manager.WriteLog(LogLevel.Error, "{0} {1}", this.TaskName, e.Message);
                Manager.WriteLog(LogLevel.Debug, e.ToString());
            }

            if (this._Target == EnvironmentVariableTarget.Process)
            {
                this.IsPostSpec = true;
            }
        }

        /// <summary>
        /// Process環境変数でセットした場合、後処理で削除
        /// </summary>
        public override void PostSpec()
        {
            foreach (string key in _EnvSet.Keys)
            {
                Environment.SetEnvironmentVariable(key, null, EnvironmentVariableTarget.Process);
            }
        }

    }
}
