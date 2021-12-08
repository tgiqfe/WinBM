using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using Audit.Lib;
using System.IO;
using IO.Lib;

namespace Audit.Work.Registry
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class ChildCount : AuditTaskWork
    {
        /*
        [TaskParameter(Mandatory = true, ResolvEnv = true, Delimiter = ';')]
        [Keys("path", "directorypath", "folderpath", "dirpath", "target", "targetpath")]
        protected string[] _Path { get; set; }
        */

        [TaskParameter(MandatoryAny = 1, Unsigned = true)]
        [Keys("directorycount", "directories", "directoryquantity", "foldercount", "folders", "folderquantity")]
        protected int? _DirectoryCount { get; set; }

        [TaskParameter(MandatoryAny = 2, Unsigned = true)]
        [Keys("filecount", "files", "filequantity", "count", "quantity")]
        protected int? _FileCount { get; set; }

        [TaskParameter(MandatoryAny = 3)]
        [Keys("empty", "isempty", "null")]
        protected bool? _IsEmpty { get; set; }

        [TaskParameter]
        [Keys("invert", "not", "no", "none")]
        protected bool _Invert { get; set; }

        public override void MainProcess()
        {
            var dictionary = new Dictionary<string, string>();
            this.Success = true;
            //int count = 0;

            /*
            foreach (string path in _Path)
            {
                
            }
            */

            AddAudit(dictionary, this._Invert);
        }

        private void ChildCountRegistryKeyCheck()
        {
            
        }
    }
}
