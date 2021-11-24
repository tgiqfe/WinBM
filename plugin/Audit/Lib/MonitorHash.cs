using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Win32;
using IO.Lib;

namespace Audit.Lib
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class MonitorHash
    {
        #region Check method

        public static bool WatchFileMD5Hash(
            WatchPath watch, Dictionary<string, string> dictionary, int serial, bool? isMonitor, string path)
        {
            if ((!isMonitor ?? true) && watch.MD5Hash == null) { return false; }

            bool ret = false;
            if (watch.MD5Hash == null)
            {
                ret = true;
                watch.MD5Hash = GetFileMD5Hash(path);
            }
            else
            {
                string pathType = "file";
                string checkTarget = "MD5Hash";

                string ret_string = GetFileMD5Hash(path);
                ret = ret_string != watch.MD5Hash;
                dictionary[$"{pathType}_{checkTarget}_{serial}"] = ret ?
                    $"{watch.MD5Hash} -> {ret_string}" :
                    ret_string;

                watch.MD5Hash = ret_string;
            }
            return ret;
        }

        public static bool WatchFileMD5Hash(
            WatchPath watch, Dictionary<string, string> dictionary, int serial, string path)
        {
            bool ret = false;
            if (watch.IsMD5Hash ?? false)
            {
                if (File.Exists(path))
                {
                    string ret_string = GetFileMD5Hash(path);
                    ret = ret_string != watch.MD5Hash;
                    if (watch.MD5Hash != null)
                    {
                        string pathType = "file";
                        string checkTarget = "MD5Hash";
                        dictionary[$"{pathType}_{checkTarget}_{serial}"] = ret ?
                            $"{watch.MD5Hash} -> {ret_string}" :
                            ret_string;
                    }
                    watch.MD5Hash = ret_string;
                }
                else
                {
                    watch.MD5Hash = null;
                }
            }
            return ret;
        }

        public static bool WatchFileSHA256Hash(
            WatchPath watch, Dictionary<string, string> dictionary, int serial, bool? isMonitor, string path)
        {
            if ((!isMonitor ?? true) && watch.SHA256Hash == null) { return false; }

            bool ret = false;
            if (watch.SHA256Hash == null)
            {
                ret = true;
                watch.SHA256Hash = GetFileSHA256Hash(path);
            }
            else
            {
                string pathType = "file";
                string checkTarget = "SHA256Hash";

                string ret_string = GetFileSHA256Hash(path);
                ret = ret_string != watch.SHA256Hash;
                dictionary[$"{pathType}_{checkTarget}_{serial}"] = ret ?
                    $"{watch.SHA256Hash} -> {ret_string}" :
                    ret_string;

                watch.SHA256Hash = ret_string;
            }
            return ret;
        }

        public static bool WatchFileSHA256Hash(
            WatchPath watch, Dictionary<string, string> dictionary, int serial, string path)
        {
            bool ret = false;
            if (watch.IsSHA256Hash ?? false)
            {
                if (File.Exists(path))
                {
                    string ret_string = GetFileSHA256Hash(path);
                    ret = ret_string != watch.SHA256Hash;
                    if (watch.SHA256Hash != null)
                    {
                        string pathType = "file";
                        string checkTarget = "SHA256Hash";
                        dictionary[$"{pathType}_{checkTarget}_{serial}"] = ret ?
                            $"{watch.SHA256Hash} -> {ret_string}" :
                            ret_string;
                    }
                    watch.SHA256Hash = ret_string;
                }
                else
                {
                    watch.SHA256Hash = null;
                }
            }
            return ret;
        }

        public static bool WatchFileSHA512Hash(
            WatchPath watch, Dictionary<string, string> dictionary, int serial, bool? isMonitor, string path)
        {
            if ((!isMonitor ?? true) && watch.SHA512Hash == null) { return false; }

            bool ret = false;
            if (watch.SHA512Hash == null)
            {
                ret |= true;
                watch.SHA512Hash = GetFileSHA512Hash(path);
            }
            else
            {
                string pathType = "file";
                string checkTarget = "SHA512Hash";

                string ret_string = GetFileSHA512Hash(path);
                ret = ret_string != watch.SHA512Hash;
                dictionary[$"{pathType}_{checkTarget}_{serial}"] = ret ?
                    $"{watch.SHA512Hash} -> {ret_string}" :
                    ret_string;

                watch.SHA512Hash = ret_string;
            }
            return ret;
        }

        public static bool WatchFileSHA512Hash(
            WatchPath watch, Dictionary<string, string> dictionary, int serial, string path)
        {
            bool ret = false;
            if (watch.IsSHA512Hash ?? false)
            {
                if (File.Exists(path))
                {
                    string ret_string = GetFileSHA512Hash(path);
                    ret = ret_string != watch.SHA512Hash;
                    if (watch.SHA512Hash != null)
                    {
                        string pathType = "file";
                        string checkTarget = "SHA512Hash";
                        dictionary[$"{pathType}_{checkTarget}_{serial}"] = ret ?
                            $"{watch.SHA512Hash} -> {ret_string}" :
                            ret_string;
                    }
                    watch.SHA512Hash = ret_string;
                }
                else
                {
                    watch.SHA512Hash = null;
                }
            }
            return ret;
        }

        public static bool WatchRegistryValueMD5Hash(
            WatchPath watch, Dictionary<string, string> dictionary, int serial, bool? isMonitor, RegistryKey regKey, string name)
        {
            if ((!isMonitor ?? true) && watch.MD5Hash == null) { return false; }

            bool ret = false;
            if (watch.MD5Hash == null)
            {
                ret = true;
                watch.MD5Hash = GetRegistryValueMD5Hash(regKey, name);
            }
            else
            {
                string pathType = "registry";
                string checkTarget = "MD5Hash";

                string ret_string = GetRegistryValueMD5Hash(regKey, name);
                ret = ret_string != watch.MD5Hash;
                dictionary[$"{pathType}_{checkTarget}_{serial}"] = ret ?
                    $"{watch.MD5Hash} -> {ret_string}" :
                    ret_string;

                watch.MD5Hash = ret_string;
            }
            return ret;
        }

        public static bool WatchRegistryValueMD5Hash(
            WatchPath watch, Dictionary<string, string> dictionary, int serial, RegistryKey regKey, string name)
        {
            bool ret = false;
            if (watch.IsMD5Hash ?? false)
            {
                if (regKey != null && regKey.GetValueNames().Any(x => x.Equals(name, StringComparison.OrdinalIgnoreCase)))
                {
                    string ret_string = GetRegistryValueMD5Hash(regKey, name);
                    ret = ret_string != watch.MD5Hash;
                    if(watch.MD5Hash != null)
                    {
                        string pathType = "registry";
                        string checkTarget = "MD5Hash";
                        dictionary[$"{pathType}_{checkTarget}_{serial}"] = ret ?
                            $"{watch.MD5Hash} -> {ret_string}" :
                            ret_string;
                    }
                    watch.MD5Hash = ret_string;
                }
                else
                {
                    watch.MD5Hash = null;
                }
            }
            return ret;
        }

        public static bool WatchRegistryValueSHA256Hash(
            WatchPath watch, Dictionary<string, string> dictionary, int serial, bool? isMonitor, RegistryKey regKey, string name)
        {
            if ((!isMonitor ?? true) && watch.SHA256Hash == null) { return false; }

            bool ret = false;
            if (watch.SHA256Hash == null)
            {
                ret = true;
                watch.SHA256Hash = GetRegistryValueSHA256Hash(regKey, name);
            }
            else
            {
                string pathType = "registry";
                string checkTarget = "SHA256Hash";

                string ret_string = GetRegistryValueSHA256Hash(regKey, name);
                ret = ret_string != watch.SHA256Hash;
                dictionary[$"{pathType}_{checkTarget}_{serial}"] = ret ?
                    $"{watch.SHA256Hash} -> {ret_string}" :
                    ret_string;

                watch.SHA256Hash = ret_string;
            }
            return ret;
        }

        public static bool WatchRegistryValueSHA256Hash(
            WatchPath watch, Dictionary<string, string> dictionary, int serial, RegistryKey regKey, string name)
        {
            bool ret = false;
            if (watch.IsSHA256Hash ?? false)
            {
                if (regKey != null && regKey.GetValueNames().Any(x => x.Equals(name, StringComparison.OrdinalIgnoreCase)))
                {
                    string ret_string = GetRegistryValueSHA256Hash(regKey, name);
                    ret = ret_string != watch.SHA256Hash;
                    if (watch.SHA256Hash != null)
                    {
                        string pathType = "registry";
                        string checkTarget = "SHA256Hash";
                        dictionary[$"{pathType}_{checkTarget}_{serial}"] = ret ?
                            $"{watch.SHA256Hash} -> {ret_string}" :
                            ret_string;
                    }
                    watch.SHA256Hash = ret_string;
                }
                else
                {
                    watch.SHA256Hash = null;
                }
            }
            return ret;
        }

        public static bool WatchRegistryValueSHA512Hash(
            WatchPath watch, Dictionary<string, string> dictionary, int serial, bool? isMonitor, RegistryKey regKey, string name)
        {
            if ((!isMonitor ?? true) && watch.SHA512Hash == null) { return false; }

            bool ret = false;
            if (watch.SHA512Hash == null)
            {
                ret = true;
                watch.SHA512Hash = GetRegistryValueSHA512Hash(regKey, name);
            }
            else
            {
                string pathType = "registry";
                string checkTarget = "SHA512Hash";

                string ret_string = GetRegistryValueSHA512Hash(regKey, name);
                ret = ret_string != watch.SHA512Hash;
                dictionary[$"{pathType}_{checkTarget}_{serial}"] = ret ?
                    $"{watch.SHA512Hash} -> {ret_string}" :
                    ret_string;

                watch.SHA512Hash = ret_string;
            }
            return ret;
        }

        public static bool WatchRegistryValueSHA512Hash(
            WatchPath watch, Dictionary<string, string> dictionary, int serial, RegistryKey regKey, string name)
        {
            bool ret = false;
            if (watch.IsSHA512Hash ?? false)
            {
                if (regKey != null && regKey.GetValueNames().Any(x => x.Equals(name, StringComparison.OrdinalIgnoreCase)))
                {
                    string ret_string = GetRegistryValueSHA512Hash(regKey, name);
                    ret = ret_string != watch.SHA512Hash;
                    if (watch.SHA512Hash != null)
                    {
                        string pathType = "registry";
                        string checkTarget = "SHA512Hash";
                        dictionary[$"{pathType}_{checkTarget}_{serial}"] = ret ?
                            $"{watch.SHA512Hash} -> {ret_string}" :
                            ret_string;
                    }
                    watch.SHA512Hash = ret_string;
                }
                else
                {
                    watch.SHA512Hash = null;
                }
            }
            return ret;
        }

        #endregion
        #region Get method

        public static string GetFileMD5Hash(string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var hashAlg = MD5.Create();
                string text = BitConverter.ToString(hashAlg.ComputeHash(fs)).Replace("-", "");
                hashAlg.Clear();
                return text;
            }
        }

        public static string GetFileSHA256Hash(string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var hashAlg = SHA256.Create();
                string text = BitConverter.ToString(hashAlg.ComputeHash(fs)).Replace("-", "");
                hashAlg.Clear();
                return text;
            }
        }

        public static string GetFileSHA512Hash(string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var hashAlg = SHA512.Create();
                string text = BitConverter.ToString(hashAlg.ComputeHash(fs)).Replace("-", "");
                hashAlg.Clear();
                return text;
            }
        }

        public static string GetRegistryValueMD5Hash(RegistryKey regKey, string name)
        {
            byte[] bytes = RegistryControl.RegistryValueToBytes(regKey, name, null, true);

            var hashAlg = MD5.Create();
            string text = BitConverter.ToString(hashAlg.ComputeHash(bytes)).Replace("-", "");
            hashAlg.Clear();
            return text;
        }

        public static string GetRegistryValueSHA256Hash(RegistryKey regKey, string name)
        {
            byte[] bytes = RegistryControl.RegistryValueToBytes(regKey, name, null, true);

            var hashAlg = MD5.Create();
            string text = BitConverter.ToString(hashAlg.ComputeHash(bytes)).Replace("-", "");
            hashAlg.Clear();
            return text;
        }

        public static string GetRegistryValueSHA512Hash(RegistryKey regKey, string name)
        {
            byte[] bytes = RegistryControl.RegistryValueToBytes(regKey, name, null, true);

            var hashAlg = MD5.Create();
            string text = BitConverter.ToString(hashAlg.ComputeHash(bytes)).Replace("-", "");
            hashAlg.Clear();
            return text;
        }

        #endregion
    }
}
