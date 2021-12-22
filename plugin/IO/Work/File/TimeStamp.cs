using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using System.IO;
using IO.Lib;

namespace IO.Work.File
{
    /// <summary>
    /// ファイルのタイムスタンプを変更
    /// </summary>
    internal class TimeStamp : IOTaskWork
    {
        [TaskParameter(Mandatory = true, ResolvEnv = true, Delimiter = ';')]
        [Keys("path", "filepath", "target", "targetpath")]
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

            TargetFileProcess(_Path, TimeStampFileAction);
        }

        private void TimeStampFileAction(string target)
        {
            try
            {
                var info = new FileInfo(target);

                if(_CreationTime != null)
                {
                    info.CreationTime = (DateTime)_CreationTime;
                }
                if(_LastWriteTime != null)
                {
                    info.LastWriteTime = (DateTime)_LastWriteTime;
                }
                if(_LastAccessTime != null)
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
