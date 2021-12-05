using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using WinBM.Recipe;

namespace WinBM.PowerShell.Lib.TestWinBMYaml
{
    internal class WinBMYaml
    {
        public string FilePath { get; set; }
        public int PageIndex { get; set; }
        public string Content { get; set; }

        public YamlKind Kind { get; set; }
        public YamlMetadata Metadata { get; set; }
        public List<YamlConfig> Config { get; set; }
        public List<YamlOutput> Output { get; set; }
        public List<YamlRequire> Require { get; set; }
        public List<YamlWork> Work { get; set; }

        public WinBMYaml() { }
        public WinBMYaml(string filePath, int pageIndex, string content)
        {
            this.FilePath = filePath;
            this.PageIndex = pageIndex;
            this.Content = content;
        }

        public bool TestDeserialize()
        {
            try
            {
                using (var sr = new StringReader(this.Content))
                {
                    _ = WinBM.Recipe.Page.Deserialize(sr);
                }
                return true;
            }
            catch { }
            return false;
        }

        public void TestParameter()
        {
            LoadFromContent();
            ViewIllegals();
        }

        private void LoadFromContent()
        {
            if (Content.Trim() != "")
            {
                this.Kind = YamlKind.Create(Content);
                this.Metadata = YamlMetadata.Create(Content);
                this.Config = YamlConfig.Create(Content);
                this.Output = YamlOutput.Create(Content);
                this.Require = YamlRequire.Create(Content);
                this.Work = YamlWork.Create(Content);
            }
        }

        private void ViewIllegals()
        {
            Kind.Illegals?.ForEach(y => y.View());
            Metadata.Illegals?.ForEach(y => y.View());
            Config.ForEach(x => x.Illegals?.ForEach(y => y.View()));
            Output.ForEach(x => x.Illegals?.ForEach(y => y.View()));
            Require.ForEach(x => x.Illegals?.ForEach(y => y.View()));
            Work.ForEach(x => x.Illegals?.ForEach(y => y.View()));
        }
    }
}

