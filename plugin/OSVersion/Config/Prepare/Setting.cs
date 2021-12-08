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
        [Keys("osversioninfodbfile", "osvinfodbfile", "osvinfodb")]
        protected string _OSVersionInfoDBFile { get; set; }

        [TaskParameter]
        [Keys("persistent", "persist")]
        protected bool _Persistent { get; set; }

        public override void MainProcess()
        {
            Manager.Setting.PluginParam ??= new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(_OSVersionInfoDBFile))
            {
                Manager.Setting.PluginParam[Item.OSVERSION_OSVERSIONINFODBFILE] = this._OSVersionInfoDBFile;
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
