using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSVersion.Lib.Windows
{
    internal class Windows11
    {
        public static OSInfo Create21H2(Edition? edition)
        {
            return new OSInfo()
            {
                Name = "Windows 11",
                OSFamily = OSFamily.Windows,
                VersionName = "21H2",
                Serial = 2109,
                Alias = new string[] { "Released Version", "ReleasedVersion" },
                Version = "10.0.22000",
                Edition = edition,
                ReleaseDate = new DateTime(2021, 10, 5),
                EndSupportDate = edition switch
                {
                    Edition.Home => new DateTime(2023, 10, 10),
                    Edition.Pro => new DateTime(2023, 10, 10),
                    Edition.ProEducation => new DateTime(2023, 10, 10),
                    Edition.ProForWorkstations => new DateTime(2023, 10, 10),
                    Edition.Enterprise => new DateTime(2024, 10, 8),
                    Edition.Education => new DateTime(2024, 10, 8),
                    _ => null,
                },
                IsServer = false,
                IsEmbedded = false
            };
        }
    }
}
