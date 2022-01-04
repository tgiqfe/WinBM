using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;

namespace Standard.Work.Message
{
    internal class Log : TaskJob
    {
        [TaskParameter]
        [Keys("loglevel", "level")]
        [Values("none,non,no,not", "debug,dbg", "info,information", "attension,attention,atten", "warn,warning", "error,eror,err")]
        [ValidateEnumSet("None", "Debug", "Info", "Attention", "Warn", "Error")]
        protected LogLevel _LogLevel { get; set; }

        [TaskParameter(MandatoryAny = 1)]
        [Keys("message", "text")]
        protected string _Message { get; set; }

        [TaskParameter(MandatoryAny = 2, Resolv = true)]
        [Keys("messageresolvenv", "messageresolv", "textresolvenv", "textresolv",
            "messageresolveenv", "messageresolve", "textresolveenv", "textresolve")]
        protected string _MessageResolvEnv { get; set; }

        public override void MainProcess()
        {
            if (!string.IsNullOrEmpty(_Message))
            {
                Manager.WriteLog(_LogLevel, _Message);
            }
            if (!string.IsNullOrEmpty(_MessageResolvEnv))
            {
                Manager.WriteLog(_LogLevel, _MessageResolvEnv);
            }

            this.Success = true;
        }
    }
}
