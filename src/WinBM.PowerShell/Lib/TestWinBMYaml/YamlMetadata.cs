using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WinBM.PowerShell.Lib.TestWinBMYaml
{
    internal class YamlMetadata
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool? Skip { get; set; }
        public bool? Step { get; set; }
        public string Priority { get; set; }

        public IllegalParamCollection Illegals { get; set; }

        public static YamlMetadata Create(string content)
        {
            var result = new YamlMetadata();

            Func<string, string, LineType, List<YamlNodeCollection>> searchContent = (category, spec, type) =>
            {
                using (var asr = new AdvancedStringReader(content))
                {
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
                            if (string.IsNullOrEmpty(spec))
                            {
                                return YamlFunctions.GetNodeCollections(asr, type);
                            }
                            else
                            {
                                inChild = true;
                                continue;
                            }
                        }
                        if (inChild && readLine.Trim() == spec)
                        {
                            return YamlFunctions.GetNodeCollections(asr, type);
                        }
                    }
                }
                return new List<YamlNodeCollection>();
            };

            foreach (YamlNode node in searchContent("metadata:", null, LineType.Metadata)[0])
            {
                switch (node.Key)
                {
                    case "name":
                        result.SetName(node);
                        break;
                    case "description":
                        result.SetDescription(node);
                        break;
                    case "skip":
                        result.SetSkip(node);
                        break;
                    case "step":
                        result.SetStep(node);
                        break;
                    case "priority":
                        result.SetPriority(node);
                        break;
                    default:
                        result.Illegals ??= new IllegalParamCollection();
                        result.Illegals.AddIllegalKey(node);
                        break;
                }
            }
            return result;
        }

        public void SetName(YamlNode node)
        {
            this.Name = node.Value;
        }

        public void SetDescription(YamlNode node)
        {
            this.Description = node.Value;
        }

        public void SetSkip(YamlNode node)
        {
            if (bool.TryParse(node.Value, out bool skip))
            {
                this.Skip = skip;
            }
            else
            {
                this.Illegals ??= new IllegalParamCollection();
                Illegals.AddIllegalValue(node);
            }
        }

        public void SetStep(YamlNode node)
        {
            if (bool.TryParse(node.Value, out bool step))
            {
                this.Skip = step;
            }
            else
            {
                this.Illegals ??= new IllegalParamCollection();
                Illegals.AddIllegalValue(node);
            }
        }

        public void SetPriority(YamlNode node)
        {
            this.Priority = node.Value;
        }
    }
}
