using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM.Task;
using System.Management.Automation;

namespace Standard.Output.Console
{
    internal class ProgressBar : TaskOutput
    {
        public override OutputType Type { get { return OutputType.ProgressBar; } }

        public override void MainProcess()
        {
            this.Success = Manager.Cmdlet != null;
        }

        public override void Write(int line, int max, int cursor, string description)
        {
            var record = new ProgressRecord(
                line,
                line switch
                {
                    1 => "Job",
                    2 => "Work",
                    _ => "",
                },
                description);
            record.PercentComplete = (int)(100.0 / max * cursor);

            Manager.Cmdlet.WriteProgress(record);
        }
    }
}
