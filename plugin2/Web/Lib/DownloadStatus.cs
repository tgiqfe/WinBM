using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Lib
{
    class DownloadStatus
    {
        public bool Canceled { get; set; }
        public bool Success { get; set; }
        public bool Error { get; set; }
        public Exception Exception { get; set; }

        public DownloadCompletedEventArgs GetDownloadCompletedEventArgs()
        {
            return new DownloadCompletedEventArgs()
            {
                Canceled = this.Canceled,
                Success = this.Success,
                Error = this.Error,
            };
        }
    }
}
