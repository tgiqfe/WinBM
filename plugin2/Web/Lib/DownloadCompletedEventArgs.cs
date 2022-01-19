using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Lib
{
    class DownloadCompletedEventArgs
    {
        public bool Canceled { get; set; }
        public bool Success { get; set; }
        public bool Error { get; set; }
    }
}
