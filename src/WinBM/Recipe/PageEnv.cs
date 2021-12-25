using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace WinBM.Recipe
{
    public class PageEnv : PageBase
    {
        [YamlMember(Alias = "spec")]
        public SpecEnv[] Spec { get; set; }

        public override string ToString()
        {
            string[] nameArray = this.Spec?.Select(x => x.Name).ToArray();
            return string.Join(", ", nameArray);
        }

        public override void PostDeserialize(string pageName)
        {
            if (Spec != null)
            {
                int count = 0;
                foreach (SpecEnv spec in this.Spec)
                {
                    count++;
                    if (string.IsNullOrEmpty(spec.Name))
                    {
                        spec.Name = string.Format("{0}_Env{1}",
                            pageName,
                            count);
                    }
                    if (spec.Param?.Count > 0)
                    {
                        var dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        foreach (KeyValuePair<string, string> pair in spec.Param)
                        {
                            dictionary[pair.Key] = pair.Value;
                        }
                        spec.Param = dictionary;
                    }
                }
            }
        }
    }
}
