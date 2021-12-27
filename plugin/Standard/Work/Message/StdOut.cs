using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;

namespace Standard.Work.Message
{
    internal class StdOut : TaskJob
    {
        [TaskParameter(MandatoryAny = 1)]
        [Keys("message", "text")]
        protected string _Message { get; set; }

        [TaskParameter(MandatoryAny = 2, Resolv = true)]
        [Keys("messageresolvenv", "messageresolv", "textresolvenv", "textresolv")]
        protected string _MessageResolvEnv { get; set; }

        public override void MainProcess()
        {
            if (!string.IsNullOrEmpty(_Message))
            {
                Manager.WriteStandard(_Message);
            }
            if (!string.IsNullOrEmpty(_MessageResolvEnv))
            {
                Manager.WriteStandard(_MessageResolvEnv);
            }

            this.Success = true;
        }
    }
}
