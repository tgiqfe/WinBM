﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using Microsoft.Win32;
using IO.Lib;

namespace IO.Work.Registry
{
    internal class Load : TaskJob
    {
        [TaskParameter(Mandatory = true, ResolvEnv = true)]
        [Keys("path", "registrypath", "targetpath", "key", "registrykey", "targetkey", "regkey", "target")]
        protected string _Path { get; set; }

        [TaskParameter(Mandatory = true, ResolvEnv = true)]
        [Keys("filepath", "file", "outputpath", "outputfile", "output")]
        protected string _FilePath { get; set; }

        public override void MainProcess()
        {
            //  ハイブファイルが存在しない場合
            if (!System.IO.File.Exists(_FilePath))
            {
                Manager.WriteLog(LogLevel.Error, "Target file is Missing. \"{0}\"", _FilePath);
                return;
            }

            this.Success = IO.Lib.RegistryHive.LoadHive2(_Path, _FilePath);
        }
    }
}
