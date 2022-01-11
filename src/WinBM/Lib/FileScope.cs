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

        public static void Clear()
        {
            if (FileScopeList != null)
            {
                Console.WriteLine("クリア");
                FileScopeList.Clear();
            }
        }

        public static string GetValue(string name)
        {
            string checkName = "%" + name + "%";
            return FileScopeList.FirstOrDefault(x => x.Name.Equals(checkName, StringComparison.OrdinalIgnoreCase))?.Value;
        }

        public static bool ContainsName(string name)
        {
            return FileScopeList.Any(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }
}
