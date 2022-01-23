using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using System.IO;

namespace IO.Require.File
{
    /// <summary>
    /// ファイルの読み取り/書き込み可否を確認し、可能であればSuccess
    /// - Read = true ⇒ 読み取り可否チェック
    /// - Write = true ⇒ 書き込み可否チェック
    /// - 両方無指定 or Read/Write両方true ⇒ 読み取り書き込み両方をチェック
    /// - 対象ファイルが存在しない ⇒ 問答無用でFailed
    /// </summary>
    internal class ReadWriteAble : TaskJob
    {
        [TaskParameter(Mandatory = true, Resolv = true, Delimiter = ';')]
        [Keys("path", "filepath", "target", "targetpath")]
        protected string[] _Path { get; set; }

        [TaskParameter]
        [Keys("read", "reader", "readlock", "readerlock", "readable")]
        protected bool? _Read { get; set; }

        [TaskParameter]
        [Keys("write", "writer", "writelock", "writerlock", "writable")]
        protected bool? _Write { get; set; }

        [TaskParameter]
        [Keys("invert", "not", "no", "none")]
        protected bool _Invert { get; set; }

        public override void MainProcess()
        {
            this.Success = true;

            foreach (string path in _Path)
            {
                ReadWriteAbleFileAction(path);
            }

            this.Success ^= this._Invert;
        }

        private void ReadWriteAbleFileAction(string target)
        {
            if (System.IO.File.Exists(target))
            {
                try
                {
                    if ((_Read == null && _Write == null) ||
                        ((_Read ?? false) && (_Write ?? false)))
                    {
                        //  Read/Write両方指定無し or Read/Write両方true
                        using (FileStream fs = new FileStream(target, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                        {
                            Manager.WriteLog(LogLevel.Info, "Target file is able to read/write. \"{0}\"", target);
                        }
                    }
                    else
                    {
                        if (_Read ?? false)
                        {
                            //  Readのみtrue
                            using (FileStream fs = new FileStream(target, FileMode.Open, FileAccess.Read, FileShare.None))
                            {
                                Manager.WriteLog(LogLevel.Info, "Target file is able to read. \"{0}\"", target);
                            }
                        }
                        else if (_Write ?? false)
                        {
                            //  Writeのみtrue
                            using (FileStream fs = new FileStream(target, FileMode.Open, FileAccess.Write, FileShare.None))
                            {
                                Manager.WriteLog(LogLevel.Info, "Target file is able to write. \"{0}\"", target);
                            }
                        }
                    }
                }
                catch
                {
                    Manager.WriteLog(LogLevel.Info, "Target file is not able to read or write. \"{0}\"", target);
                    Success = false;
                }
            }
            else
            {
                Manager.WriteLog(LogLevel.Info, "Target file is missing. \"{0}\"", target);
                Success = false;
                return;
            }
        }
    }
}
