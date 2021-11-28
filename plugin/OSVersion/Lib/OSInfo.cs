using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSVersion.Lib.Windows;
using OSVersion.Lib.Linux;

namespace OSVersion.Lib
{
    internal class OSInfo : Arithmetic
    {
        #region Class Paramter

        /// <summary>
        /// OSの名前
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// OSファミリー (Windows/Mac/Linux)
        /// </summary>
        public OSFamily OSFamily { get; set; }

        /// <summary>
        /// OSのバージョンの名前。一番通りの良い名前。
        /// </summary>
        public string VersionName { get; set; }

        /// <summary>
        /// OSバージョン名のその他の名前。開発時のコードネームも含む
        /// </summary>
        public string[] Alias { get; set; }

        /// <summary>
        /// バージョン
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// エディション
        /// </summary>
        public Edition? Edition { get; set; }

        /// <summary>
        /// Linuxディストリビューション
        /// </summary>
        public Distribution? Distribution { get; set; }

        /// <summary>
        /// リリース日
        /// </summary>
        public DateTime ReleaseDate { get; set; }

        /// <summary>
        /// サポート終了日
        /// </summary>
        public DateTime? EndSupportDate { get; set; }

        /// <summary>
        /// サーバOSであるかどうか
        /// </summary>
        public bool IsServer { get; set; }

        /// <summary>
        /// 組み込み用OSであるかどうか
        /// </summary>
        public bool IsEmbedded { get; set; }

        #endregion

        /// <summary>
        /// Alismeticで判定時に使用するGroup
        /// </summary>
        public override string Group
        {
            get
            {
                return this.OSFamily == OSFamily.Linux ?
                    $"{OSFamily} {Distribution}" :
                    this.OSFamily.ToString();
            }
        }

        /// <summary>
        /// メジャーバージョン
        /// </summary>
        public string MajorVersion
        {
            get { return Version.Contains(".") ? Version.Split('.')[0] : Version; }
        }

        /// <summary>
        /// マイナーバージョン
        /// </summary>
        public string MinorVersion
        {
            get { return Version.Contains(".") && Version.Split('.').Length > 1 ? Version.Split('.')[1] : Version; }
        }

        /// <summary>
        /// ビルド番号
        /// </summary>
        public string BuildVersion
        {
            get { return Version.Contains(".") && Version.Split('.').Length > 2 ? Version.Split('.')[2] : Version; }
        }

        public string RevisionVersion
        {
            get { return Version.Contains(".") && Version.Split('.').Length > 3 ? Version.Split('.')[3] : Version; }
        }

        /// <summary>
        /// 引数無しコンストラクタ
        /// </summary>
        public OSInfo() { }

        /// <summary>
        /// 文字列化
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}{1} ver.{2}",
                Name,
                Edition == null || Edition == Windows.Edition.None ?
                    "" :
                    " " + Edition,
                VersionName);
        }

        /// <summary>
        /// OSバージョンで解決できるかどうかを判定
        /// </summary>
        /// <param name="s"></param>
        /// <param name="family"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryParse(string s, OSFamily family, out OSInfo result)
        {
            result = family switch
            {
                OSFamily.Windows =>
                    OSVersion.GetWindows(s) ??
                    OSVersion.GetWindowsServer(s),
                OSFamily.Mac => OSVersion.GetMac(s),
                OSFamily.Linux => OSVersion.GetLinux(s),
                _ => null,
            };
            return result is not null;
        }

        public static OSInfo GetMinVersion(OSFamily family)
        {
            return family switch
            {
                OSFamily.Windows => Windows.MinMax.CreateMinimum(),
                OSFamily.Mac => Mac.MinMax.CreateMinimum(),
                OSFamily.Linux => Linux.MinMax.CreateMinimum(),
                _ => null,
            };
        }

        public static OSInfo GetMaxVersion(OSFamily family)
        {
            return family switch
            {
                OSFamily.Windows => Windows.MinMax.CreateMaximum(),
                OSFamily.Mac => Mac.MinMax.CreateMaximum(),
                OSFamily.Linux => Linux.MinMax.CreateMaximum(),
                _ => null,
            };
        }

        #region GetCurrent method

        /// <summary>
        /// 現在実行しているコンピュータのOSInfo
        /// </summary>
        /// <returns></returns>
        public static OSInfo GetCurrent()
        {
            OSInfoCollection collection = new OSInfoCollection();
            collection.LoadDefault();
            return GetCurrent(collection);
        }

        public static OSInfo GetCurrent(OSInfoCollection collection)
        {
            if (OperatingSystem.IsWindows())
            {
                return FindWindows.GetCurrent(collection);
            }
            else if (OperatingSystem.IsMacOS())
            {
                return FindMac.GetOSInfo();
            }
            else if (OperatingSystem.IsLinux())
            {
                return FindLinux.GetOSInfo();
            }
            return null;
        }

        #endregion
        #region GetWindows

        public static OSInfo GetWindows(int versionSerial, OSInfoCollection collection)
        {
            var windowsCollection = collection.Where(x => x.OSFamily == OSFamily.Windows).Where(x => !x.IsServer);
            OSInfo result = windowsCollection.FirstOrDefault(x => x.Serial == versionSerial);
            if (result is not null)
            {
                result.Edition = null;
                result.EndSupportDate = null;
            }

            return result;
        }

        public static OSInfo GetWindows(int versionSerial)
        {
            OSInfoCollection collection = new OSInfoCollection();
            collection.LoadDefault();
            return GetWindows(versionSerial, collection);
        }

        public static OSInfo GetWindows(string versionName, OSInfoCollection collection)
        {
            if (int.TryParse(versionName, out int tempInt))
            {
                return GetWindows(tempInt, collection);
            }

            var windowsCollection = collection.Where(x => x.OSFamily == OSFamily.Windows).Where(x => !x.IsServer);
            OSInfo result =
                windowsCollection.FirstOrDefault(x => x.VersionName.Equals(versionName, StringComparison.OrdinalIgnoreCase)) ??
                windowsCollection.FirstOrDefault(x => x.Alias.Any(y => y.Equals(versionName, StringComparison.OrdinalIgnoreCase))) ??
                windowsCollection.FirstOrDefault(x => x.Version.Equals(versionName)) ??
                windowsCollection.FirstOrDefault(x => x.BuildVersion.Equals(versionName));
            if (result is not null)
            {
                result.Edition = null;
                result.EndSupportDate = null;
            }

            return result;
        }

        public static OSInfo GetWindows(string versionName)
        {
            OSInfoCollection collection = new OSInfoCollection();
            collection.LoadDefault();
            return GetWindows(versionName, collection);
        }

        public static OSInfo GetWindowsServer(int versionSerial, OSInfoCollection collection)
        {
            var winSVCollection = collection.Where(x => x.OSFamily == OSFamily.Windows).Where(x => x.IsServer);
            OSInfo result = winSVCollection.FirstOrDefault(x => x.Serial == versionSerial);
            if (result is not null)
            {
                result.Edition = null;
                result.EndSupportDate = null;
            }

            return result;
        }

        public static OSInfo GetWindowsServer(int versionSerial)
        {
            OSInfoCollection collection = new OSInfoCollection();
            collection.LoadDefault();
            return GetWindowsServer(versionSerial, collection);
        }

        public static OSInfo GetWindowsServer(string versionName, OSInfoCollection collection)
        {
            if (int.TryParse(versionName, out int tempInt))
            {
                return GetWindows(tempInt, collection);
            }

            var winSVCollection = collection.Where(x => x.OSFamily == OSFamily.Windows).Where(x => x.IsServer);
            OSInfo result =
                winSVCollection.FirstOrDefault(x => x.VersionName.Equals(versionName, StringComparison.OrdinalIgnoreCase)) ??
                winSVCollection.FirstOrDefault(x => x.Alias.Any(y => y.Equals(versionName, StringComparison.OrdinalIgnoreCase))) ??
                winSVCollection.FirstOrDefault(x => x.Version.Equals(versionName));
            if (result is not null)
            {
                result.Edition = null;
                result.EndSupportDate = null;
            }

            return result;
        }

        public static OSInfo GetWindowsServer(string versionName)
        {
            OSInfoCollection collection = new OSInfoCollection();
            collection.LoadDefault();
            return GetWindowsServer(versionName, collection);
        }

        #endregion
        #region GetMac

        public static OSInfo GetMac(int versionSerial, OSInfoCollection collection)
        {
            var macCollection = collection.Where(x => x.OSFamily == OSFamily.Mac);
            OSInfo result = macCollection.FirstOrDefault(x => x.Serial == versionSerial);
            return result;
        }

        public static OSInfo GetMac(int versionSerial)
        {
            OSInfoCollection collection = new OSInfoCollection();
            collection.LoadDefault();
            return GetMac(versionSerial, collection);
        }

        public static OSInfo GetMac(string versionName, OSInfoCollection collection)
        {
            if (int.TryParse(versionName, out int tempInt))
            {
                return GetMac(tempInt);
            }

            var macCollection = collection.Where(x => x.OSFamily == OSFamily.Mac);
            OSInfo result =
                macCollection.FirstOrDefault(x => x.VersionName.Equals(versionName, StringComparison.OrdinalIgnoreCase)) ??
                macCollection.FirstOrDefault(x => x.Alias.Any(y => y.Equals(versionName, StringComparison.OrdinalIgnoreCase))) ??
                macCollection.FirstOrDefault(x => x.Version.Equals(versionName));
            return result;
        }

        public static OSInfo GetMac(string versionName)
        {
            OSInfoCollection collection = new OSInfoCollection();
            collection.LoadDefault();
            return GetMac(versionName, collection);
        }

        #endregion
        #region GetLinux

        public static OSInfo GetLinux(int versionSerial, OSInfoCollection collection)
        {
            var macCollection = collection.Where(x => x.OSFamily == OSFamily.Linux);
            OSInfo result = macCollection.FirstOrDefault(x => x.Serial == versionSerial);
            return result;
        }

        public static OSInfo GetLinux(int versionSerial)
        {
            OSInfoCollection collection = new OSInfoCollection();
            collection.LoadDefault();
            return GetLinux(versionSerial, collection);
        }

        public static OSInfo GetLinux(string versionName, OSInfoCollection collection)
        {
            if (int.TryParse(versionName, out int tempInt))
            {
                return GetLinux(tempInt);
            }

            var linuxCollection = collection.Where(x => x.OSFamily == OSFamily.Linux);
            OSInfo result =
                linuxCollection.FirstOrDefault(x => x.VersionName.Equals(versionName, StringComparison.OrdinalIgnoreCase)) ??
                linuxCollection.FirstOrDefault(x => x.Alias.Any(y => y.Equals(versionName, StringComparison.OrdinalIgnoreCase))) ??
                linuxCollection.FirstOrDefault(x => x.Version.Equals(versionName));
            return result;
        }

        public static OSInfo GetLinux(string versionName)
        {
            OSInfoCollection collection = new OSInfoCollection();
            collection.LoadDefault();
            return GetLinux(versionName, collection);
        }

        public static OSInfo GetLinux(int versionSerial, Linux.Distribution distribution, OSInfoCollection collection)
        {
            var linuxCollection = collection.Where(x => x.OSFamily == OSFamily.Linux).Where(x => x.Distribution == distribution);
            OSInfo result = linuxCollection.FirstOrDefault(x => x.Serial == versionSerial);
            return result;
        }

        public static OSInfo GetLinux(int versionSerial, Linux.Distribution distribution)
        {
            OSInfoCollection collection = new OSInfoCollection();
            collection.LoadDefault();
            return GetLinux(versionSerial, distribution, collection);
        }

        public static OSInfo GetLinux(string versionName, Linux.Distribution distribution, OSInfoCollection collection)
        {
            if (int.TryParse(versionName, out int tempInt))
            {
                return GetLinux(tempInt);
            }

            var linuxCollection = collection.Where(x => x.OSFamily == OSFamily.Linux).Where(x => x.Distribution == distribution);
            OSInfo result =
                linuxCollection.FirstOrDefault(x => x.VersionName.Equals(versionName, StringComparison.OrdinalIgnoreCase)) ??
                linuxCollection.FirstOrDefault(x => x.Alias.Any(y => y.Equals(versionName, StringComparison.OrdinalIgnoreCase))) ??
                linuxCollection.FirstOrDefault(x => x.Version.Equals(versionName));
            return result;
        }

        public static OSInfo GetLinux(string versionName, Linux.Distribution distribution)
        {
            OSInfoCollection collection = new OSInfoCollection();
            collection.LoadDefault();
            return GetLinux(versionName, distribution, collection);
        }

        #endregion


    }
}
