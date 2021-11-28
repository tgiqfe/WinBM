using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSVersion.Lib.Linux
{
    internal class MinMax
    {
        public static OSInfo CreateMinimum()
        {
            return new OSInfo()
            {
                Name = "Linux (MinimumVer)",
                OSFamily = OSFamily.Linux,
                VersionName = "LinuxMinimum",
                Serial = int.MinValue + 3,
                Alias = new string[] { },
                Version = int.MinValue.ToString(),
                Distribution = Distribution.None,
                ReleaseDate = DateTime.MinValue,
                EndSupportDate = DateTime.MaxValue,
                IsServer = false,
                IsEmbedded = false
            };
        }

        public static OSInfo CreateMaximum()
        {
            return new OSInfo()
            {
                Name = "Linux (MaximumVer)",
                OSFamily = OSFamily.Linux,
                VersionName = "LinuxMaximum",
                Serial = int.MaxValue - 3,
                Alias = new string[] { },
                Version = int.MaxValue.ToString(),
                Distribution = Distribution.None,
                ReleaseDate = DateTime.MinValue,
                EndSupportDate = DateTime.MaxValue,
                IsServer = false,
                IsEmbedded = false
            };
        }
    }
}
