using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;

namespace Standard.Config.Prepare
{
    internal class Setting : TaskConfig
    {
        [TaskParameter]
        [Keys("stepconfig", "configstep")]
        protected bool StepConfig { get; set; }

        [TaskParameter]
        [Keys("stepoutput", "outputstep")]
        protected bool StepOutput { get; set; }

        [TaskParameter]
        [Keys("steprequire", "requirestep")]
        protected bool StepRequire { get; set; }

        [TaskParameter]
        [Keys("stepwork", "workstep")]
        protected bool StepWork { get; set; }

        [TaskParameter(Resolv = true, Delimiter = ';')]
        [Keys("plugins", "pluginfiles")]
        protected string[] _PluginFiles { get; set; }

        [TaskParameter(Resolv = true)]
        [Keys("plugindir", "plugindirectory")]
        protected string _PluginDirectory { get; set; }

        [TaskParameter]
        [Keys("persistent", "persist")]
        protected bool _Persistent { get; set; }

        public override void MainProcess()
        {
            Manager.Setting.StepConfig = this.StepConfig;

            Manager.Setting.StepOutput = this.StepOutput;

            Manager.Setting.StepRequire = this.StepRequire;

            Manager.Setting.StepWork = this.StepWork;
            
            if (_PluginFiles?.Length > 0)
            {
                Manager.Setting.PluginFiles = this._PluginFiles;
            }

            if (!string.IsNullOrEmpty(_PluginDirectory))
            {
                Manager.Setting.PluginDirectory = this._PluginDirectory;
            }

            if (_Persistent)
            {
                this.IsPostSpec = true;
            }
        }

        public override void PostSpec()
        {
            Manager.Setting.Save();
        }
    }
}
