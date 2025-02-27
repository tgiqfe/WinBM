﻿using System;
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

        [TaskParameter(Resolv = true)]
        [Keys("activity", "title")]
        protected string _Activity { get; set; }

        public override void MainProcess()
        {
            this.Success = Manager.Cmdlet != null;

            if (string.IsNullOrEmpty(_Activity))
            {
                _Activity = "Job";
            }
        }

        public override void Write(int line, string activity, int max, int cursor, string description)
        {
            int percent = (int)(100.0 / max * cursor);
            var record = new ProgressRecord(
                line,
                activity ?? _Activity,
                string.Format(" {0}%: {1}", percent, description));
            if (percent == 0)
            {
                record.RecordType = ProgressRecordType.Completed;
            }

            record.PercentComplete = percent;

            Manager.Cmdlet.WriteProgress(record);
        }

        
    }
}
