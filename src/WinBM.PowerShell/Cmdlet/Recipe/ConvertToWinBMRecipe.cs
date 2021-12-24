using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using WinBM.Recipe;
using System.IO;
using System.Collections.ObjectModel;

namespace WinBM.PowerShell.Cmdlet.Recipe
{
    [Cmdlet(VerbsData.ConvertTo, "WinBMRecipe")]
    public class ConvertToWinBMRecipe : PSCmdlet, IDynamicParameters
    {
        [Parameter(ValueFromPipeline = true)]
        public WinBM.Recipe.Page[] Page { get; set; }

        private const string TYPE_YAML = "Yaml";
        private const string TYPE_PSCOMMAND = "PSCommand";

        #region Dynamic Parameter

        private const string PARAM_NAME = "OutputType";
        private RuntimeDefinedParameterDictionary _dictionary;
        private string[] _outputTypes = null;

        public object GetDynamicParameters()
        {
            _dictionary = new RuntimeDefinedParameterDictionary();
            if (_outputTypes == null)
            {
                //  将来拡張用
                _outputTypes = new string[] { TYPE_YAML, TYPE_PSCOMMAND };
            }
            Collection<Attribute> attribute = new Collection<Attribute>()
            {
                new ParameterAttribute(),
                new AliasAttribute("Type"),
                new ValidateSetAttribute(_outputTypes),
            };
            var rdp = new RuntimeDefinedParameter(PARAM_NAME, typeof(string), attribute);
            _dictionary.Add(PARAM_NAME, rdp);

            return _dictionary;
        }

        #endregion

        protected override void ProcessRecord()
        {
            string outputType = _dictionary[PARAM_NAME].Value as string ?? "Yaml";

            switch (outputType)
            {
                case TYPE_YAML:
                    var sb = new StringBuilder();
                    foreach (WinBM.Recipe.Page page in this.Page)
                    {
                        sb.Append(page.Serialize());
                    }
                    WriteObject(sb.ToString());
                    break;
                case TYPE_PSCOMMAND:
                    string psCommand = CreatePSCommand(this.Page);
                    WriteObject(psCommand);
                    break;
            }

        }

        #region CreatePSCommand

        private string CreatePSCommand(WinBM.Recipe.Page[] pages)
        {
            var varPageSet = new VarPageSet();

            foreach (WinBM.Recipe.Page page in pages)
            {
                var varPage = new VarPage();
                varPage.Kind = page.Kind.ToString();
                varPage.Metadata = GetCommandMetadata(page.Metadata);

                switch (page.Kind)
                {
                    case WinBM.Recipe.Page.EnumKind.Config:
                        if (page.Config.Spec != null)
                        {
                            foreach (SpecConfig spec in page.Config.Spec)
                            {
                                varPage.Config.Add(GetCommandConfig(spec));
                            }
                        }
                        break;
                    case WinBM.Recipe.Page.EnumKind.Output:
                        if (page.Output.Spec != null)
                        {
                            foreach (SpecOutput spec in page.Output.Spec)
                            {
                                varPage.Output.Add(GetCommandOutput(spec));
                            }
                        }
                        break;
                    case WinBM.Recipe.Page.EnumKind.Job:
                        if (page.Job.Require != null)
                        {
                            foreach (SpecJob spec in page.Job.Require)
                            {
                                varPage.Require.Add(GetCommandJob(spec));
                            }
                        }
                        if (page.Job.Work != null)
                        {
                            foreach (SpecJob spec in page.Job.Work)
                            {
                                varPage.Work.Add(GetCommandJob(spec));
                            }
                        }
                        break;
                }
                varPageSet.VarList.Add(varPage);
            }

            return varPageSet.ToString();
        }

        private string GetCommandMetadata(Metadata metadata)
        {
            var sb = new StringBuilder();

            sb.Append("New-WinBMPageMetadata");
            if (!string.IsNullOrEmpty(metadata.Name))
            {
                sb.Append($" -Name \"{metadata.Name}\"");
            }
            if (!string.IsNullOrEmpty(metadata.Description))
            {
                sb.Append($" -Description \"{metadata.Description}\"");
            }
            if (metadata.Skip ?? false)
            {
                sb.Append(" -Skip");
            }
            if (metadata.Step ?? false)
            {
                sb.Append(" -Step");
            }
            if (metadata.Priority != null && metadata.Priority != 0)
            {
                sb.Append($" -Priority {metadata.Priority}");
            }

            return sb.ToString();
        }

        private string GetCommandConfig(SpecConfig spec)
        {
            var sb = new StringBuilder();

            sb.Append("New-WinBMPageConfig");
            if (!string.IsNullOrEmpty(spec.Name))
            {
                sb.Append($" -Name \"{spec.Name}\"");
            }
            if (!string.IsNullOrEmpty(spec.Description))
            {
                sb.Append($" -Description \"{spec.Description}\"");
            }
            if (!string.IsNullOrEmpty(spec.Task))
            {
                sb.Append($" -Task {spec.Task}");
            }
            if (spec.Skip ?? false)
            {
                sb.Append(" -Skip");
            }
            if (spec.Param?.Count > 0)
            {
                sb.Append(" -Param @{ ");
                foreach (KeyValuePair<string, string> pair in spec.Param)
                {
                    sb.Append($"\"{pair.Key}\" = \"{pair.Value}\"; ");
                }
                sb.Append("}");
            }

            return sb.ToString();
        }

        private string GetCommandOutput(SpecOutput spec)
        {
            var sb = new StringBuilder();
            sb.Append("New-WinBMPageOutput");
            if (!string.IsNullOrEmpty(spec.Name))
            {
                sb.Append($" -Name \"{spec.Name}\"");
            }
            if (!string.IsNullOrEmpty(spec.Description))
            {
                sb.Append($" -Description \"{spec.Description}\"");
            }
            if (!string.IsNullOrEmpty(spec.Task))
            {
                sb.Append($" -Task {spec.Task}");
            }
            if (spec.Skip ?? false)
            {
                sb.Append(" -Skip");
            }
            if (spec.Param?.Count > 0)
            {
                sb.Append(" -Param @{ ");
                foreach (KeyValuePair<string, string> pair in spec.Param)
                {
                    sb.Append($"\"{pair.Key}\" = \"{pair.Value}\"; ");
                }
                sb.Append("}");
            }

            return sb.ToString();
        }

        private string GetCommandJob(SpecJob spec)
        {
            var sb = new StringBuilder();

            sb.Append("New-WinBMPageJob");
            if (!string.IsNullOrEmpty(spec.Name))
            {
                sb.Append($" -Name \"{spec.Name}\"");
            }
            if (!string.IsNullOrEmpty(spec.Description))
            {
                sb.Append($" -Description \"{spec.Description}\"");
            }
            if (!string.IsNullOrEmpty(spec.Task))
            {
                sb.Append($" -Task {spec.Task}");
            }
            if (spec.Skip ?? false)
            {
                sb.Append(" -Skip");
            }
            if (spec.Param?.Count > 0)
            {
                sb.Append(" -Param @{ ");
                foreach (KeyValuePair<string, string> pair in spec.Param)
                {
                    sb.Append($"\"{pair.Key}\" = \"{pair.Value}\"; ");
                }
                sb.Append("}");
            }
            if (spec.Failed != null)
            {
                sb.Append($" -Failed {spec.Failed}");
            }
            if (spec.Progress ?? false)
            {
                sb.Append(" -Progress");
            }

            return sb.ToString();
        }

        #endregion
        #region CreatePSCommand class

        class VarPageSet
        {
            public List<VarPage> VarList { get; set; }

            public VarPageSet()
            {
                this.VarList = new List<VarPage>();
            }

            public override string ToString()
            {
                var sb = new StringBuilder();
                for (int i = 0; i < VarList.Count; i++)
                {
                    if (i > 0) { sb.AppendLine(); }
                    sb.AppendLine($"# Page {i}");

                    sb.Append(VarList[i].GetCommand(i));

                    sb.AppendLine(string.Format("page_{0} = New-WinBMPage -Kind {1}{2}{3}",
                        i,
                        VarList[i].Kind,
                        VarList[i].GetMetadataParameterString(i),
                        VarList[i].GetSpecParameterString(i)));
                }

                return sb.ToString();
            }
        }

        class VarPage
        {
            public string Kind { get; set; }
            public string Metadata { get; set; }
            public List<string> Config { get; set; }
            public List<string> Output { get; set; }
            public List<string> Require { get; set; }
            public List<string> Work { get; set; }

            public VarPage()
            {
                this.Config = new List<string>();
                this.Output = new List<string>();
                this.Require = new List<string>();
                this.Work = new List<string>();
            }

            public string GetCommand(int pageNum)
            {
                var sb = new StringBuilder();

                sb.AppendLine($"$metadata_{pageNum} = {Metadata}");

                switch (Kind)
                {
                    case "Config":
                        Config.Select((x, index) => $"$config_{pageNum}_{index} = {x}").
                            ToList().
                            ForEach(x => sb.AppendLine(x));
                        break;
                    case "Output":
                        Output.Select((x, index) => $"$output_{pageNum}_{index} = {x}").
                            ToList().
                            ForEach(x => sb.AppendLine(x));
                        break;
                    case "Job":
                        Require.Select((x, index) => $"$require_{pageNum}_{index} = {x}").
                            ToList().
                            ForEach(x => sb.AppendLine(x));
                        Work.Select((x, index) => $"$work_{pageNum}_{index} = {x}").
                            ToList().
                            ForEach(x => sb.AppendLine(x));
                        break;
                }

                return sb.ToString();
            }

            public string GetMetadataParameterString(int pageNum)
            {
                return $" -Metadata $metadata_{pageNum}";
            }

            public string GetSpecParameterString(int pageNum)
            {
                switch (Kind)
                {
                    case "Config":
                        return string.Format(" -Config @({0})",
                            string.Join(", ",
                                Config.Select((x, index) => $"$config_{pageNum}_{index}")));
                    case "Output":
                        return string.Format(" -Kind @({0})",
                            string.Join(", ",
                                Output.Select((x, index) => $"$output_{pageNum}_{index}")));
                    case "Job":
                        return string.Format(" -Require @({0}) -Work @({1})",
                            string.Join(", ",
                                Require.Select((x, index) => $"$require_{pageNum}_{index}")),
                            string.Join(", ",
                                Work.Select((x, index) => $"$work_{pageNum}_{index}")));
                }
                return null;
            }
        }

        #endregion
    }
}
