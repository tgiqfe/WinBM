using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using System.IO;
using IO.Lib;

namespace IO.Output.File
{
    internal class Log : TaskOutput
    {
        public override OutputType Type { get { return OutputType.Log; } }

        [TaskParameter(Mandatory = true, Resolv = true)]
        [Keys("path", "filepath", "target", "targetpath")]
        protected string _Path { get; set; }

        [TaskParameter]
        [Keys("minimumlevel", "min", "minimum", "minimumlv", "minlv")]
        [Values("none,non,no,not", "debug,dbg", "info,information", "attension,attention,atten", "warn,warning", "error,eror,err")]
        [ValidateEnumSet("None", "Debug", "Info", "Attention", "Warn", "Error")]
        protected LogLevel _MinimumLevel { get; set; }

        [TaskParameter]
        [Keys("encoding", "encode", "enc")]
        protected string _Encoding { get; set; }

        private Encoding enc = null;

        public override void MainProcess()
        {
            string parent = System.IO.Path.GetDirectoryName(_Path);
            if (!System.IO.Directory.Exists(parent))
            {
                System.IO.Directory.CreateDirectory(parent);
            }

            enc = FileEncoding.Get(_Encoding);

            this.Success = true;
        }

        public override void Write(LogLevel level, string message)
        {
            if (level != LogLevel.None && level >= _MinimumLevel)
            {
                using (var sw = new StreamWriter(_Path, true, enc))
                {
                    sw.WriteLine("[{0}][{1}] {2}",
                        DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), level, message);
                }
            }

            this.Success = true;
        }
    }
}
