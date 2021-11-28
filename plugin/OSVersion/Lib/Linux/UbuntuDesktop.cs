using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSVersion.Lib.Linux
{
    internal class UbuntuDesktop
    {
        public static OSInfo Create1804()
        {
            return new OSInfo()
            {
                Name = "Ubuntu Desktop",
                OSFamily = OSFamily.Linux,
                VersionName = "18.04",
                Serial = 1804,
                Alias = new string[] { "Bionic Beaver", "BionicBeaver", "Bionic", "Beaver" },
                Version = "1804",
                Distribution = Distribution.Ubuntu,
                ReleaseDate = new DateTime(2018, 4, 26),
                EndSupportDate = new DateTime(2023, 4, 30),     //  2023年4月までとの記載なので、30日にセット
                IsServer = false,
                IsEmbedded = false,
            };
        }

        public static OSInfo Create1810()
        {
            return new OSInfo()
            {
                Name = "Ubuntu Desktop",
                OSFamily = OSFamily.Linux,
                VersionName = "18.10",
                Serial = 1810,
                Alias = new string[] { "Cosmic Cuttlefish", "CosmicCuttlefish", "Cosmic", "Cuttlefish" },
                Version = "1810",
                Distribution = Distribution.Ubuntu,
                ReleaseDate = new DateTime(2018, 10, 18),
                EndSupportDate = new DateTime(2019, 7, 31),     //  2020年7月までとの記載なので、31日にセット
                IsServer = false,
                IsEmbedded = false,
            };
        }

        public static OSInfo Create1904()
        {
            return new OSInfo()
            {
                Name = "Ubuntu Desktop",
                OSFamily = OSFamily.Linux,
                VersionName = "19.04",
                Serial = 1904,
                Alias = new string[] { "Disco Dingo", "DiscoDingo", "Disco", "Dingo" },
                Version = "1904",
                Distribution = Distribution.Ubuntu,
                ReleaseDate = new DateTime(2010, 4, 18),
                EndSupportDate = new DateTime(2020, 1, 31),     //  2020年1月までとの記載なので、31日にセット
                IsServer = false,
                IsEmbedded = false,
            };
        }

        public static OSInfo Create1910()
        {
            return new OSInfo()
            {
                Name = "Ubuntu Desktop",
                OSFamily = OSFamily.Linux,
                VersionName = "19.10",
                Serial = 1910,
                Alias = new string[] { "Eoan Ermine", "EoanErmine", "Eoan", "Ermine" },
                Version = "1910",
                Distribution = Distribution.Ubuntu,
                ReleaseDate = new DateTime(2019, 10, 17),
                EndSupportDate = new DateTime(2020, 7, 31),     //  2020年7月までとの記載なので、31日にセット
                IsServer = false,
                IsEmbedded = false,
            };
        }


        public static OSInfo Create2004()
        {
            return new OSInfo()
            {
                Name = "Ubuntu Desktop",
                OSFamily = OSFamily.Linux,
                VersionName = "20.04",
                Serial = 2004,
                Alias = new string[] { "Focal Fossa", "FocalFossa", "Focal", "Fossa" },
                Version = "20.04",
                Distribution = Distribution.Ubuntu,
                ReleaseDate = new DateTime(2020, 4, 23),
                EndSupportDate = new DateTime(2025, 4, 30),     //  2025年4月までとの記載なので、30日にセット
                IsServer = false,
                IsEmbedded = false,
            };
        }

        public static OSInfo Create2010()
        {
            return new OSInfo()
            {
                Name = "Ubuntu Desktop",
                OSFamily = OSFamily.Linux,
                VersionName = "20.10",
                Serial = 2010,
                Alias = new string[] { "Groovy Gorilla", "GroovyGorilla", "Groovy", "Gorilla" },
                Version = "20.10",
                Distribution = Distribution.Ubuntu,
                ReleaseDate = new DateTime(2020, 10, 22),
                EndSupportDate = new DateTime(2021, 7, 31),     //  2021年7月までとの記載なので、31日にセット
                IsServer = false,
                IsEmbedded = false,
            };
        }

        public static OSInfo Create2104()
        {
            return new OSInfo()
            {
                Name = "Ubuntu Desktop",
                OSFamily = OSFamily.Linux,
                VersionName = "2104",
                Serial = 2104,
                Alias = new string[] { "Hirsute Hippo", "HirsuteHippo", "Hirsute", "Hippo" },
                Version = "21.04",
                Distribution = Distribution.Ubuntu,
                ReleaseDate = new DateTime(2021, 4, 22),
                EndSupportDate = new DateTime(2022, 1, 31),     //  2022年1月までとの記載なので、31日にセット
                IsServer = false,
                IsEmbedded = false,
            };
        }
    }
}
