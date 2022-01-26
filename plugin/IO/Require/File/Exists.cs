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
        [TaskParameter(Mandatory = true, Resolv = true, Delimiter = ';')]
        [Keys("path", "filepath", "target", "targetpath")]
        protected string[] _Path { get; set; }

        [TaskParameter]
        [Keys("invert", "not", "no", "none")]
        protected bool _Invert { get; set; }

        public override void MainProcess()
        {
            this.Success = true;

            foreach (var path in _Path)
            {
                bool ret = System.IO.File.Exists(path);
                if (!ret)
                {
                    Manager.WriteLog(WinBM.LogLevel.Attention, "File is missing. \"{0}\"", _Path);
                }

                Success &= ret;
            }

            this.Success ^= this._Invert;
        }
    }
}
