using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using WinBM.Recipe;

namespace WinBM.PowerShell.Cmdlet.Recipe
{
    [Cmdlet(VerbsCommon.Add, "WinBMPage")]
    public class AddWinBMPage : PSCmdlet
    {
        [Parameter]
        public SpecInit[] Init { get; set; }

        [Parameter]
        public SpecConfig[] Config { get; set; }

        [Parameter]
        public SpecOutput[] Output { get; set; }

        [Parameter]
        public SpecJob[] Require { get; set; }

        [Parameter]
        public SpecJob[] Work { get; set; }

        [Parameter(ValueFromPipeline = true)]
        public WinBM.Recipe.Page Page { get; set; }

        protected override void ProcessRecord()
        {
            var page = this.Page;
            if (page != null)
            {
                switch (page.Kind)
                {
                    case WinBM.Recipe.Page.EnumKind.Init:
                        page.Init ??= new PageInit();
                        page.Init.Spec ??= new SpecInit[] { };
                        page.Init.Spec.Concat(this.Init);
                        break;
                    case WinBM.Recipe.Page.EnumKind.Config:
                        page.Config ??= new PageConfig();
                        page.Config.Spec ??= new SpecConfig[] { };
                        page.Config.Spec.Concat(this.Config);
                        break;
                    case WinBM.Recipe.Page.EnumKind.Output:
                        page.Output ??= new PageOutput();
                        page.Output.Spec ??= new SpecOutput[] { };
                        page.Output.Spec.Concat(this.Output);
                        break;
                    case WinBM.Recipe.Page.EnumKind.Job:
                        page.Job ??= new PageJob();
                        page.Job.Require ??= new SpecJob[] { };
                        page.Job.Require.Concat(this.Require);
                        page.Job.Work ??= new SpecJob[] { };
                        page.Job.Work.Concat(this.Work);
                        break;
                }
            }

            WriteObject(page);
        }
    }
}
