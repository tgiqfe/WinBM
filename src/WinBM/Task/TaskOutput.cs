using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinBM.Task
{
    public class TaskOutput : TaskBase
    {
        public enum OutputType
        {
            None,
            Standard,
            StandardError,
            Log,
            Task,
            ProgressBar,
        }

        public virtual OutputType Type { get; }

        public virtual void Write(string message) { }

        public virtual void Write(LogLevel level, string message) { }

        public virtual void Write(TaskBase taskBase) { }

        public virtual void Write(int line, int max, int cursor, string description) { }
    }
}
