using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace WinBM.Recipe
{
    public class SpecBase
    {
        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlMember(Alias = "description")]
        public string Description { get; set; }

        [YamlMember(Alias = "skip")]
        public bool? Skip { get; set; }

        [YamlMember(Alias = "task")]
        public string Task { get; set; }

        [YamlMember(Alias = "param")]
        public Dictionary<string, string> Param { get; set; }
    }
}
