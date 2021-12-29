using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using OSVersion.Lib;

namespace OSVersion.Require.Windows
{
    internal class Within : OSVersionTaskRequire
    {
        [TaskParameter(Mandatory = true)]
        [Keys("range", "ranges")]
        protected string[] _Range { get; set; }

        /// <summary>
        /// サーバOSであるかどうか。指定無しの場合は、どちらでも可
        /// </summary>
        [TaskParameter]
        [Keys("server", "isserver", "windowsserver", "winserver")]
        protected bool? _IsServer { get; set; }

        [TaskParameter]
        [Keys("invert", "not", "no", "none")]
        protected bool _Invert { get; set; }

        public override void MainProcess()
        {
            OSVersionInfoCollection collection = LoadOSVersionInfoDB();

            OSVersionInfo thisPC = OSVersionInfo.GetCurrent(collection);
            if (_IsServer != null && (bool)_IsServer != thisPC.IsServer)
            {
                Manager.WriteLog(WinBM.LogLevel.Attention, "Server OS is mismatch.");
                return;
            }

            //  現在実行中のOSが、_Rangeのいずれかの範囲に含まれていたらSuccess
            foreach (string range in _Range)
            {
                MinMaxOSVersionInfo minMax = new MinMaxOSVersionInfo(range);
                Manager.WriteLog(WinBM.LogLevel.Info, "Range: {0} - {1}", minMax.Min.VersionName, minMax.Max.VersionName);
                Success |= minMax.Within(thisPC);
            }

            if (!Success)
            {
                Manager.WriteLog(WinBM.LogLevel.Attention, "OS version is out of range. [{0}]", thisPC);
            }
        }
    }
}
