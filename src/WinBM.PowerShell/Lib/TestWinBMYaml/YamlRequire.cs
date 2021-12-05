using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WinBM.PowerShell.Lib.TestWinBMYaml
{
    internal class YamlRequire 
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool? Skip { get; set; }
        public string Task { get; set; }
        public Dictionary<string, string> Param { get; set; }
        public string Failed { get; set; }
        public bool? Progress { get; set; }

        public IllegalParamCollection Illegals { get; set; }

        public static List<YamlRequire> Create(string content)
        {
            var resultList = new List<YamlRequire>();

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

            foreach (var collection in searchContent("job:", "require:", LineType.JobRequire))
            {
                var spec = new YamlRequire();
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
                        case "failed":
                            spec.SetFailed(node);
                            break;
                        case "progress":
                            spec.SetProgress(node);
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
            this.Task = node.Value;
        }

        public void SetParam(YamlNode node)
        {
            using (var asr = new AdvancedStringReader(node.Value))
            {
                this.Param = YamlFunctions.GetNodeCollections(asr, LineType.JobrequireParam)[0].ToDictionary();
            }
        }

        public void SetFailed(YamlNode node)
        {
            if (Enum.TryParse(node.Value, out WinBM.Recipe.SpecJob.FailedAction failed))
            {
                this.Failed = failed.ToString();
            }
            else
            {
                this.Illegals ??= new IllegalParamCollection();
                Illegals.AddIllegalValue(node);
            }
        }

        public void SetProgress(YamlNode node)
        {
            if (bool.TryParse(node.Value, out bool progress))
            {
                this.Progress = progress;
            }
            else
            {
                this.Illegals ??= new IllegalParamCollection();
                Illegals.AddIllegalValue(node);
            }
        }
    }
}
