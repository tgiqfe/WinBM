using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuditMonitor.Arguments
{
    class ArgsParamAttribute : Attribute
    {
        public bool Mandatory { get; set; }
    }
}
