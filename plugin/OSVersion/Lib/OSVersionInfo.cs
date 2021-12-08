using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSVersion.Lib.Windows;
using OSVersion.Lib.Linux;

namespace OSVersion.Lib
{
    internal class OSVersionInfo : Arithmetic
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
        public OSVersionInfo() { }

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
        public static bool TryParse(string s, OSFamily family, out OSVersionInfo result)
        {
            result = family switch
            {
                OSFamily.Windows =>
                    OSVersionInfo.GetWindows(s) ??
                    OSVersionInfo.GetWindowsServer(s),
                OSFamily.Mac => OSVersionInfo.GetMac(s),
                OSFamily.Linux => OSVersionInfo.GetLinux(s),
                _ => null,
            };
            return result is not null;
        }

        public static OSVersionInfo GetMinVersion()
        {
            return OSVersion.Lib.Other.MinMax.CreateMinimum();
        }

        public static OSVersionInfo GetMaxVersion()
        {
            return OSVersion.Lib.Other.MinMax.CreateMaximum();
        }

        #region GetCurrent method

        /// <summary>
        /// 現在実行しているコンピュータのOSVersionInfo
        /// </summary>
        /// <returns></returns>
        public static OSVersionInfo GetCurrent()
        {
            return GetCurrent(new OSVersionInfoCollection(loadDefault: true));
        }

        public static OSVersionInfo GetCurrent(OSVersionInfoCollection collection)
        {
            if (OperatingSystem.IsWindows())
            {
                return FindWindows.GetCurrent(collection);
            }
            else if (OperatingSystem.IsMacOS())
            {
                return FindMac.GetOSVersionInfo();
            }
            else if (OperatingSystem.IsLinux())
            {
                return FindLinux.GetOSVersionInfo();
            }
            return null;
        }

        #endregion
        #region GetWindows

        public static OSVersionInfo GetWindows(int versionSerial, OSVersionInfoCollection collection)
        {
            var windowsCollection = collection.Where(x => x.OSFamily == OSFamily.Windows).Where(x => !x.IsServer);
            OSVersionInfo result = windowsCollection.FirstOrDefault(x => x.Serial == versionSerial);
            if (result is not null)
            {
                result.Edition = null;
                result.EndSupportDate = null;
            }

            return result;
        }

        public static OSVersionInfo GetWindows(int versionSerial)
        {
            return GetWindows(versionSerial, new OSVersionInfoCollection(loadDefault: true));
        }

        public static OSVersionInfo GetWindows(string versionName, OSVersionInfoCollection collection)
        {
            if (int.TryParse(versionName, out int tempInt))
            {
                return GetWindows(tempInt, collection);
            }

            var windowsCollection = collection.Where(x => x.OSFamily == OSFamily.Windows).Where(x => !x.IsServer);
            OSVersionInfo result =
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

        public static OSVersionInfo GetWindows(string versionName)
        {
            return GetWindows(versionName, new OSVersionInfoCollection(loadDefault: true));
        }

        public static OSVersionInfo GetWindowsServer(int versionSerial, OSVersionInfoCollection collection)
        {
            var winSVCollection = collection.Where(x => x.OSFamily == OSFamily.Windows).Where(x => x.IsServer);
            OSVersionInfo result = winSVCollection.FirstOrDefault(x => x.Serial == versionSerial);
            if (result is not null)
            {
                result.Edition = null;
                result.EndSupportDate = null;
            }

            return result;
        }

        public static OSVersionInfo GetWindowsServer(int versionSerial)
        {
            return GetWindowsServer(versionSerial, new OSVersionInfoCollection(loadDefault: true));
        }

        public static OSVersionInfo GetWindowsServer(string versionName, OSVersionInfoCollection collection)
        {
            if (int.TryParse(versionName, out int tempInt))
            {
                return GetWindows(tempInt, collection);
            }

            var winSVCollection = collection.Where(x => x.OSFamily == OSFamily.Windows).Where(x => x.IsServer);
            OSVersionInfo result =
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

        public static OSVersionInfo GetWindowsServer(string versionName)
        {
            return GetWindowsServer(versionName, new OSVersionInfoCollection(loadDefault: true));
        }

        #endregion
        #region GetMac

        public static OSVersionInfo GetMac(int versionSerial, OSVersionInfoCollection collection)
        {
            var macCollection = collection.Where(x => x.OSFamily == OSFamily.Mac);
            OSVersionInfo result = macCollection.FirstOrDefault(x => x.Serial == versionSerial);
            return result;
        }

        public static OSVersionInfo GetMac(int versionSerial)
        {
            return GetMac(versionSerial, new OSVersionInfoCollection(loadDefault: true));
        }

        public static OSVersionInfo GetMac(string versionName, OSVersionInfoCollection collection)
        {
            if (int.TryParse(versionName, out int tempInt))
            {
                return GetMac(tempInt);
            }

            var macCollection = collection.Where(x => x.OSFamily == OSFamily.Mac);
            OSVersionInfo result =
                macCollection.FirstOrDefault(x => x.VersionName.Equals(versionName, StringComparison.OrdinalIgnoreCase)) ??
                macCollection.FirstOrDefault(x => x.Alias.Any(y => y.Equals(versionName, StringComparison.OrdinalIgnoreCase))) ??
                macCollection.FirstOrDefault(x => x.Version.Equals(versionName));
            return result;
        }

        public static OSVersionInfo GetMac(string versionName)
        {
            return GetMac(versionName, new OSVersionInfoCollection(loadDefault: true));
        }

        #endregion
        #region GetLinux

        public static OSVersionInfo GetLinux(int versionSerial, OSVersionInfoCollection collection)
        {
            var macCollection = collection.Where(x => x.OSFamily == OSFamily.Linux);
            OSVersionInfo result = macCollection.FirstOrDefault(x => x.Serial == versionSerial);
            return result;
        }

        public static OSVersionInfo GetLinux(int versionSerial)
        {
            return GetLinux(versionSerial, new OSVersionInfoCollection(loadDefault: true));
        }

        public static OSVersionInfo GetLinux(string versionName, OSVersionInfoCollection collection)
        {
            if (int.TryParse(versionName, out int tempInt))
            {
                return GetLinux(tempInt);
            }

            var linuxCollection = collection.Where(x => x.OSFamily == OSFamily.Linux);
            OSVersionInfo result =
                linuxCollection.FirstOrDefault(x => x.VersionName.Equals(versionName, StringComparison.OrdinalIgnoreCase)) ??
                linuxCollection.FirstOrDefault(x => x.Alias.Any(y => y.Equals(versionName, StringComparison.OrdinalIgnoreCase))) ??
                linuxCollection.FirstOrDefault(x => x.Version.Equals(versionName));
            return result;
        }

        public static OSVersionInfo GetLinux(string versionName)
        {
            return GetLinux(versionName, new OSVersionInfoCollection(loadDefault: true));
        }

        public static OSVersionInfo GetLinux(int versionSerial, Linux.Distribution distribution, OSVersionInfoCollection collection)
        {
            var linuxCollection = collection.Where(x => x.OSFamily == OSFamily.Linux).Where(x => x.Distribution == distribution);
            OSVersionInfo result = linuxCollection.FirstOrDefault(x => x.Serial == versionSerial);
            return result;
        }

        public static OSVersionInfo GetLinux(int versionSerial, Linux.Distribution distribution)
        {
            return GetLinux(versionSerial, distribution, new OSVersionInfoCollection(loadDefault: true));
        }

        public static OSVersionInfo GetLinux(string versionName, Linux.Distribution distribution, OSVersionInfoCollection collection)
        {
            if (int.TryParse(versionName, out int tempInt))
            {
                return GetLinux(tempInt);
            }

            var linuxCollection = collection.Where(x => x.OSFamily == OSFamily.Linux).Where(x => x.Distribution == distribution);
            OSVersionInfo result =
                linuxCollection.FirstOrDefault(x => x.VersionName.Equals(versionName, StringComparison.OrdinalIgnoreCase)) ??
                linuxCollection.FirstOrDefault(x => x.Alias.Any(y => y.Equals(versionName, StringComparison.OrdinalIgnoreCase))) ??
                linuxCollection.FirstOrDefault(x => x.Version.Equals(versionName));
            return result;
        }

        public static OSVersionInfo GetLinux(string versionName, Linux.Distribution distribution)
        {
            return GetLinux(versionName, distribution, new OSVersionInfoCollection(loadDefault: true));
        }

        #endregion
    }
}
