using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinBM.PowerShell.Lib.TestWinBMYaml
{
    internal class YamlNodeCollection : List<YamlNode>
    {
        public void Add(int line, LineType type, string key, string val)
        {
            this.Add(new YamlNode(line, type)
            {
                Key = key,
                Value = val,
            });
        }

        public void AppendValue(string val)
        {
            this.Last().Value += val;
        }

        public Dictionary<string, string> ToDictionary()
        {
            var dictionary = new Dictionary<string, string>();
            this.ForEach(x => dictionary[x.Key] = x.Value);
            return dictionary;
        }
    }
}
