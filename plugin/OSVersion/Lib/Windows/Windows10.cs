using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSVersion.Lib.Windows
{
    internal class Windows10
    {
        public static OSInfo Create1507(Edition? edition)
        {
            return new OSInfo()
            {
                Name = "Windows 10",
                OSFamily = OSFamily.Windows,
                VersionName = "1507",
                Serial = 1507,
                Alias = new string[] { "Released in July 2015", "ReleasedinJuly2015", "Threshold 1", "Threshold1" },
                Version = "10.0.10240",
                Edition = edition,
                ReleaseDate = new DateTime(2015, 7, 29),
                EndSupportDate = edition switch
                {
                    Edition.Home => new DateTime(2015, 7, 29),
                    Edition.Pro => new DateTime(2015, 7, 29),
                    Edition.Enterprise => new DateTime(2015, 7, 29),
                    Edition.Education => new DateTime(2015, 7, 29),
                    Edition.ProEducation => null,
                    Edition.EnterpriseLTSB => new DateTime(2025, 10, 14),
                    Edition.EnterpriseLTSC => null,
                    _ => DateTime.MinValue,
                },
                IsServer = false,
                IsEmbedded = false
            };
        }

        public static OSInfo Create1511(Edition? edition)
        {
            return new OSInfo()
            {
                Name = "Windows 10",
                OSFamily = OSFamily.Windows,
                VersionName = "1511",
                Serial = 1511,
                Alias = new string[] { "November Update", "NovemberUpdate", "Threshold 2", "Threshold2" },
                Version = "10.0.10586",
                Edition = edition,
                ReleaseDate = new DateTime(2015, 11, 10),
                EndSupportDate = edition switch
                {
                    Edition.Home => new DateTime(2017, 10, 10),
                    Edition.Pro => new DateTime(2017, 10, 10),
                    Edition.Enterprise => new DateTime(2018, 4, 10),
                    Edition.Education => new DateTime(2018, 4, 10),
                    Edition.ProEducation => null,
                    Edition.EnterpriseLTSB => null,
                    Edition.EnterpriseLTSC => null,
                    _ => null
                },
                IsServer = false,
                IsEmbedded = false,
            };
        }

        public static OSInfo Create1607(Edition? edition)
        {
            return new OSInfo()
            {
                Name = "Windows 10",
                OSFamily = OSFamily.Windows,
                VersionName = "1607",
                Serial = 1607,
                Alias = new string[] { "Anniversary Update", "AnniversaryUpdate", "Redstone 1", "Redstone1" },
                Version = "10.0.14393",
                Edition = edition,
                ReleaseDate = new DateTime(2016, 8, 2),
                EndSupportDate = edition switch
                {
                    Edition.Home => new DateTime(2018, 4, 10),
                    Edition.Pro => new DateTime(2018, 4, 10),
                    Edition.Enterprise => new DateTime(2019, 4, 9),
                    Edition.Education => new DateTime(2019, 4, 9),
                    Edition.ProEducation => new DateTime(2019, 4, 9),
                    Edition.EnterpriseLTSB => null,
                    Edition.EnterpriseLTSC => new DateTime(2026, 10, 13),
                    _ => null
                },
                IsServer = false,
                IsEmbedded = false
            };
        }

        public static OSInfo Create1703(Edition? edition)
        {
            return new OSInfo()
            {
                Name = "Windows 10",
                OSFamily = OSFamily.Windows,
                VersionName = "1703",
                Serial = 1703,
                Alias = new string[] { "Creators Update", "CreatorsUpdate", "Redstone 2", "Redstone2" },
                Version = "10.0.15063",
                Edition = edition,
                ReleaseDate = new DateTime(2017, 4, 5),
                EndSupportDate = edition switch
                {
                    Edition.Home => new DateTime(2018, 10, 9),
                    Edition.Pro => new DateTime(2018, 10, 9),
                    Edition.Enterprise => new DateTime(2019, 10, 8),
                    Edition.Education => new DateTime(2019, 10, 8),
                    Edition.ProEducation => new DateTime(2019, 10, 8),
                    Edition.EnterpriseLTSB => null,
                    Edition.EnterpriseLTSC => null,
                    _ => null
                },
                IsServer = false,
                IsEmbedded = false
            };
        }

        public static OSInfo Create1709(Edition? edition)
        {
            return new OSInfo()
            {
                Name = "Windows 10",
                OSFamily = OSFamily.Windows,
                VersionName = "1709",
                Serial = 1709,
                Alias = new string[] { "Fall Creators Update", "FallCreatorsUpdate", "Redstone 3", "Redstone3" },
                Version = "10.0.16299",
                Edition = edition,
                ReleaseDate = new DateTime(2017, 10, 17),
                EndSupportDate = edition switch
                {
                    Edition.Home => new DateTime(2019, 4, 9),
                    Edition.Pro => new DateTime(2019, 4, 9),
                    Edition.Enterprise => new DateTime(2020, 10, 13),
                    Edition.Education => new DateTime(2020, 10, 13),
                    Edition.ProEducation => new DateTime(2020, 10, 13),
                    Edition.EnterpriseLTSB => null,
                    Edition.EnterpriseLTSC => null,
                    _ => null
                },
                IsServer = false,
                IsEmbedded = false
            };
        }

        public static OSInfo Create1803(Edition? edition)
        {
            return new OSInfo()
            {
                Name = "Windows 10",
                OSFamily = OSFamily.Windows,
                VersionName = "1803",
                Serial = 1803,
                Alias = new string[] { "April 2018 Update", "April2018Update", "Redstone 4", "Redstone4" },
                Version = "10.0.17134",
                Edition = edition,
                ReleaseDate = new DateTime(2018, 4, 30),
                EndSupportDate = edition switch
                {
                    Edition.Home => new DateTime(2019, 11, 12),
                    Edition.Pro => new DateTime(2019, 11, 12),
                    Edition.Enterprise => new DateTime(2021, 5, 11),
                    Edition.Education => new DateTime(2021, 5, 11),
                    Edition.ProEducation => new DateTime(2021, 5, 11),
                    Edition.EnterpriseLTSB => null,
                    Edition.EnterpriseLTSC => null,
                    _ => null
                },
                IsServer = false,
                IsEmbedded = false
            };
        }

        public static OSInfo Create1809(Edition? edition)
        {
            return new OSInfo()
            {
                Name = "Windows 10",
                OSFamily = OSFamily.Windows,
                VersionName = "1809",
                Serial = 1809,
                Alias = new string[] { "October 2018 Update", "October2018Update", "Redstone 5", "Redstone5" },
                Version = "10.0.17763",
                Edition = edition,
                ReleaseDate = new DateTime(2018, 11, 13),
                EndSupportDate = edition switch
                {
                    Edition.Home => new DateTime(2020, 11, 10),
                    Edition.Pro => new DateTime(2020, 11, 10),
                    Edition.Enterprise => new DateTime(2021, 5, 11),
                    Edition.Education => new DateTime(2021, 5, 11),
                    Edition.ProEducation => new DateTime(2021, 5, 11),
                    Edition.EnterpriseLTSB => null,
                    Edition.EnterpriseLTSC => new DateTime(2029, 1, 9),
                    _ => null
                },
                IsServer = false,
                IsEmbedded = false
            };
        }

        public static OSInfo Create1903(Edition? edition)
        {
            return new OSInfo()
            {
                Name = "Windows 10",
                OSFamily = OSFamily.Windows,
                VersionName = "1903",
                Serial = 1903,
                Alias = new string[] { "May 2019 Update", "May2019Update", "19H1" },
                Version = "10.0.18362",
                Edition = edition,
                ReleaseDate = new DateTime(2019, 5, 21),
                EndSupportDate = edition switch
                {
                    Edition.Home => new DateTime(2020, 12, 8),
                    Edition.Pro => new DateTime(2020, 12, 8),
                    Edition.Enterprise => new DateTime(2020, 12, 8),
                    Edition.Education => new DateTime(2020, 12, 8),
                    Edition.ProEducation => new DateTime(2020, 12, 8),
                    Edition.EnterpriseLTSB => null,
                    Edition.EnterpriseLTSC => null,
                    _ => null
                },
                IsServer = false,
                IsEmbedded = false
            };
        }

        public static OSInfo Create1909(Edition? edition)
        {
            return new OSInfo()
            {
                Name = "Windows 10",
                OSFamily = OSFamily.Windows,
                VersionName = "1909",
                Serial = 1909,
                Alias = new string[] { "November 2019 Update", "November2019Update", "19H2" },
                Version = "10.0.18636",
                Edition = edition,
                ReleaseDate = new DateTime(2019, 11, 12),
                EndSupportDate = edition switch
                {
                    Edition.Home => new DateTime(2021, 5, 11),
                    Edition.Pro => new DateTime(2021, 5, 11),
                    Edition.Enterprise => new DateTime(2021, 5, 11),
                    Edition.Education => new DateTime(2021, 5, 11),
                    Edition.ProEducation => new DateTime(2021, 5, 11),
                    Edition.EnterpriseLTSB => null,
                    Edition.EnterpriseLTSC => null,
                    _ => null
                },
                IsServer = false,
                IsEmbedded = false
            };
        }

        public static OSInfo Create2004(Edition? edition)
        {
            return new OSInfo()
            {
                Name = "Windows 10",
                OSFamily = OSFamily.Windows,
                VersionName = "2004",
                Serial = 2004,
                Alias = new string[] { "May 2020 Update", "May2020Update", "20H1" },
                Version = "10.0.19041",
                Edition = edition,
                ReleaseDate = new DateTime(2020, 5, 27),
                EndSupportDate = edition switch
                {
                    Edition.Home => new DateTime(2021, 12, 14),
                    Edition.Pro => new DateTime(2021, 12, 14),
                    Edition.Enterprise => new DateTime(2021, 12, 14),
                    Edition.Education => new DateTime(2021, 12, 14),
                    Edition.ProEducation => new DateTime(2021, 12, 14),
                    Edition.EnterpriseLTSB => null,
                    Edition.EnterpriseLTSC => null,
                    _ => null
                },
                IsServer = false,
                IsEmbedded = false
            };
        }

        public static OSInfo Create20H2(Edition? edition)
        {
            return new OSInfo()
            {
                Name = "Windows 10",
                OSFamily = OSFamily.Windows,
                VersionName = "20H2",
                Serial = 2009,
                Alias = new string[] { "October 2020 Update", "October2020Update" },
                Version = "10.0.19042",
                Edition = edition,
                ReleaseDate = new DateTime(2020, 10, 20),
                EndSupportDate = edition switch
                {
                    Edition.Home => new DateTime(2022, 5, 10),
                    Edition.Pro => new DateTime(2022, 5, 10),
                    Edition.Enterprise => new DateTime(2023, 5, 9),
                    Edition.Education => new DateTime(2023, 5, 9),
                    Edition.ProEducation => new DateTime(2023, 5, 9),
                    Edition.EnterpriseLTSB => null,
                    Edition.EnterpriseLTSC => null,
                    _ => null
                },
                IsServer = false,
                IsEmbedded = false
            };
        }

        public static OSInfo Create21H1(Edition? edition)
        {
            return new OSInfo()
            {
                Name = "Windows 10",
                OSFamily = OSFamily.Windows,
                VersionName = "21H1",
                Serial = 2103,
                Alias = new string[] { "May 2021 Update", "May2021Update" },
                Version = "10.0.19043",
                Edition = edition,
                ReleaseDate = new DateTime(2021, 5, 18),
                EndSupportDate = edition switch
                {
                    Edition.Home => new DateTime(2022, 12, 13),
                    Edition.Pro => new DateTime(2022, 12, 13),
                    Edition.Enterprise => new DateTime(2022, 12, 13),
                    Edition.Education => new DateTime(2022, 12, 13),
                    Edition.ProEducation => new DateTime(2022, 12, 13),
                    Edition.EnterpriseLTSB => null,
                    Edition.EnterpriseLTSC => null,
                    _ => null
                },
                IsServer = false,
                IsEmbedded = false
            };
        }

        public static OSInfo Create21H2(Edition? edition)
        {
            return new OSInfo()
            {
                Name = "Windows 10",
                OSFamily = OSFamily.Windows,
                VersionName = "21H2",
                Serial = 2110,
                Alias = new string[] { "November 2021 Update", "November2021Update" },
                Version = "10.0.19044",
                Edition = edition,
                ReleaseDate = new DateTime(2021, 11, 16),
                EndSupportDate = edition switch
                {
                    Edition.Home => new DateTime(2023, 5, 16),          //  仮(リリースから18カ月)
                    Edition.Pro => new DateTime(2023, 5, 16),           //  仮(リリースから18カ月)
                    Edition.Enterprise => new DateTime(2024, 11, 16),   //  仮(リリースから36カ月)
                    Edition.Education => new DateTime(2024, 11, 16),    //  仮(リリースから36カ月)
                    Edition.ProEducation => new DateTime(2024, 11, 16), //  仮(リリースから36カ月)
                    Edition.EnterpriseLTSB => null,
                    Edition.EnterpriseLTSC => null,
                    _ => null
                },
                IsServer = false,
                IsEmbedded = false
            };
        }
    }
}
