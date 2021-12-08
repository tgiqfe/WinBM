using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Audit.Lib
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class MonitorTimeStamp
    {
        #region Compare method



        #endregion
        #region Watch method

        public static bool WatchFileCreationTime(
            WatchPath watch, Dictionary<string, string> dictionary, int serial, FileInfo info)
        {
            bool ret = false;
            if (watch.IsCreationTime ?? false)
            {
                if (info.Exists)
                {
                    string ret_string = GetFileCreationTime(info,
                        watch.IsDateOnly ?? false,
                        watch.IsTimeOnly ?? false);
                    ret = ret_string != watch.CreationTime;
                    if (watch.CreationTime != null)
                    {
                        string pathType = "file";
                        string checkTarget = "CreationTime";
                        dictionary[$"{pathType}_{checkTarget}_{serial}"] = ret ?
                            $"{watch.CreationTime} -> {ret_string}" :
                            ret_string;
                    }
                    watch.CreationTime = ret_string;
                }
                else
                {
                    watch.CreationTime = null;
                }
            }
            return ret;
        }

        public static bool WatchDirectoryCreationTime(
            WatchPath watch, Dictionary<string, string> dictionary, int serial, DirectoryInfo info)
        {
            bool ret = false;
            if (watch.IsCreationTime ?? false)
            {
                if (info.Exists)
                {
                    string ret_string = GetDirectoryCreationTime(info,
                        watch.IsDateOnly ?? false,
                        watch.IsTimeOnly ?? false);
                    ret = ret_string != watch.CreationTime;
                    if (watch.CreationTime != null)
                    {
                        string pathType = "directory";
                        string checkTarget = "CreationTime";
                        dictionary[$"{pathType}_{checkTarget}_{serial}"] = ret ?
                            $"{watch.CreationTime} -> {ret_string}" :
                            ret_string;
                    }
                    watch.CreationTime = ret_string;
                }
                else
                {
                    watch.CreationTime = null;
                }
            }
            return ret;
        }

        public static bool WatchFileLastWriteTime(
            WatchPath watch, Dictionary<string, string> dictionary, int serial, FileInfo info)
        {
            bool ret = false;
            if (watch.IsLastWriteTime ?? false)
            {
                if (info.Exists)
                {
                    string ret_string = GetFileLastWriteTime(info,
                        watch.IsDateOnly ?? false,
                        watch.IsTimeOnly ?? false);
                    ret = ret_string != watch.LastWriteTime;
                    if (watch.LastWriteTime != null)
                    {
                        string pathType = "file";
                        string checkTarget = "LastWriteTime";
                        dictionary[$"{pathType}_{checkTarget}_{serial}"] = ret ?
                            $"{watch.LastWriteTime} -> {ret_string}" :
                            ret_string;
                    }
                    watch.LastWriteTime = ret_string;
                }
                else
                {
                    watch.LastWriteTime = null;
                }
            }
            return ret;
        }

        public static bool WatchDirectoryLastWriteTime(
            WatchPath watch, Dictionary<string, string> dictionary, int serial, DirectoryInfo info)
        {
            bool ret = false;
            if (watch.IsLastWriteTime ?? false)
            {
                if (info.Exists)
                {
                    string ret_string = GetDirectoryLastWriteTime(info,
                        watch.IsDateOnly ?? false,
                        watch.IsTimeOnly ?? false);
                    ret = ret_string != watch.LastWriteTime;
                    if (watch.LastWriteTime != null)
                    {
                        string pathType = "directory";
                        string checkTarget = "LastWriteTime";
                        dictionary[$"{pathType}_{checkTarget}_{serial}"] = ret ?
                            $"{watch.LastWriteTime} -> {ret_string}" :
                            ret_string;
                    }
                    watch.LastWriteTime = ret_string;
                }
                else
                {
                    watch.LastWriteTime = null;
                }
            }
            return ret;
        }

        public static bool WatchFileLastAccessTime(
            WatchPath watch, Dictionary<string, string> dictionary, int serial, FileInfo info)
        {
            bool ret = false;
            if (watch.IsLastAccessTime ?? false)
            {
                if (info.Exists)
                {
                    string ret_string = GetFileLastAccessTime(info,
                        watch.IsDateOnly ?? false,
                        watch.IsTimeOnly ?? false);
                    ret = ret_string != watch.LastAccessTime;
                    if (watch.LastAccessTime != null)
                    {
                        string pathType = "file";
                        string checkTarget = "LastAccessTime";
                        dictionary[$"{pathType}_{checkTarget}_{serial}"] = ret ?
                            $"{watch.LastAccessTime} -> {ret_string}" :
                            ret_string;
                    }
                    watch.LastAccessTime = ret_string;
                }
                else
                {
                    watch.LastAccessTime = null;
                }
            }
            return ret;
        }

        public static bool WatchDirectoryLastAccessTime(
            WatchPath watch, Dictionary<string, string> dictionary, int serial, DirectoryInfo info)
        {
            bool ret = false;
            if (watch.IsLastAccessTime ?? false)
            {
                if (info.Exists)
                {
                    string ret_string = GetDirectoryLastAccessTime(info,
                        watch.IsDateOnly ?? false,
                        watch.IsTimeOnly ?? false);
                    ret = ret_string != watch.LastAccessTime;
                    if (watch.LastAccessTime != null)
                    {
                        string pathType = "directory";
                        string checkTarget = "LastAccessTime";
                        dictionary[$"{pathType}_{checkTarget}_{serial}"] = ret ?
                            $"{watch.LastAccessTime} -> {ret_string}" :
                            ret_string;
                    }
                    watch.LastAccessTime = ret_string;
                }
                else
                {
                    watch.LastAccessTime = null;
                }
            }
            return ret;
        }

        #endregion
        #region Get method

        /*
        public static string GetFileCreationTime(string path, bool isDateOnly, bool isTimeOnly)
        {
            return DateToString(new FileInfo(path).CreationTime, isDateOnly, isTimeOnly);
        }
        */
        public static string GetFileCreationTime(FileInfo info, bool isDateOnly, bool isTimeOnly)
        {
            return DateToString(info.CreationTime, isDateOnly, isTimeOnly);
        }
        /*
        public static string GetDirectoryCreationTime(string path, bool isDateOnly, bool isTimeOnly)
        {
            return DateToString(new DirectoryInfo(path).CreationTime, isDateOnly, isTimeOnly);
        }
        */
        public static string GetDirectoryCreationTime(DirectoryInfo info, bool isDateOnly, bool isTimeOnly)
        {
            return DateToString(info.CreationTime, isDateOnly, isTimeOnly);
        }

        /*
        public static string GetFileLastWriteTime(string path, bool isDateOnly, bool isTimeOnly)
        {
            return DateToString(new FileInfo(path).LastWriteTime, isDateOnly, isTimeOnly);
        }
        */
        public static string GetFileLastWriteTime(FileInfo info, bool isDateOnly, bool isTimeOnly)
        {
            return DateToString(info.LastWriteTime, isDateOnly, isTimeOnly);
        }
        /*
        public static string GetDirectoryLastWriteTime(string path, bool isDateOnly, bool isTimeOnly)
        {
            return DateToString(new DirectoryInfo(path).LastWriteTime, isDateOnly, isTimeOnly);
        }
        */
        public static string GetDirectoryLastWriteTime(DirectoryInfo info, bool isDateOnly, bool isTimeOnly)
        {
            return DateToString(info.LastWriteTime, isDateOnly, isTimeOnly);
        }

        /*
        public static string GetFileLastAccessTime(string path, bool isDateOnly, bool isTimeOnly)
        {
            return DateToString(new FileInfo(path).LastAccessTime, isDateOnly, isTimeOnly);
        }
        */
        public static string GetFileLastAccessTime(FileInfo info, bool isDateOnly, bool isTimeOnly)
        {
            return DateToString(info.LastAccessTime, isDateOnly, isTimeOnly);
        }
        /*
        public static string GetDirectoryLastAccessTime(string path, bool isDateOnly, bool isTimeOnly)
        {
            return DateToString(new DirectoryInfo(path).LastAccessTime, isDateOnly, isTimeOnly);
        }
        */
        public static string GetDirectoryLastAccessTime(DirectoryInfo info, bool isDateOnly, bool isTimeOnly)
        {
            return DateToString(info.LastAccessTime, isDateOnly, isTimeOnly);
        }

        private static string DateToString(DateTime date, bool isDateOnly, bool isTimeOnly)
        {
            if (isDateOnly)
                return date.ToString("yyyy/MM/dd");
            else if (isTimeOnly)
                return date.ToString("HH:mm:ss");
            else
                return date.ToString("yyyy/MM/dd HH:mm:ss");
        }

        #endregion
    }
}
