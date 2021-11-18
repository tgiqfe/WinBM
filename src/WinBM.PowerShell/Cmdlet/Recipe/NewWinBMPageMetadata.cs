using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using WinBM.Recipe;

namespace WinBM.PowerShell.Cmdlet.Recipe
{
    [Cmdlet(VerbsCommon.New, "WinBMPageMetadata")]
    public class NewWinBMPageMetadata : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        public string Name { get; set; }

        [Parameter]
        public string Description { get; set; }

        [Parameter]
        public SwitchParameter Skip { get; set; }

        [Parameter]
        public SwitchParameter Step { get; set; }

        [Parameter]
        public int? Priority { get; set; }

        protected override void ProcessRecord()
        {
            var metadata = new Metadata()
            {
                Name = this.Name,
                Description = this.Description,
                Skip = this.Skip,
                Step = this.Step,
                Priority = Priority,
            };
            WriteObject(metadata);
        }
    }
}
