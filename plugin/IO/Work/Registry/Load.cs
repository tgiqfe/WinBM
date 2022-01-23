using System;
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
        [TaskParameter(Mandatory = true, Resolv = true)]
        [Keys("path", "registrypath", "targetpath", "key", "registrykey", "targetkey", "regkey", "target")]
        protected string _Path { get; set; }

        [TaskParameter(Mandatory = true, Resolv = true)]
        [Keys("filepath", "file", "outputpath", "outputfile", "output")]
        protected string _FilePath { get; set; }

        public override void MainProcess()
        {
            //  PathがHKEY_USERSで始まらない場合、修正
            if (_Path.Contains("\\"))
            {
                if (!_Path.StartsWith("HKEY_USERS\\", StringComparison.OrdinalIgnoreCase))
                {
                    _Path = "HKEY_USERS\\" + _Path.Substring(_Path.LastIndexOf("\\") + 1);
                }
            }
            else
            {
                _Path = "HKEY_USERS\\" + _Path;
            }

            //  ハイブファイルが存在しない場合
            if (!System.IO.File.Exists(_FilePath))
            {
                Manager.WriteLog(LogLevel.Error, "Target file is Missing. \"{0}\"", _FilePath);
                return;
            }

            Manager.WriteLog(LogLevel.Info, "Load Hive. \"{0}\"", _Path);
            this.Success = IO.Lib.RegistryHive.LoadHive(_Path, _FilePath);
        }
    }
}
