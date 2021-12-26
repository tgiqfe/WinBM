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
                    if (readLine.Contains("#"))
                    {
                        readLine = YamlFunctions.RemoveComment(readLine);
                    }
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

            //  Kind指定したNodeが存在するかチェック
            using (var asr = new AdvancedStringReader(content))
            {
                string category = "";
                string[] specs = null;
                switch (result.Kind)
                {
                    case "Init":
                        category = "init:";
                        specs = new string[] { "spec:" };
                        break;
                    case "Config":
                        category = "config:";
                        specs = new string[] { "spec:" };
                        break;
                    case "Output":
                        category = "output:";
                        specs = new string[] { "spec:" };
                        break;
                    case "Job":
                        category = "job:";
                        specs = new string[] { "work:", "require:" };
                        break;
                }

                int specSuccessCount = 0;
                string readLine = "";
                bool inChild = false;
                while ((readLine = asr.ReadLine()) != null)
                {
                    if (readLine.Contains("#"))
                    {
                        readLine = YamlFunctions.RemoveComment(readLine);
                    }
                    if (readLine == category)
                    {
                        inChild = true;
                        continue;
                    }
                    if (inChild && specs.Any(x => x == readLine.Trim()))
                    {
                        specSuccessCount++;
                        inChild = false;
                        continue;
                    }
                    if (!readLine.StartsWith(" "))
                    {
                        if (!readLine.StartsWith("kind:") && readLine != "metadata:")
                        {
                            result.Illegals ??= new IllegalParamCollection();
                            result.Illegals.AddIllegalNode(asr.Line, "Illegal node: " + readLine);
                        }
                    }
                }
                if (specSuccessCount != specs.Length)
                {
                    result.Illegals ??= new IllegalParamCollection();
                    result.Illegals.AddIllegalNode(-1, "Insufficient node. [" + result.Kind + "]");
                }
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
