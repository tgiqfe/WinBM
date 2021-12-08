using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSVersion.Lib.Other
{
    internal class MinMax
    {
        public static OSInfo CreateMinimum()
        {
            return new OSInfo()
            {
                Name = "MinVersion",
                OSFamily = OSFamily.Any,
                VersionName = "MinimumVersion",
                Serial = int.MinValue,
                Alias = new string[] { "Minimum", "min", "minver" },
                Version = int.MinValue.ToString(),
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
                Name = "MaxVersion",
                OSFamily = OSFamily.Any,
                VersionName = "MaximumVersion",
                Serial = int.MaxValue,
                Alias = new string[] { "Maximum", "max", "maxver" },
                Version = int.MaxValue.ToString(),
                ReleaseDate = DateTime.MinValue,
                EndSupportDate = DateTime.MaxValue,
                IsServer = false,
                IsEmbedded = false
            };
        }
    }
}

