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
        [TaskParameter(Resolv = true)]
        [Keys("osversioninfodbfile", "osvinfodbfile", "osvinfodb")]
        protected string _OSVersionInfoDBFile { get; set; }

        public override void MainProcess()
        {
            Manager.PluginParam ??= new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(_OSVersionInfoDBFile))
            {
                Manager.PluginParam[Item.OSVERSION_OSVERSIONINFODBFILE] = this._OSVersionInfoDBFile;
            }
        }
    }
}
