using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WinBM.PowerShell.Manifest
{
    class Psm1
    {
        public static void Create(ProjectInfo info)
        {
            if (!File.Exists(info.DllFile)) { return; }
            using (StreamWriter sw = new StreamWriter(info.Psm1File, false, Encoding.UTF8))
            {
                sw.WriteLine();
            }
        }
    }
}
