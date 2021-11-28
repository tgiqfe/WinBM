using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSVersion.Lib.Windows
{
    internal class WindowsServer
    {
        public static OSInfo Create2000(Edition? edition)
        {
            //  エディション
            //  Professional, Server, Advanced Server, Datacenter Server
            return new OSInfo()
            {
                Name = "Windows Server 2000",
                OSFamily = OSFamily.Windows,
                VersionName = "2000",
                Serial = 50,
                Alias = new string[] { "NT 5.0" },
                Version = "5.0.2195",
                Edition = edition,
                ReleaseDate = new DateTime(2000, 2, 18),
                EndSupportDate = new DateTime(2010, 7, 13),
                IsServer = true,
                IsEmbedded = false
            };
        }

        public static OSInfo Create2003(Edition? edition)
        {
            //  エディション
            //  Web, Standard, Enterprise, Datacenter, Compute Cluster, Small Business, Storage Server
            return new OSInfo()
            {
                Name = "Windows Server 2003",
                OSFamily = OSFamily.Windows,
                VersionName = "2003",
                Serial = 52,
                Alias = new string[] { "NT 5.2" },
                Version = "5.2.3790",
                Edition = edition,
                ReleaseDate = new DateTime(2003, 6, 25),
                EndSupportDate = new DateTime(2015, 7, 14),
                IsServer = true,
                IsEmbedded = false
            };
        }
        public static OSInfo Create2003R2(Edition? edition)
        {
            //  エディション
            //  Web, Standard, Enterprise, Datacenter, Compute Cluster, Small Business, Storage Server
            return new OSInfo()
            {
                Name = "Windows Server 2003R2",
                OSFamily = OSFamily.Windows,
                VersionName = "2003R2",
                Serial = 53,
                Alias = new string[] { "NT 5.2", "2003 R2" },
                Version = "5.2.3790",
                Edition = edition,
                ReleaseDate = new DateTime(2005, 12, 6),
                EndSupportDate = new DateTime(2015, 7, 14),
                IsServer = true,
                IsEmbedded = false
            };
        }

        public static OSInfo Create2008(Edition? edition)
        {
            //  エディション
            //  Standard, Enterprise, Datacenter, Web, Storage Server, Small Business, Foundation
            return new OSInfo()
            {
                Name = "Windows Server 2008",
                OSFamily = OSFamily.Windows,
                VersionName = "6.0",
                Serial = 60,
                Alias = new string[] { "NT 6.0", "2008", "2008SP1", "2008SP2", "2008 SP1", "2008 SP2" },
                Version = "6.0.6002",
                Edition = edition,
                ReleaseDate = new DateTime(2008, 2, 5),
                EndSupportDate = new DateTime(2020, 1, 14),
                IsServer = true,
                IsEmbedded = false
            };
        }

        public static OSInfo Create2008R2(Edition? edition)
        {
            //  エディション
            //  Web, Foundation, Standard, Enterprise, Datacenter
            return new OSInfo()
            {
                Name = "Windows Server 2008R2",
                OSFamily = OSFamily.Windows,
                VersionName = "2008R2",
                Serial = 61,
                Alias = new string[] { "NT 6.1", "2008 R2", "2008 R2 SP1", "2008R2SP1" },
                Version = "6.1.7601",
                Edition = edition,
                ReleaseDate = new DateTime(2009, 9, 1),
                EndSupportDate = new DateTime(2020, 1, 14),
                IsServer = true,
                IsEmbedded = false
            };
        }

        public static OSInfo Create2012(Edition? edition)
        {
            //  エディション
            //  Foundation, Essentials, Standard, Datacenter
            return new OSInfo()
            {
                Name = "Windows Server 2012",
                OSFamily = OSFamily.Windows,
                VersionName = "2012",
                Serial = 62,
                Alias = new string[] { "NT 6.2" },
                Version = "6.2.9200",
                Edition = edition,
                ReleaseDate = new DateTime(2012, 9, 5),
                EndSupportDate = new DateTime(2023, 10, 10),
                IsServer = true,
                IsEmbedded = false
            };
        }

        public static OSInfo Create2012R2(Edition? edition)
        {
            //  エディション
            //  Essentials, Standard, Datacenter
            return new OSInfo()
            {
                Name = "Windows Server 2012R2",
                OSFamily = OSFamily.Windows,
                VersionName = "2012R2",
                Serial = 63,
                Alias = new string[] { "NT 6.3" },
                Version = "6.3.9600",
                Edition = edition,
                ReleaseDate = new DateTime(2013, 10, 17),
                EndSupportDate = new DateTime(2023, 10, 10),
                IsServer = true,
                IsEmbedded = false
            };
        }

        public static OSInfo Create2016(Edition? edition)
        {
            //  エディション
            //  Essentials, Standard, Datacenter
            return new OSInfo()
            {
                Name = "Windows Server 2016",
                OSFamily = OSFamily.Windows,
                VersionName = "2016",
                Serial = 1607,
                Alias = new string[] { "NT 10.0.14393" },
                Version = "10.0.14393",
                Edition = edition,
                ReleaseDate = new DateTime(2016, 9, 26),
                EndSupportDate = new DateTime(2027, 1, 12),
                IsServer = true,
                IsEmbedded = false
            };
        }

        public static OSInfo Create2019(Edition? edition)
        {
            //  エディション
            //  Essentials, Standard, Datacenter
            return new OSInfo()
            {
                Name = "Windows Server 2019",
                OSFamily = OSFamily.Windows,
                VersionName = "2019",
                Serial = 1809,
                Alias = new string[] { "NT 10.0.17763" },
                Version = "10.0.17763",
                Edition = edition,
                ReleaseDate = new DateTime(2018, 9, 2),
                EndSupportDate = new DateTime(2029, 1, 9),
                IsServer = true,
                IsEmbedded = false
            };
        }
    }
}
