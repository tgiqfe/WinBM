using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM.Task;

namespace Standard.Output.Console
{
    internal class Task : TaskOutput
    {
        public override OutputType Type { get { return OutputType.Task; } }

        public override void MainProcess()
        {
            this.Success = true;
        }

        public override void Write(TaskBase taskBase)
        {
            System.Console.WriteLine(taskBase.TaskName);
        }
    }
}
