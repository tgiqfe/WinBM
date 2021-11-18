using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using System.Threading;

namespace Standard.Require.Process
{
    internal class Exists : TaskJob
    {
        [TaskParameter(Mandatory = true)]
        [Keys("name", "processname", "procname", "targetprocess", "targetprocessname")]
        protected string[] _Name { get; set; }

        [TaskParameter(Unsigned = true)]
        [Keys("wait", "waittime", "waitmilliseconds", "waitmillisecond", "waitmillisec", "milliseconds", "millisecond", "msec")]
        protected int? _WaitTime { get; set; }

        [TaskParameter(Unsigned = true)]
        [Keys("waitseconds", "waitsecond", "seconds", "second", "sec")]
        protected int? _WaitTimeSec { get; set; }

        [TaskParameter(Unsigned = true)]
        [Keys("checkinterval", "interval")]
        protected int? _CheckInterval { get; set; }

        [TaskParameter(Unsigned = true)]
        [Keys("checkintervalseconds", "checkintervalsecond", "intervalseconds", "intervalsecond", "intervalsec")]
        protected int? _CheckIntervalSec { get; set; }

        [TaskParameter]
        [Keys("invert", "not", "no", "none")]
        protected bool _Invert { get; set; }

        public override void MainProcess()
        {
            this.Success = true;

            List<string> list = _Name.
                Select(x => System.IO.Path.GetFileNameWithoutExtension(x)).
                Reverse().
                ToList();
            int waitTime = (_WaitTime ?? 0) + ((_WaitTimeSec ?? 0) * 1000);
            int checkInterval = (_CheckInterval ?? 0) + ((_CheckIntervalSec ?? 0) * 1000);

            DateTime startTime = DateTime.Now;

            do
            {
                //  同名のプロセスが複数ヒットした場合は、最初の1つが見つかった時点で成功と判断
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    System.Diagnostics.Process[] processes =
                        System.Diagnostics.Process.GetProcessesByName(list[i]);
                    if (processes.Length > 0)
                    {
                        Manager.WriteLog(LogLevel.Info, "Process fined. pid={0}", processes[0].Id);
                        list.RemoveAt(i);
                    }
                }
                if (list.Count == 0) { break; }

                if (checkInterval > 0)
                {
                    Manager.WriteLog(LogLevel.Debug, $"Interval Wait {checkInterval}msec.");
                    Thread.Sleep(checkInterval);
                }
                else if (waitTime > 0)
                {
                    //  _CheckIntervalが0 or 無指定だが、_WaitTimeのみを指定している場合。
                    //  ここでスリープすると、次のwhileの時点ではwaitTime以上の時間を過ぎていることを期待。
                    Manager.WriteLog(LogLevel.Info, $"Wait {waitTime}msec.");
                    Thread.Sleep(waitTime);
                }
            } while (list.Count > 0 &&
                waitTime > 0 &&
                (DateTime.Now - startTime).TotalMilliseconds < waitTime);

            foreach (string name in list)
            {
                //  プロセスチェック対象のリストが空でなければSuccess = false
                this.Success = false;
                Manager.WriteLog(LogLevel.Info, $"Process missing. {name}");
            }

            this.Success ^= this._Invert;
        }
    }
}
