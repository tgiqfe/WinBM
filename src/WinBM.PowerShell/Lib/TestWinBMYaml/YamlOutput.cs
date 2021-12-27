using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WinBM.PowerShell.Lib.TestWinBMYaml
{
    internal class YamlOutput
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool? Skip { get; set; }
        public string Task { get; set; }
        public Dictionary<string, string> Param { get; set; }

        public IllegalParamCollection Illegals { get; set; }

        public static List<YamlOutput> Create(string content)
        {
            var resultList = new List<YamlOutput>();

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

            foreach (var collection in searchContent("output:", "spec:", LineType.OutputSpec))
            {
                var spec = new YamlOutput();
                foreach (YamlNode node in collection)
                {
                    switch (node.Key)
                    {
                        case "name":
                            spec.SetName(node);
                            break;
                        case "description":
                            spec.SetDescription(node);
                            break;
                        case "skip":
                            spec.SetSkip(node);
                            break;
                        case "task":
                            spec.SetTask(node);
                            break;
                        case "param":
                            spec.SetParam(node);
                            break;
                        default:
                            spec.Illegals ??= new IllegalParamCollection();
                            spec.Illegals.AddIllegalKey(node);
                            break;
                    }
                }
                resultList.Add(spec);
            }

            return resultList;
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

        public void SetTask(YamlNode node)
        {
            string task = node.Value;
            if (task.Contains("/") && task.Split('/').Length == 3 && !task.Contains("\n"))
            {
                this.Task = node.Value;
            }
            else
            {
                this.Illegals ??= new IllegalParamCollection();
                Illegals.AddIllegalValue(node);
            }
        }

        public void SetParam(YamlNode node)
        {
            using (var asr = new AdvancedStringReader(node.Value))
            {
                var parameters = YamlFunctions.GetNodeCollections(asr, LineType.OutputSpecParam);
                if (parameters == null || parameters.Count == 0)
                {
                    node.Value = "(Empty)";
                    this.Illegals ??= new IllegalParamCollection();
                    Illegals.AddIllegalValue(node);
                    return;
                }

                this.Param = parameters[0].ToDictionary();
            }
        }
    }
}
