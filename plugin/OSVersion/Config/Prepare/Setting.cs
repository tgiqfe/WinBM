using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;

namespace OSVersion.Config.Prepare
{
    internal class Setting : TaskConfig
    {
        [TaskParameter(ResolvEnv = true)]
        [Keys("osinfodbfile", "osinfodb")]
        protected string _OSInfoDBFile { get; set; }

        [TaskParameter]
        [Keys("persistent", "persist")]
        protected bool _Persistent { get; set; }

        public override void MainProcess()
        {
            Manager.Setting.PluginParam ??= new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(_OSInfoDBFile))
            {
                Manager.Setting.PluginParam[Item.OSVERSION_OSINFODBFILE] = this._OSInfoDBFile;
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
