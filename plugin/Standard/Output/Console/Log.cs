using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM.Task;
using WinBM;

namespace Standard.Output.Console
{
    internal class Log : TaskOutput
    {
        public override OutputType Type { get { return OutputType.Log; } }

        [TaskParameter]
        [Keys("minimumlevel", "min", "minimum", "minimumlv", "minlv")]
        [Values("none,non,no,not", "debug,dbg", "info,information", "attension,attention,atten", "warn,warning", "error,eror,err")]
        [ValidateEnumSet("None", "Debug", "Info", "Attention", "Warn", "Error")]
        protected LogLevel _MinimumLevel { get; set; }

        public override void MainProcess()
        {
            this.Success = true;
        }

        public override void Write(LogLevel level, string message)
        {
            if (level != LogLevel.None && level >= _MinimumLevel)
            {
                System.Console.WriteLine("[{0}][{1}] {2}",
                DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), level, message);
            }

            this.Success = true;
        }
    }
}
