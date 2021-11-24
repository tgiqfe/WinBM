using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Principal;
using System.Security.AccessControl;
using Microsoft.Win32;
using IO.Lib;

namespace Audit.Lib
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class MonitorSecurity
    {
        #region Watch method

        public static bool WatchFileAccess(
            WatchPath watch, Dictionary<string, string> dictionary, int serial, bool? isMonitor, FileInfo info)
        {
            if ((!isMonitor ?? true) && watch.Access == null) { return false; }

            bool ret = false;
            if (watch.Access == null)
            {
                ret = true;
                watch.Access = AccessRuleSummary.FileToAccessString(info);
            }
            else
            {
                string pathType = "file";
                string checkTarget = "Access";

                string ret_string = AccessRuleSummary.FileToAccessString(info);
                ret = ret_string != watch.Access;
                dictionary[$"{pathType}_{checkTarget}_{serial}"] = ret ?
                    $"{watch.Access} -> {ret_string}" :
                    ret_string;

                watch.Access = ret_string;
            }
            return ret;
        }

        public static bool WatchFileAccess(
            WatchPath watch, Dictionary<string, string> dictionary, int serial, FileInfo info)
        {
            bool ret = false;
            if (watch.IsAccess ?? false)
            {
                if (info.Exists)
                {
                    string ret_string = AccessRuleSummary.FileToAccessString(info);
                    ret = ret_string != watch.Access;
                    if (watch.Access != null)
                    {
                        string pathType = "file";
                        string checkTarget = "Access";
                        dictionary[$"{pathType}_{checkTarget}_{serial}"] = ret ?
                            $"{watch.Access} -> {ret_string}" :
                            ret_string;
                    }
                    watch.Access = ret_string;
                }
                else
                {
                    watch.Access = null;
                }
            }
            return ret;
        }

        public static bool WatchFileOwner(
            WatchPath watch, Dictionary<string, string> dictionary, int serial, bool? isMonitor, FileInfo info)
        {
            if ((!isMonitor ?? true) && watch.Owner == null) { return false; }

            bool ret = false;
            if (watch.Owner == null)
            {
                ret = true;
                watch.Owner = GetFileOwner(info);
            }
            else
            {
                string pathType = "file";
                string checkTarget = "Owner";

                string ret_string = GetFileOwner(info);
                ret = ret_string != watch.Owner;
                dictionary[$"{pathType}_{checkTarget}_{serial}"] = ret ?
                    $"{watch.Owner} -> {ret_string}" :
                    ret_string;

                watch.Owner = ret_string;
            }
            return ret;
        }

        public static bool WatchFileOwner(
            WatchPath watch, Dictionary<string, string> dictionary, int serial, FileInfo info)
        {
            bool ret = false;
            if (watch.IsOwner ?? false)
            {
                if (info.Exists)
                {
                    string ret_string = GetFileOwner(info);
                    ret = ret_string != watch.Owner;
                    if (watch.Owner != null)
                    {
                        string pathType = "file";
                        string checkTarget = "Owner";
                        dictionary[$"{pathType}_{checkTarget}_{serial}"] = ret ?
                            $"{watch.Owner} -> {ret_string}" :
                            ret_string;
                    }
                    watch.Owner = ret_string;
                }
                else
                {
                    watch.Owner = null;
                }
            }
            return ret;
        }

        public static bool WatchFileInherited(
            WatchPath watch, Dictionary<string, string> dictionary, int serial, bool? isMonitor, FileInfo info)
        {
            if ((!isMonitor ?? true) && watch.Inherited == null) { return false; }

            bool ret = false;
            if (watch.Inherited == null)
            {
                ret = true;
                watch.Inherited = GetFileInherited(info);
            }
            else
            {
                string pathType = "file";
                string checkTarget = "Inherited";

                bool ret_bool = GetFileInherited(info);
                ret = ret_bool != watch.Inherited;
                dictionary[$"{pathType}_{checkTarget}_{serial}"] = ret ?
                    $"{watch.Inherited} -> {ret_bool}" :
                    ret_bool.ToString();

                watch.Inherited = ret_bool;
            }
            return ret;
        }

        public static bool WatchFileInherited(
            WatchPath watch, Dictionary<string, string> dictionary, int serial, FileInfo info)
        {
            bool ret = false;
            if (watch.Inherited ?? false)
            {
                if (info.Exists)
                {
                    bool ret_bool = GetFileInherited(info);
                    ret = ret_bool != watch.Inherited;
                    if (watch.Inherited != null)
                    {
                        string pathType = "file";
                        string checkTarget = "Inherited";
                        dictionary[$"{pathType}_{checkTarget}_{serial}"] = ret ?
                            $"{watch.Inherited} -> {ret_bool}" :
                            ret_bool.ToString();
                    }
                    watch.Inherited = ret_bool;
                }
                else
                {
                    watch.Inherited = null;
                }
            }
            return ret;
        }

        public static bool WatchDirectoryAccess(
            WatchPath watch, Dictionary<string, string> dictionary, int serial, bool? isMonitor, DirectoryInfo info)
        {
            if ((!isMonitor ?? true) && watch.Access == null) { return false; }

            bool ret = false;
            if (watch.Access == null)
            {
                ret = true;
                watch.Access = AccessRuleSummary.DirectoryToAccessString(info);
            }
            else
            {
                string pathType = "directory";
                string checkTarget = "Access";

                string ret_string = AccessRuleSummary.DirectoryToAccessString(info);
                ret = ret_string != watch.Access;
                dictionary[$"{pathType}_{checkTarget}_{serial}"] = ret ?
                    $"{watch.Access} -> {ret_string}" :
                    ret_string;

                watch.Access = ret_string;
            }
            return ret;
        }

        public static bool WatchDirectoryAccess(
            WatchPath watch, Dictionary<string, string> dictionary, int serial, DirectoryInfo info)
        {
            bool ret = false;
            if (watch.IsAccess ?? false)
            {
                if (info.Exists)
                {
                    string ret_string = AccessRuleSummary.DirectoryToAccessString(info);
                    ret = ret_string != watch.Access;
                    if (watch.Access != null)
                    {
                        string pathType = "directory";
                        string checkTarget = "Access";
                        dictionary[$"{pathType}_{checkTarget}_{serial}"] = ret ?
                            $"{watch.Access} -> {ret_string}" :
                            ret_string;
                    }
                    watch.Access = ret_string;
                }
                else
                {
                    watch.Access = null;
                }
            }
            return ret;
        }

        public static bool WatchDirectoryOwner(
            WatchPath watch, Dictionary<string, string> dictionary, int serial, bool? isMonitor, DirectoryInfo info)
        {
            if ((!isMonitor ?? true) && watch.Owner == null) { return false; }

            bool ret = false;
            if (watch.Owner == null)
            {
                ret = true;
                watch.Owner = GetDirectoryOwner(info);
            }
            else
            {
                string pathType = "directory";
                string checkTarget = "Owner";

                string ret_string = GetDirectoryOwner(info);
                ret = ret_string != watch.Owner;
                dictionary[$"{pathType}_{checkTarget}_{serial}"] = ret ?
                    $"{watch.Owner} -> {ret_string}" :
                    ret_string;

                watch.Owner = ret_string;
            }
            return ret;
        }

        public static bool WatchDirectoryOwner(
            WatchPath watch, Dictionary<string, string> dictionary, int serial, DirectoryInfo info)
        {
            bool ret = false;
            if (watch.IsOwner ?? false)
            {
                if (info.Exists)
                {
                    string ret_string = GetDirectoryOwner(info);
                    ret = ret_string != watch.Owner;
                    if (watch.Owner != null)
                    {
                        string pathType = "directory";
                        string checkTarget = "Owner";
                        dictionary[$"{pathType}_{checkTarget}_{serial}"] = ret ?
                            $"{watch.Owner} -> {ret_string}" :
                            ret_string;
                    }
                    watch.Owner = ret_string;
                }
                else
                {
                    watch.Owner = null;
                }
            }
            return ret;
        }

        public static bool WatchDirectoryInherited(
            WatchPath watch, Dictionary<string, string> dictionary, int serial, bool? isMonitor, DirectoryInfo info)
        {
            if ((!isMonitor ?? true) && watch.Inherited == null) { return false; }

            bool ret = false;
            if (watch.Inherited == null)
            {
                ret = true;
                watch.Inherited = GetDirectoryInherited(info);
            }
            else
            {
                string pathType = "directory";
                string checkTarget = "Inherited";

                bool ret_bool = GetDirectoryInherited(info);
                ret = ret_bool != watch.Inherited;
                dictionary[$"{pathType}_{checkTarget}_{serial}"] = ret ?
                    $"{watch.Inherited} -> {ret_bool}" :
                    ret_bool.ToString();

                watch.Inherited = ret_bool;
            }
            return ret;
        }

        public static bool WatchDirectoryInherited(
            WatchPath watch, Dictionary<string, string> dictionary, int serial, DirectoryInfo info)
        {
            bool ret = false;
            if (watch.Inherited ?? false)
            {
                if (info.Exists)
                {
                    bool ret_bool = GetDirectoryInherited(info);
                    ret = ret_bool != watch.Inherited;
                    if (watch.Inherited != null)
                    {
                        string pathType = "directory";
                        string checkTarget = "Inherited";
                        dictionary[$"{pathType}_{checkTarget}_{serial}"] = ret ?
                            $"{watch.Inherited} -> {ret_bool}" :
                            ret_bool.ToString();
                    }
                    watch.Inherited = ret_bool;
                }
                else
                {
                    watch.Inherited = null;
                }
            }
            return ret;
        }

        public static bool WatchRegistryKeyAccess(
            WatchPath watch, Dictionary<string, string> dictionary, int serial, bool? isMonitor, RegistryKey regKey)
        {
            if ((!isMonitor ?? true) && watch.Access == null) { return false; }

            bool ret = false;
            if (watch.Access == null)
            {
                ret = true;
                watch.Access = AccessRuleSummary.RegistryKeyToAccessString(regKey);
            }
            else
            {
                string pathType = "registry";
                string checkTarget = "Access";

                string ret_string = AccessRuleSummary.RegistryKeyToAccessString(regKey);
                ret = ret_string != watch.Access;
                dictionary[$"{pathType}_{checkTarget}_{serial}"] = ret ?
                    $"{watch.Access} -> {ret_string}" :
                    ret_string;

                watch.Access = ret_string;
            }
            return ret;
        }

        public static bool WatchRegistryKeyAccess(
            WatchPath watch, Dictionary<string, string> dictionary, int serial, RegistryKey regKey)
        {
            bool ret = false;
            if (watch.IsAccess ?? false)
            {
                if (regKey != null)
                {
                    string ret_string = AccessRuleSummary.RegistryKeyToAccessString(regKey);
                    ret = ret_string != watch.Access;
                    if (watch.Access != null)
                    {
                        string pathType = "registry";
                        string checkTarget = "Access";
                        dictionary[$"{pathType}_{checkTarget}_{serial}"] = ret ?
                            $"{watch.Access} -> {ret_string}" :
                            ret_string;
                    }
                    watch.Access = ret_string;
                }
                else
                {
                    watch.Access = null;
                }
            }
            return ret;
        }

        public static bool WatchRegistryKeyOwner(
            WatchPath watch, Dictionary<string, string> dictionary, int serial, bool? isMonitor, RegistryKey regKey)
        {
            if ((!isMonitor ?? true) && watch.Owner == null) { return false; }

            bool ret = false;
            if (watch.Owner == null)
            {
                ret = true;
                watch.Owner = GetRegistryKeyOwner(regKey);
            }
            else
            {
                string pathType = "registry";
                string checkTarget = "Owner";

                string ret_string = GetRegistryKeyOwner(regKey);
                ret = ret_string != watch.Owner;
                dictionary[$"{pathType}_{checkTarget}_{serial}"] = ret ?
                    $"{watch.Owner} -> {ret_string}" :
                    ret_string;

                watch.Owner = ret_string;
            }
            return ret;
        }

        public static bool WatchRegistryKeyOwner(
            WatchPath watch, Dictionary<string, string> dictionary, int serial, RegistryKey regKey)
        {
            bool ret = false;
            if (watch.IsOwner ?? false)
            {
                if (regKey != null)
                {
                    string ret_string = GetRegistryKeyOwner(regKey);
                    ret = ret_string != watch.Owner;
                    if (watch.Owner != null)
                    {
                        string pathType = "registry";
                        string checkTarget = "Owner";
                        dictionary[$"{pathType}_{checkTarget}_{serial}"] = ret ?
                            $"{watch.Owner} -> {ret_string}" :
                            ret_string;
                    }
                    watch.Owner = ret_string;
                }
                else
                {
                    watch.Owner = null;
                }
            }
            return ret;
        }

        public static bool WatchRegistryKeyInherited(
            WatchPath watch, Dictionary<string, string> dictionary, int serial, bool? isMonitor, RegistryKey regKey)
        {
            if ((!isMonitor ?? true) && watch.Inherited == null) { return false; }

            bool ret = false;
            if (watch.Inherited == null)
            {
                ret = true;
                watch.Inherited = GetRegistryKeyInherited(regKey);
            }
            else
            {
                string pathType = "registry";
                string checkTarget = "Inherited";

                bool ret_bool = GetRegistryKeyInherited(regKey);
                ret = ret_bool != watch.Inherited;
                dictionary[$"{pathType}_{checkTarget}_{serial}"] = ret ?
                    $"{watch.Inherited} -> {ret_bool}" :
                    ret_bool.ToString();

                watch.Inherited = ret_bool;
            }
            return ret;
        }

        public static bool WatchRegistryKeyInherited(
            WatchPath watch, Dictionary<string, string> dictionary, int serial, RegistryKey regKey)
        {
            bool ret = false;
            if (watch.IsInherited ?? false)
            {
                if (regKey != null)
                {
                    bool ret_bool = GetRegistryKeyInherited(regKey);
                    ret = ret_bool != watch.Inherited;
                    if (watch.Inherited != null)
                    {
                        string pathType = "registry";
                        string checkTarget = "Inherited";
                        dictionary[$"{pathType}_{checkTarget}_{serial}"] = ret ?
                            $"{watch.Inherited} -> {ret_bool}" :
                            ret_bool.ToString();
                    }
                    watch.Inherited = ret_bool;
                }
                else
                {
                    watch.Inherited = null;
                }
            }
            return ret;
        }

        #endregion
        #region Get method

        /*
        public static string GetFileOwner(string path)
        {
            return new FileInfo(path).GetAccessControl().GetOwner(typeof(NTAccount)).Value;
        }
        */
        public static string GetFileOwner(FileInfo info)
        {
            return info.GetAccessControl().GetOwner(typeof(NTAccount)).Value;
        }
        /*
        public static string GetDirectoryOwner(string path)
        {
            return new DirectoryInfo(path).GetAccessControl().GetOwner(typeof(NTAccount)).Value;
        }
        */
        public static string GetDirectoryOwner(DirectoryInfo info)
        {
            return info.GetAccessControl().GetOwner(typeof(NTAccount)).Value;
        }

        /*
        public static bool GetFileInherited(string path)
        {
            return !(new FileInfo(path).GetAccessControl().AreAccessRulesProtected);
        }
        */
        public static bool GetFileInherited(FileInfo info)
        {
            return !info.GetAccessControl().AreAccessRulesProtected;
        }
        /*
        public static bool GetDirectoryInherited(string path)
        {
            return !(new DirectoryInfo(path).GetAccessControl().AreAccessRulesProtected);
        }
        */
        public static bool GetDirectoryInherited(DirectoryInfo info)
        {
            return !info.GetAccessControl().AreAccessRulesProtected;
        }

        public static string GetRegistryKeyOwner(RegistryKey regKey)
        {
            return regKey.GetAccessControl().GetOwner(typeof(NTAccount)).Value;
        }
        public static bool GetRegistryKeyInherited(RegistryKey regKey)
        {
            return !regKey.GetAccessControl().AreAccessRulesProtected;
        }

        #endregion
    }
}
