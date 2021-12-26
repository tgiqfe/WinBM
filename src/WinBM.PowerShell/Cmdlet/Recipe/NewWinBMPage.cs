using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using WinBM.Recipe;
using System.IO;

namespace WinBM.PowerShell.Cmdlet.Recipe
{
    [Cmdlet(VerbsCommon.New, "WinBMPage")]
    public class NewWinBMPage : PSCmdlet
    {
        [Parameter]
        public WinBM.Recipe.Page.EnumKind Kind { get; set; }

        [Parameter]
        public Metadata Metadata { get; set; }

        [Parameter]
        public SpecEnv[] Env { get; set; }

        [Parameter]
        public SpecConfig[] Config { get; set; }

        [Parameter]
        public SpecOutput[] Output { get; set; }

        [Parameter]
        public SpecJob[] Require { get; set; }

        [Parameter]
        public SpecJob[] Work { get; set; }

        protected override void ProcessRecord()
        {
            var page = new WinBM.Recipe.Page()
            {
                Kind = this.Kind,
                Metadata = this.Metadata,
            };
            switch (this.Kind)
            {
                case WinBM.Recipe.Page.EnumKind.Init:
                    page.Env = new PageEnv();
                    if (this.Env?.Length > 0)
                    {
                        page.Env.Spec = this.Env;
                    }
                    break;
                case WinBM.Recipe.Page.EnumKind.Config:
                    page.Config = new PageConfig();
                    if(this.Config?.Length > 0)
                    {
                        page.Config.Spec = this.Config;
                    }
                    break;
                case WinBM.Recipe.Page.EnumKind.Output:
                    page.Output = new PageOutput();
                    if (this.Output?.Length > 0)
                    {
                        page.Output.Spec = this.Output;
                    }
                    break;
                case WinBM.Recipe.Page.EnumKind.Job:
                    page.Job = new PageJob();
                    if (this.Require?.Length > 0)
                    {
                        page.Job.Require = this.Require;
                    }
                    if (this.Work?.Length > 0)
                    {
                        page.Job.Work = this.Work;
                    }
                    break;
            }
            WriteObject(page);
        }
    }
}
