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

        [TaskParameter]
        [Keys("invert", "not", "no", "none")]
        protected bool _Invert { get; set; }

        public override void MainProcess()
        {
            OSVersionInfoCollection collection = LoadOSInfoDB();

            OSVersionInfo thisPC = OSVersionInfo.GetCurrent(collection);

            //  現在実行中のOSが、_Rangeのいずれかの範囲に含まれていたらSuccess
            foreach (string range in _Range)
            {
                MinMaxOSVersionInfo minMax = new MinMaxOSVersionInfo(range);
                Success |= minMax.Within(thisPC);
            }

            if (!Success)
            {
                Manager.WriteLog(WinBM.LogLevel.Attention, "OS version is out of range. {0}", thisPC);
            }
        }
    }
}
