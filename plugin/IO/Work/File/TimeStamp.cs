﻿using System;
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
        protected bool? _CreationTime { get; set; }

        [TaskParameter(MandatoryAny = 2)]
        [Keys("lastwritetime", "lastwrite", "lastwritedate", "modifytime", "modifydate", "modtime", "moddate")]
        protected bool? _LastWriteTime { get; set; }

        [TaskParameter(MandatoryAny = 3)]
        [Keys("lastaccesstime", "lastaccess", "lastaccessdate", "lastaccess")]
        protected bool? _LastAccessTime { get; set; }



    }
}
