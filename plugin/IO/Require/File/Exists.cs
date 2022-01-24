using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM.Task;

namespace IO.Require.File
{
    internal class Exists : TaskJob
    {
        [TaskParameter(Mandatory = true, Resolv = true)]
        [Keys("path", "filepath", "target", "targetpath")]
        protected string _Path { get; set; }

        [TaskParameter]
        [Keys("invert", "not", "no", "none")]
        protected bool _Invert { get; set; }

        public override void MainProcess()
        {
            this.Success = System.IO.File.Exists(_Path);

            if (!Success)
            {
                Manager.WriteLog(WinBM.LogLevel.Attention, "File is missing. \"{0}\"", _Path);
            }

            this.Success ^= this._Invert;
        }
    }
}
