using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSVersion.Lib.Mac
{
    internal class MacOS
    {
        public static OSVersionInfo Create1011()
        {
            return new OSVersionInfo()
            {
                Name = "OS X",
                OSFamily = OSFamily.Mac,
                VersionName = "10.11",
                Serial = 101100,
                Alias = new string[] { "OS X El Capitan", "El Capitan", "ElCapitan" },
                Version = "10.11",
                ReleaseDate = new DateTime(2016, 9, 20),
                IsServer = false,
                IsEmbedded = false,
            };
        }

        public static OSVersionInfo Create1012()
        {
            return new OSVersionInfo()
            {
                Name = "macOS",
                OSFamily = OSFamily.Mac,
                VersionName = "10.12",
                Serial = 101200,
                Alias = new string[] { "macOS Sierra", "Sierra" },
                Version = "10.12",
                ReleaseDate = new DateTime(2016, 9, 20),
                IsServer = false,
                IsEmbedded = false,
            };
        }

        public static OSVersionInfo Create1013()
        {
            return new OSVersionInfo()
            {
                Name = "macOS",
                OSFamily = OSFamily.Mac,
                VersionName = "10.13",
                Serial = 101300,
                Alias = new string[] { "macOS High Sierra", "High Sierra", "HighSierra" },
                Version = "10.13",
                ReleaseDate = new DateTime(2017, 9, 25),
                IsServer = false,
                IsEmbedded = false,
            };
        }

        public static OSVersionInfo Create1014()
        {
            return new OSVersionInfo()
            {
                Name = "macOS",
                OSFamily = OSFamily.Mac,
                VersionName = "10.14",
                Serial = 101400,
                Alias = new string[] { "macOS Mojave", "Mojave" },
                Version = "10.14",
                ReleaseDate = new DateTime(2018, 9, 25),
                IsServer = false,
                IsEmbedded = false,
            };
        }

        public static OSVersionInfo Create1015()
        {
            return new OSVersionInfo()
            {
                Name = "macOS",
                OSFamily = OSFamily.Mac,
                VersionName = "10.15",
                Serial = 101500,
                Alias = new string[] { "macOS Catalina", "Catalina" },
                Version = "10.15",
                ReleaseDate = new DateTime(2019, 10, 8),
                IsServer = false,
                IsEmbedded = false,
            };
        }

        public static OSVersionInfo Create11()
        {
            return new OSVersionInfo()
            {
                Name = "macOS",
                OSFamily = OSFamily.Mac,
                VersionName = "11",
                Serial = 110000,
                Alias = new string[] { "macOS Big Sur", "Big Sur", "BigSur" },
                Version = "11",
                ReleaseDate = new DateTime(2020, 11, 13),
                IsServer = false,
                IsEmbedded = false,
            };
        }

        public static OSVersionInfo Create12()
        {
            return new OSVersionInfo()
            {
                Name = "macOS",
                OSFamily = OSFamily.Mac,
                VersionName = "12",
                Serial = 120000,
                Alias = new string[] { "macOS Monterey", "Monterey" },
                Version = "12",
                ReleaseDate = new DateTime(2021, 10, 25),
                IsServer = false,
                IsEmbedded = false,
            };
        }
    }
}
