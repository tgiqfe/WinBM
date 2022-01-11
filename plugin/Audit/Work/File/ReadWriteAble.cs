using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using System.IO;
using IO.Lib;

namespace Audit.Work.File
{
    internal class ReadWriteAble : AuditTaskWork
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
            var dictionary = new Dictionary<string, string>();
            this.Success = true;
            int count = 0;

            foreach (string path in _Path)
            {
                ReadWriteAbleFileAction(path, dictionary, ++count);
            }

            AddAudit(dictionary, this._Invert);
        }

        private void ReadWriteAbleFileAction(string target, Dictionary<string, string> dictionary, int count)
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
                            dictionary[$"file_{count}_read/writable"] = target;
                        }
                    }
                    else
                    {
                        if (_Read ?? false)
                        {
                            //  Readのみtrue
                            using (FileStream fs = new FileStream(target, FileMode.Open, FileAccess.Read, FileShare.None))
                            {
                                dictionary[$"file_{count}_readable"] = target;
                            }
                        }
                        else if (_Write ?? false)
                        {
                            //  Writeのみtrue
                            using (FileStream fs = new FileStream(target, FileMode.Open, FileAccess.Write, FileShare.None))
                            {
                                dictionary[$"file_{count}_writable"] = target;
                            }
                        }
                    }
                }
                catch
                {
                    dictionary[$"file_{count}_not_able"] = target;
                    Success = false;
                }
            }
            else
            {
                dictionary[$"file_{count}_missing"] = target;
                Success = false;
                return;
            }
        }
    }
}
