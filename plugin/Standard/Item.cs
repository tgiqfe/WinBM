using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Standard
{
    internal class Item
    {
        private static readonly string _WorkDir = Environment.UserName == "SYSTEM" ?
            Path.Combine(Environment.GetEnvironmentVariable("ProgramData"), "WinBM") :
            Path.Combine(Path.GetTempPath(), "WinBM");

        public static string GetScriptLanguageDBFile()
        {
            return System.IO.Path.Combine(_WorkDir, "Lang", "ScriptLanguage.json");
        }
    }
}
