using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using Standard.Lib;

namespace Standard.Require.HostName
{
    internal class Within : TaskJob
    {
        [TaskParameter]
        [Keys("hostname", "computername", "name", "host", "computer")]
        protected string _HostName { get; set; }

        [TaskParameter]
        [Keys("fromsetting", "setting")]
        protected bool? _FromSetting { get; set; }

        [TaskParameter]
        [Keys("invert", "not", "no", "none")]
        protected bool _Invert { get; set; }

        public override void MainProcess()
        {
            if (!string.IsNullOrEmpty(_HostName))
            {
                Manager.WriteLog(LogLevel.Info, "CheckTarget: {0}", _HostName);

                Success |= new CheckHostName().IsMatch(_HostName);
            }

            this.Success ^= this._Invert;
        }
    }
}
