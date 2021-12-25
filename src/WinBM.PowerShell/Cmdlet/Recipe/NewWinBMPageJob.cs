using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Collections;
using WinBM.Recipe;
using WinBM.PowerShell.Lib;

namespace WinBM.PowerShell.Cmdlet.Recipe
{
    [Cmdlet(VerbsCommon.New, "WinBMPageJob")]
    public class NewWinBMPageJob : PSCmdlet
    {
        [Parameter]
        public string Name { get; set; }

        [Parameter]
        public string Description { get; set; }

        [Parameter]
        public SwitchParameter Skip { get; set; }

        [Parameter(Mandatory = true, Position = 0)]
        public string Task { get; set; }

        [Parameter]
        public Hashtable Param { get; set; }

        [Parameter]
        public WinBM.Recipe.SpecJob.FailedAction? Failed { get; set; }

        [Parameter]
        public SwitchParameter Progress { get; set; }

        protected override void ProcessRecord()
        {
            var spec = new SpecJob()
            {
                Name = this.Name,
                Description = this.Description,
                Skip = this.Skip,
                Task = this.Task,
                Param = this.Param.ToDictionary(),
                Failed = this.Failed,
                Progress = this.Progress,
            };

            WriteObject(spec);
        }
    }
}
