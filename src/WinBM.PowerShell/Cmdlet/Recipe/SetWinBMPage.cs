using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using WinBM.Recipe;
using System.IO;

namespace WinBM.PowerShell.Cmdlet
{
    [Cmdlet(VerbsCommon.Set, "WinBMPage")]
    public class SetWinBMPage : PSCmdlet
    {
        [Parameter]
        public WinBM.Recipe.Page.EnumKind? Kind { get; set; }

        [Parameter]
        public Metadata Metadata { get; set; }

        [Parameter]
        public PageConfig Config { get; set; }

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
            var page = this.Page ?? new WinBM.Recipe.Page();

            if (this.Kind != null)
            {
                page.Kind = (WinBM.Recipe.Page.EnumKind)this.Kind;
            }
            if (this.Metadata != null)
            {
                page.Metadata = this.Metadata;
            }

            switch (this.Kind)
            {
                case WinBM.Recipe.Page.EnumKind.Config:
                    page.Config = this.Config;
                    page.Output = null;
                    page.Job = null;
                    break;
                case WinBM.Recipe.Page.EnumKind.Output:
                    page.Output = new PageOutput();
                    if (this.Output != null && this.Output.Length > 0)
                    {
                        page.Output.Spec = this.Output;
                    }
                    page.Config = null;
                    page.Job = null;
                    break;
                case WinBM.Recipe.Page.EnumKind.Job:
                    page.Job = new PageJob();
                    if (this.Require != null && this.Require.Length > 0)
                    {
                        page.Job.Require = this.Require;
                    }
                    if (this.Work != null && this.Work.Length > 0)
                    {
                        page.Job.Work = this.Work;
                    }
                    page.Config = null;
                    page.Output = null;
                    break;
            }

            WriteObject(page);
        }
    }
}
