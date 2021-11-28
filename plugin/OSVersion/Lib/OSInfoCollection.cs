using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using OSVersion.Lib.Windows;
using System.IO;

namespace OSVersion.Lib
{
    internal class OSInfoCollection : List<OSInfo>
    {
        #region Constructor

        public OSInfoCollection() { }

        public OSInfoCollection(bool loadDefault)
        {
            if (loadDefault)
            {
                this.LoadDefault();
            }
        }

        #endregion
        #region Load default

        public void LoadDefault()
        {
            //  Windows 10
            Add(Windows10.Create1507(null));
            Add(Windows10.Create1507(Edition.Home));
            Add(Windows10.Create1507(Edition.Pro));
            Add(Windows10.Create1507(Edition.Enterprise));
            Add(Windows10.Create1507(Edition.Education));
            Add(Windows10.Create1507(Edition.EnterpriseLTSB));

            Add(Windows10.Create1511(null));
            Add(Windows10.Create1511(Edition.Home));
            Add(Windows10.Create1511(Edition.Pro));
            Add(Windows10.Create1511(Edition.Enterprise));
            Add(Windows10.Create1511(Edition.Education));

            Add(Windows10.Create1607(null));
            Add(Windows10.Create1607(Edition.Home));
            Add(Windows10.Create1607(Edition.Pro));
            Add(Windows10.Create1607(Edition.Enterprise));
            Add(Windows10.Create1607(Edition.Education));
            Add(Windows10.Create1607(Edition.ProEducation));
            Add(Windows10.Create1607(Edition.EnterpriseLTSB));

            Add(Windows10.Create1703(null));
            Add(Windows10.Create1703(Edition.Home));
            Add(Windows10.Create1703(Edition.Pro));
            Add(Windows10.Create1703(Edition.Enterprise));
            Add(Windows10.Create1703(Edition.Education));
            Add(Windows10.Create1703(Edition.ProEducation));

            Add(Windows10.Create1709(null));
            Add(Windows10.Create1709(Edition.Home));
            Add(Windows10.Create1709(Edition.Pro));
            Add(Windows10.Create1709(Edition.Enterprise));
            Add(Windows10.Create1709(Edition.Education));
            Add(Windows10.Create1709(Edition.ProEducation));

            Add(Windows10.Create1803(null));
            Add(Windows10.Create1803(Edition.Home));
            Add(Windows10.Create1803(Edition.Pro));
            Add(Windows10.Create1803(Edition.Enterprise));
            Add(Windows10.Create1803(Edition.Education));
            Add(Windows10.Create1803(Edition.ProEducation));

            Add(Windows10.Create1809(null));
            Add(Windows10.Create1809(Edition.Home));
            Add(Windows10.Create1809(Edition.Pro));
            Add(Windows10.Create1809(Edition.Enterprise));
            Add(Windows10.Create1809(Edition.Education));
            Add(Windows10.Create1809(Edition.ProEducation));
            Add(Windows10.Create1809(Edition.EnterpriseLTSC));

            Add(Windows10.Create1903(null));
            Add(Windows10.Create1903(Edition.Home));
            Add(Windows10.Create1903(Edition.Pro));
            Add(Windows10.Create1903(Edition.Enterprise));
            Add(Windows10.Create1903(Edition.Education));
            Add(Windows10.Create1903(Edition.ProEducation));

            Add(Windows10.Create1909(null));
            Add(Windows10.Create1909(Edition.Home));
            Add(Windows10.Create1909(Edition.Pro));
            Add(Windows10.Create1909(Edition.Enterprise));
            Add(Windows10.Create1909(Edition.Education));
            Add(Windows10.Create1909(Edition.ProEducation));

            Add(Windows10.Create2004(null));
            Add(Windows10.Create2004(Edition.Home));
            Add(Windows10.Create2004(Edition.Pro));
            Add(Windows10.Create2004(Edition.Enterprise));
            Add(Windows10.Create2004(Edition.Education));
            Add(Windows10.Create2004(Edition.ProEducation));

            Add(Windows10.Create20H2(null));
            Add(Windows10.Create20H2(Edition.Home));
            Add(Windows10.Create20H2(Edition.Pro));
            Add(Windows10.Create20H2(Edition.Enterprise));
            Add(Windows10.Create20H2(Edition.Education));
            Add(Windows10.Create20H2(Edition.ProEducation));

            Add(Windows10.Create21H1(null));
            Add(Windows10.Create21H1(Edition.Home));
            Add(Windows10.Create21H1(Edition.Pro));
            Add(Windows10.Create21H1(Edition.Enterprise));
            Add(Windows10.Create21H1(Edition.Education));
            Add(Windows10.Create21H1(Edition.ProEducation));

            Add(Windows10.Create21H2(null));
            Add(Windows10.Create21H2(Edition.Home));
            Add(Windows10.Create21H2(Edition.Pro));
            Add(Windows10.Create21H2(Edition.Enterprise));
            Add(Windows10.Create21H2(Edition.Education));
            Add(Windows10.Create21H2(Edition.ProEducation));

            //  Windows 11
            Add(Windows11.Create21H2(null));
            Add(Windows11.Create21H2(Edition.Home));
            Add(Windows11.Create21H2(Edition.Pro));
            Add(Windows11.Create21H2(Edition.ProEducation));
            Add(Windows11.Create21H2(Edition.ProForWorkstations));
            Add(Windows11.Create21H2(Edition.Enterprise));
            Add(Windows11.Create21H2(Edition.Education));

            //  Windows Sever
            Add(WindowsServer.Create2000(null));
            Add(WindowsServer.Create2000(Edition.Professional));
            Add(WindowsServer.Create2000(Edition.Server));
            Add(WindowsServer.Create2000(Edition.AdvancedServer));
            Add(WindowsServer.Create2000(Edition.DatacenterServer));

            Add(WindowsServer.Create2003(null));
            Add(WindowsServer.Create2003(Edition.Web));
            Add(WindowsServer.Create2003(Edition.Standard));
            Add(WindowsServer.Create2003(Edition.Enterprise));
            Add(WindowsServer.Create2003(Edition.Datacenter));
            Add(WindowsServer.Create2003(Edition.ComputeCluster));
            Add(WindowsServer.Create2003(Edition.SmallBusiness));
            Add(WindowsServer.Create2003(Edition.StorageServer));

            Add(WindowsServer.Create2003R2(null));
            Add(WindowsServer.Create2003R2(Edition.Web));
            Add(WindowsServer.Create2003R2(Edition.Standard));
            Add(WindowsServer.Create2003R2(Edition.Enterprise));
            Add(WindowsServer.Create2003R2(Edition.Datacenter));
            Add(WindowsServer.Create2003R2(Edition.ComputeCluster));
            Add(WindowsServer.Create2003R2(Edition.SmallBusiness));
            Add(WindowsServer.Create2003R2(Edition.StorageServer));

            Add(WindowsServer.Create2008(null));
            Add(WindowsServer.Create2008(Edition.Standard));
            Add(WindowsServer.Create2008(Edition.Enterprise));
            Add(WindowsServer.Create2008(Edition.Datacenter));
            Add(WindowsServer.Create2008(Edition.Web));
            Add(WindowsServer.Create2008(Edition.StorageServer));
            Add(WindowsServer.Create2008(Edition.SmallBusiness));
            Add(WindowsServer.Create2008(Edition.Foundation));

            Add(WindowsServer.Create2008R2(null));
            Add(WindowsServer.Create2008R2(Edition.Web));
            Add(WindowsServer.Create2008R2(Edition.Foundation));
            Add(WindowsServer.Create2008R2(Edition.Standard));
            Add(WindowsServer.Create2008R2(Edition.Enterprise));
            Add(WindowsServer.Create2008R2(Edition.Datacenter));

            Add(WindowsServer.Create2012(null));
            Add(WindowsServer.Create2012(Edition.Foundation));
            Add(WindowsServer.Create2012(Edition.Essentials));
            Add(WindowsServer.Create2012(Edition.Standard));
            Add(WindowsServer.Create2012(Edition.Datacenter));

            Add(WindowsServer.Create2012R2(null));
            Add(WindowsServer.Create2012R2(Edition.Essentials));
            Add(WindowsServer.Create2012R2(Edition.Standard));
            Add(WindowsServer.Create2012R2(Edition.Datacenter));

            Add(WindowsServer.Create2016(null));
            Add(WindowsServer.Create2016(Edition.Essentials));
            Add(WindowsServer.Create2016(Edition.Standard));
            Add(WindowsServer.Create2016(Edition.Datacenter));

            Add(WindowsServer.Create2019(null));
            Add(WindowsServer.Create2019(Edition.Essentials));
            Add(WindowsServer.Create2019(Edition.Standard));
            Add(WindowsServer.Create2019(Edition.Datacenter));

        }

        #endregion
        #region Load/Save

        public static OSInfoCollection Load(string dbPath)
        {
            OSInfoCollection result = null;
            try
            {
                using (var sr = new StreamReader(dbPath, Encoding.UTF8))
                {
                    result = JsonSerializer.Deserialize<OSInfoCollection>(sr.ReadToEnd());
                }
            }
            catch { }

            if (result == null)
            {
                result = new OSInfoCollection(loadDefault: true);
                result.Save(dbPath);
            }

            return result;
        }

        public void Save(string dbPath)
        {
            using (var sw = new StreamWriter(dbPath, false, Encoding.UTF8))
            {
                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions()
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                });
                sw.WriteLine(json);
            }
        }

        #endregion
    }
}