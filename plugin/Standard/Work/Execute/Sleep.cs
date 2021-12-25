using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using System.Threading;

namespace Standard.Work.Execute
{
    internal class Sleep : TaskJob
    {
        [TaskParameter(MandatoryAny = 1, Unsigned = true, ResolvEnv = true)]
        [Keys("seconds", "second", "sec")]
        protected int? _Seconds { get; set; }

        [TaskParameter(MandatoryAny = 2, Unsigned = true, ResolvEnv = true)]
        [Keys("milliseconds", "millisecond", "milli", "msec", "millisec")]
        protected int? _MilliSeconds { get; set; }

        /// <summary>
        /// 指定時間待機
        /// </summary>
        public override void MainProcess()
        {
            int msec = (_Seconds ?? 0) * 1000 + (_MilliSeconds ?? 0);

            Thread.Sleep(msec);
        }
    }
}
