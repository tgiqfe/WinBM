using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime;

namespace OSVersion.Lib
{
    internal class OSVersion
    {
        private static OSInfoCollection _collection = null;

        #region GetCurrent method

        /// <summary>
        /// Windows/Mac/Linuxを判定し、それぞれのFind～クラスでのOSInfoチェック
        /// </summary>
        /// <returns></returns>
        public static OSInfo GetCurrent()
        {
            _collection ??= OSInfoCollection.Load();

            if (OperatingSystem.IsWindows())
            {
                return FindWindows.GetCurrent(_collection);
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

        public static OSInfo GetWindows(int versionSerial)
        {
            _collection ??= OSInfoCollection.Load();

            var windowsCollection = _collection.Where(x => x.OSFamily == OSFamily.Windows).Where(x => !x.IsServer);
            OSInfo result = windowsCollection.FirstOrDefault(x => x.Serial == versionSerial);
            if (result is not null)
            {
                result.Edition = null;
                result.EndSupportDate = null;
            }

            return result;
        }

        public static OSInfo GetWindows(string versionName)
        {
            if (int.TryParse(versionName, out int tempInt))
            {
                return GetWindows(tempInt);
            }

            _collection ??= OSInfoCollection.Load();

            var windowsCollection = _collection.Where(x => x.OSFamily == OSFamily.Windows).Where(x => !x.IsServer);
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

        public static OSInfo GetWindowsServer(int versionSerial)
        {
            _collection ??= OSInfoCollection.Load();

            var winSVCollection = _collection.Where(x => x.OSFamily == OSFamily.Windows).Where(x => x.IsServer);
            OSInfo result = winSVCollection.FirstOrDefault(x => x.Serial == versionSerial);
            if (result is not null)
            {
                result.Edition = null;
                result.EndSupportDate = null;
            }

            return result;
        }

        public static OSInfo GetWindowsServer(string versionName)
        {
            if (int.TryParse(versionName, out int tempInt))
            {
                return GetWindows(tempInt);
            }

            _collection ??= OSInfoCollection.Load();

            var winSVCollection = _collection.Where(x => x.OSFamily == OSFamily.Windows).Where(x => x.IsServer);
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

        #endregion
        #region GetMac

        public static OSInfo GetMac(int versionSerial)
        {
            _collection ??= OSInfoCollection.Load();

            var macCollection = _collection.Where(x => x.OSFamily == OSFamily.Mac);
            OSInfo result = macCollection.FirstOrDefault(x => x.Serial == versionSerial);
            return result;
        }

        public static OSInfo GetMac(string versionName)
        {
            if(int.TryParse(versionName, out int tempInt))
            {
                return GetMac(tempInt);
            }

            _collection ??= OSInfoCollection.Load();

            var macCollection = _collection.Where(x => x.OSFamily == OSFamily.Mac);
            OSInfo result =
                macCollection.FirstOrDefault(x => x.VersionName.Equals(versionName, StringComparison.OrdinalIgnoreCase)) ??
                macCollection.FirstOrDefault(x => x.Alias.Any(y => y.Equals(versionName, StringComparison.OrdinalIgnoreCase))) ??
                macCollection.FirstOrDefault(x => x.Version.Equals(versionName));
            return result;
        }

        #endregion
        #region GetLinux

        public static OSInfo GetLinux(int versionSerial)
        {
            _collection ??= OSInfoCollection.Load();

            var macCollection = _collection.Where(x => x.OSFamily == OSFamily.Linux);
            OSInfo result = macCollection.FirstOrDefault(x => x.Serial == versionSerial);
            return result;
        }

        public static OSInfo GetLinux(string versionName)
        {
            if (int.TryParse(versionName, out int tempInt))
            {
                return GetLinux(tempInt);
            }

            _collection ??= OSInfoCollection.Load();

            var linuxCollection = _collection.Where(x => x.OSFamily == OSFamily.Linux);
            OSInfo result =
                linuxCollection.FirstOrDefault(x => x.VersionName.Equals(versionName, StringComparison.OrdinalIgnoreCase)) ??
                linuxCollection.FirstOrDefault(x => x.Alias.Any(y => y.Equals(versionName, StringComparison.OrdinalIgnoreCase))) ??
                linuxCollection.FirstOrDefault(x => x.Version.Equals(versionName));
            return result;
        }

        public static OSInfo GetLinux(int versionSerial, Linux.Distribution distribution)
        {
            _collection ??= OSInfoCollection.Load();

            var linuxCollection = _collection.Where(x => x.OSFamily == OSFamily.Linux).Where(x => x.Distribution == distribution);
            OSInfo result = linuxCollection.FirstOrDefault(x => x.Serial == versionSerial);
            return result;
        }

        public static OSInfo GetLinux(string versionName, Linux.Distribution distribution)
        {
            if (int.TryParse(versionName, out int tempInt))
            {
                return GetLinux(tempInt);
            }

            _collection ??= OSInfoCollection.Load();

            var linuxCollection = _collection.Where(x => x.OSFamily == OSFamily.Linux).Where(x => x.Distribution == distribution);
            OSInfo result =
                linuxCollection.FirstOrDefault(x => x.VersionName.Equals(versionName, StringComparison.OrdinalIgnoreCase)) ??
                linuxCollection.FirstOrDefault(x => x.Alias.Any(y => y.Equals(versionName, StringComparison.OrdinalIgnoreCase))) ??
                linuxCollection.FirstOrDefault(x => x.Version.Equals(versionName));
            return result;
        }

        #endregion
    }
}
