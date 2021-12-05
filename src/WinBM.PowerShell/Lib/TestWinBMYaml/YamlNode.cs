using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinBM.PowerShell.Lib.TestWinBMYaml
{
    internal class YamlNode
    {
        public int Line { get; set; }
        public LineType Type { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }

        public YamlNode(int line, LineType type)
        {
            this.Line = line;
            this.Type = type;
        }
    }
}
