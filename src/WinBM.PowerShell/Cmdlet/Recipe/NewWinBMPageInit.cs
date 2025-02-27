﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using WinBM.Recipe;
using System.Collections;
using WinBM.PowerShell.Lib;

namespace WinBM.PowerShell.Cmdlet.Recipe
{
    [Cmdlet(VerbsCommon.New, "WinBMPageInit")]
    public class NewWinBMPageInit : PSCmdlet
    {
        [Parameter]
        public string Name { get; set; }

        [Parameter]
        public string Description { get; set; }

        [Parameter]
        public SwitchParameter Skip { get; set; }

        [Parameter]
        public string Task { get; set; }

        [Parameter]
        public Hashtable Param { get; set; }

        protected override void ProcessRecord()
        {
            var spec = new SpecInit()
            {
                Name = this.Name,
                Description = this.Description,
                Skip = this.Skip,
                Task = this.Task,
                Param = this.Param.ToDictionary(),
            };
            WriteObject(spec);
        }
    }
}
