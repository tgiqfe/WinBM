using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinBM.PowerShell.Lib.TestWinBMYaml
{
    internal class IllegalParamCollection : List<IllegalParam>
    {
        public void AddIllegalValue(YamlNode node)
        {
            this.Add(new IllegalParam()
            {
                IllegalType = IllegalType.Value,
                Line = node.Line,
                Message = node.Key + ": " + node.Value,
            });
        }

        public void AddIllegalKey(YamlNode node)
        {
            this.Add(new IllegalParam()
            {
                IllegalType = IllegalType.Key,
                Line = node.Line,
                Message = node.Key + ": " + node.Value,
            });
        }

        public void AddNothingDll(YamlNode node)
        {
            //  未実装
        }
    }
}
