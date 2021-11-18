using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM.Task;

namespace Standard.Output.Console
{
    internal class StdErr : TaskOutput
    {
        public override OutputType Type { get { return OutputType.StandardError; } }

        public override void MainProcess()
        {
            this.Success = true;
        }

        public override void Write(string message)
        {
            System.Console.ForegroundColor = ConsoleColor.DarkRed;
            System.Console.WriteLine(message);
            System.Console.ResetColor();
        }
    }
}
