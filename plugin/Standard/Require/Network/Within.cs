using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using Standard.Lib;

namespace Standard.Require.Network
{
    internal class Within : TaskJob
    {
        [TaskParameter]
        [Keys("network", "address", "networkaddress", "ipaddress", "ipaddr", "addr")]
        protected string _NetworkAddress { get; set; }

        [TaskParameter]
        [Keys("fromsetting", "setting")]
        protected bool? _FromSetting { get; set; }

        [TaskParameter]
        [Keys("invert", "not", "no", "none")]
        protected bool _Invert { get; set; }

        public override void MainProcess()
        {
            if (!string.IsNullOrEmpty(_NetworkAddress))
            {
                Manager.WriteLog(LogLevel.Info, "CheckTarget: {0}", _NetworkAddress);

                Success |= new CheckNetworkAddress().IsMatch(_NetworkAddress);
            }

            this.Success ^= this._Invert;
        }
    }
}
