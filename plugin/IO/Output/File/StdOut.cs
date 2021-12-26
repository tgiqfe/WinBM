using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using IO.Lib;

namespace IO.Output.File
{
    internal class StdOut : TaskOutput
    {
        public override OutputType Type { get { return OutputType.Standard; } }

        [TaskParameter(Mandatory = true, Resolv = true)]
        [Keys("path", "filepath", "target", "targetpath")]
        protected string _Path { get; set; }

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

        public override void Write(string message)
        {
            using (var sw = new System.IO.StreamWriter(_Path, true, enc))
            {
                sw.WriteLine(message);
            }
        }
    }
}
