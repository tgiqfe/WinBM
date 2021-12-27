using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using System.IO;
using IO.Lib;

namespace IO.Work.Directory
{
    internal class TimeStamp : IOTaskWork
    {
        [TaskParameter(Mandatory = true, Resolv = true, Delimiter = ';')]
        [Keys("path", "filepath", "target", "targetpath", "dirpath", "directorypath")]
        protected string[] _Path { get; set; }

        [TaskParameter(MandatoryAny = 1)]
        [Keys("creationtime", "creation", "creationdate")]
        protected DateTime? _CreationTime { get; set; }

        [TaskParameter(MandatoryAny = 2)]
        [Keys("lastwritetime", "lastwrite", "lastwritedate", "modifytime", "modifydate", "modtime", "moddate")]
        protected DateTime? _LastWriteTime { get; set; }

        [TaskParameter(MandatoryAny = 3)]
        [Keys("lastaccesstime", "lastaccess", "lastaccessdate")]
        protected DateTime? _LastAccessTime { get; set; }

        public override void MainProcess()
        {
            this.Success = true;

            TargetDirectoryProcess(_Path, TimeStampDirectoryAction);
        }

        private void TimeStampDirectoryAction(string target)
        {
            try
            {
                var info = new DirectoryInfo(target);

                if (_CreationTime != null)
                {
                    info.CreationTime = (DateTime)_CreationTime;
                }
                if (_LastWriteTime != null)
                {
                    info.LastWriteTime = (DateTime)_LastWriteTime;
                }
                if (_LastAccessTime != null)
                {
                    info.LastAccessTime = (DateTime)_LastAccessTime;
                }
            }
            catch (Exception e)
            {
                Manager.WriteLog(LogLevel.Error, "{0} {1}", this.TaskName, e.Message);
                Manager.WriteLog(LogLevel.Debug, e.ToString());
                this.Success = false;
            }
        }
    }
}
