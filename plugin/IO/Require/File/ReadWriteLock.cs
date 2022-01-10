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
    /// ファイルの読み取り/書き込みロックの有無を確認し、ロック状態ならばSuccess
    /// - Read = true ⇒ 読み取りロックを確認
    /// - Write = true ⇒ 書き込みロックを確認
    /// - 両方無指定 or Read/Write両方true ⇒ 読み取り書き込みロックを確認
    /// - 対象ファイルが存在しない ⇒ 問答無用でFailed
    /// </summary>
    internal class ReadWriteLock : TaskJob
    {
        [TaskParameter(Mandatory = true, Resolv = true, Delimiter = ';')]
        [Keys("path", "filepath", "target", "targetpath")]
        protected string[] _Path { get; set; }

        [TaskParameter]
        [Keys("read", "reader", "readlock", "readerlock")]
        protected bool? _Read { get; set; }

        [TaskParameter]
        [Keys("write", "writer", "writelock", "writerlock")]
        protected bool? _Write { get; set; }

        [TaskParameter]
        [Keys("invert", "not", "no", "none")]
        protected bool _Invert { get; set; }

        public override void MainProcess()
        {
            this.Success = true;

            foreach (string path in _Path)
            {
                if (System.IO.File.Exists(path))
                {
                    try
                    {
                        if ((_Read == null && _Write == null) ||
                            ((_Read ?? false) && (_Write ?? false)))
                        {
                            //  Read/Write両方指定無し or Read/Write両方true
                            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) { }
                        }
                        else
                        {
                            if (_Read ?? false)
                            {
                                //  Readのみtrue
                                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None)) { }
                            }
                            else if (_Write ?? false)
                            {
                                //  Writeのみtrue
                                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.None)) { }
                            }
                        }
                        Manager.WriteLog(LogLevel.Info, "Target file is not locked. \"{0}\"", path);
                        Success = false;
                    }
                    catch
                    {
                        Manager.WriteLog(LogLevel.Info, "Target file is locked. \"{0}\"", path);
                    }
                }
                else
                {
                    Manager.WriteLog(LogLevel.Error, "Target file is missing. \"{0}\"", path);
                    Success = false;
                    return;
                }
            }

            this.Success ^= this._Invert;
        }
    }
}
