using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinBM.Lib
{
    public class FileScope
    {
        public static List<FileEnv> FileScopeList { get; set; }

        public static void Add(string path, string name, string val)
        {
            FileScopeList ??= new List<FileEnv>();
            FileScopeList.Add(new FileEnv()
            {
                Path = path,
                Name = name,
                Value = val
            });
        }
    }
}
