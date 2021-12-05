using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WinBM.PowerShell.Lib.TestWinBMYaml
{
    internal class YamlKind
    {
        public string Kind { get; set; }

        public IllegalParamCollection Illegals { get; set; }

        public static YamlKind Create(string content)
        {
            var result = new YamlKind();

            YamlNodeCollection collection = new YamlNodeCollection();
            using (var asr = new AdvancedStringReader(content))
            {
                string readLine = "";
                while ((readLine = asr.ReadLine()) != null)
                {
                    if (readLine.StartsWith("kind:"))
                    {
                        collection.Add(
                            asr.Line,
                            LineType.Kind,
                            "kind",
                            readLine.Substring(readLine.IndexOf(":") + 1).Trim());
                        break;
                    }
                }

                result.SetKind(collection.First());
            }
            return result;
        }

        public void SetKind(YamlNode node)
        {
            if (Enum.TryParse(node.Value, out WinBM.Recipe.Page.EnumKind kind))
            {
                this.Kind = kind.ToString();
            }
            else
            {
                this.Illegals ??= new IllegalParamCollection();
                Illegals.AddIllegalValue(node);
            }
        }
    }
}
