using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IO.Lib;

namespace Audit.Lib.Monitor
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class WatchFunctions
    {
        /// <summary>
        /// ファイルのWatchチェック
        /// </summary>
        /// <param name="target_monitor"></param>
        /// <param name="target_db"></param>
        /// <param name="dictionary"></param>
        /// <param name="serial"></param>
        /// <returns></returns>
        internal static bool CheckFile(MonitorTarget target_monitor, MonitorTarget target_db, Dictionary<string, string> dictionary, int serial)
        {
            bool result = false;

            //  CreationTime
            if (target_monitor.IsCreationTime ?? false)
            {
                target_monitor.CheckCreationTime();
                bool  ret = target_monitor.CreationTime != target_db.CreationTime;
                if (target_db.CreationTime != null)
                {
                    dictionary[$"{serial}_{target_monitor.PathTypeName}_CreationTime"] = ret ?
                        string.Format("{0} -> {1}",
                            target_db.CreationTime,
                            target_monitor.CreationTime) :
                        target_monitor.CreationTime;
                }
                result |= ret;
            }

            //  LastWriteTime
            if (target_monitor.IsLastWriteTime ?? false)
            {
                target_monitor.CheckLastWriteTime();
                bool ret = target_monitor.LastWriteTime != target_db.LastWriteTime;
                if (target_db.LastWriteTime != null)
                {
                    dictionary[$"{serial}_{target_monitor.PathTypeName}_LastWriteTime"] = ret ?
                        string.Format("{0} -> {1}",
                            target_db.LastWriteTime,
                            target_monitor.LastWriteTime) :
                        target_monitor.LastWriteTime;
                }
                result |= ret;
            }

            //  LastAccessTime
            if (target_monitor.IsLastAccessTime ?? false)
            {
                target_monitor.CheckLastAccessTime();
                bool ret = target_monitor.LastAccessTime != target_db.LastAccessTime;
                if (target_db.LastAccessTime != null)
                {
                    dictionary[$"{serial}_{target_monitor.PathTypeName}_LastAccessTime"] = ret ?
                        string.Format("{0} -> {1}",
                            target_db.LastAccessTime,
                            target_monitor.LastAccessTime) :
                        target_monitor.LastAccessTime;
                }
                result |= ret;
            }

            //  Access
            if (target_monitor.IsAccess ?? false)
            {
                target_monitor.CheckAccess();
                bool ret = target_monitor.Access != target_db.Access;
                if (target_db.Access != null)
                {
                    dictionary[$"{serial}_{target_monitor.PathTypeName}_Access"] = ret ?
                        string.Format("{0} -> {1}",
                            target_db.Access,
                            target_monitor.Access) :
                        target_monitor.Access;
                }
                result |= ret;
            }

            //  Owner
            if (target_monitor.IsOwner ?? false)
            {
                target_monitor.CheckOwner();
                bool ret = target_monitor.Owner != target_db.Owner;
                if (target_db.Owner != null)
                {
                    dictionary[$"{serial}_{target_monitor.PathTypeName}_Owner"] = ret ?
                        string.Format("{0} -> {1}",
                            target_db.Owner,
                            target_monitor.Owner) :
                        target_monitor.Owner;
                }
                result |= ret;
            }

            //  Inherited
            if (target_monitor.IsInherited ?? false)
            {
                target_monitor.CheckInherited();
                bool ret = target_monitor.Inherited != target_db.Inherited;
                if (target_db.Inherited != null)
                {
                    dictionary[$"{serial}_{target_monitor.PathTypeName}_Inherited"] = ret ?
                        string.Format("{0} -> {1}",
                            target_db.Inherited,
                            target_monitor.Inherited) :
                        target_monitor.Inherited.ToString();
                }
                result |= ret;
            }

            //  Attributes
            if (target_monitor.IsAttributes ?? false)
            {
                target_monitor.CheckAttributes();
                bool ret = !target_monitor.Attributes.SequenceEqual(target_db.Attributes ?? new bool[0] { });
                if (target_db.Attributes != null)
                {
                    dictionary[$"{serial}_{target_monitor.PathTypeName}_Attributes"] = ret ?
                        string.Format("{0} -> {1}",
                            MonitorFunctions.ToReadableAttributes(target_db.Attributes),
                            MonitorFunctions.ToReadableAttributes(target_monitor.Attributes)) :
                        MonitorFunctions.ToReadableAttributes(target_monitor.Attributes);
                }
                result |= ret;
            }

            //  MD5Hash
            if (target_monitor.IsMD5Hash ?? false)
            {
                target_monitor.CheckMD5Hash();
                bool ret = target_monitor.MD5Hash != target_db.MD5Hash;
                if (target_db.MD5Hash != null)
                {
                    dictionary[$"{serial}_{target_monitor.PathTypeName}_MD5Hash"] = ret ?
                        string.Format("{0} -> {1}",
                            target_db.MD5Hash,
                            target_monitor.MD5Hash) :
                        target_monitor.MD5Hash;
                }
                result |= ret;
            }

            //  SHA256Hash
            if (target_monitor.IsSHA256Hash ?? false)
            {
                target_monitor.CheckSHA256Hash();
                bool ret = target_monitor.SHA256Hash != target_db.SHA256Hash;
                if (target_db.SHA256Hash != null)
                {
                    dictionary[$"{serial}_{target_monitor.PathTypeName}_SHA256Hash"] = ret ?
                        string.Format("{0} -> {1}",
                            target_db.SHA256Hash,
                            target_monitor.SHA256Hash) :
                        target_monitor.SHA256Hash;
                }
                result |= ret;
            }

            //  SHA512Hash
            if (target_monitor.IsSHA512Hash ?? false)
            {
                target_monitor.CheckSHA512Hash();
                bool ret = target_monitor.SHA512Hash != target_db.SHA512Hash;
                if (target_db.SHA512Hash != null)
                {
                    dictionary[$"{serial}_{target_monitor.PathTypeName}_SHA512Hash"] = ret ?
                        string.Format("{0} -> {1}",
                            target_db.SHA512Hash,
                            target_monitor.SHA512Hash) :
                        target_monitor.SHA512Hash;
                }
                result |= ret;
            }

            //  Size
            if (target_monitor.IsSize ?? false)
            {
                target_monitor.CheckSize();
                bool ret = target_monitor.Size != target_db.Size;
                if (target_db.Size != null)
                {
                    dictionary[$"{serial}_{target_monitor.PathTypeName}_Size"] = ret ?
                        string.Format("{0}({1}) -> {2}({3})",
                            target_db.Size,
                            MonitorFunctions.ToReadableSize(target_db.Size ?? 0),
                            target_monitor.Size,
                            MonitorFunctions.ToReadableSize(target_monitor.Size ?? 0)) :
                        string.Format("{0}({1})",
                            target_monitor.Size,
                            MonitorFunctions.ToReadableSize(target_monitor.Size ?? 0));
                }
                result |= ret;
            }

            return result;
        }

        /// <summary>
        /// ディレクトリのWatchチェック
        /// </summary>
        /// <param name="target_monitor"></param>
        /// <param name="target_db"></param>
        /// <param name="dictionary"></param>
        /// <param name="serial"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        internal static bool CheckDirectory(MonitorTarget target_monitor, MonitorTarget target_db, Dictionary<string, string> dictionary, int serial, int depth)
        {
            bool result = false;

            //  CreationTime
            if (target_monitor.IsCreationTime ?? false)
            {
                target_monitor.CheckCreationTime();
                bool ret = target_monitor.CreationTime != target_db.CreationTime;
                if (target_db.CreationTime != null)
                {
                    dictionary[$"{serial}_{target_monitor.PathTypeName}_CreationTime"] = ret ?
                        string.Format("{0} -> {1}",
                            target_db.CreationTime,
                            target_monitor.CreationTime) :
                        target_monitor.CreationTime;
                }
                result |= ret;
            }

            //  LastWriteTime
            if (target_monitor.IsLastWriteTime ?? false)
            {
                target_monitor.CheckLastWriteTime();
                bool ret = target_monitor.LastWriteTime != target_db.LastWriteTime;
                if (target_db.LastWriteTime != null)
                {
                    dictionary[$"{serial}_{target_monitor.PathTypeName}_LastWriteTime"] = ret ?
                        string.Format("{0} -> {1}",
                            target_db.LastWriteTime,
                            target_monitor.LastWriteTime) :
                        target_monitor.LastWriteTime;
                }
                result |= ret;
            }

            //  LastAccessTime
            if (target_monitor.IsLastAccessTime ?? false)
            {
                target_monitor.CheckLastAccessTime();
                bool ret = target_monitor.LastAccessTime != target_db.LastAccessTime;
                if (target_db.LastAccessTime != null)
                {
                    dictionary[$"{serial}_{target_monitor.PathTypeName}_LastAccessTime"] = ret ?
                        string.Format("{0} -> {1}",
                            target_db.LastAccessTime,
                            target_monitor.LastAccessTime) :
                        target_monitor.LastAccessTime;
                }
                result |= ret;
            }

            //  Access
            if (target_monitor.IsAccess ?? false)
            {
                target_monitor.CheckAccess();
                bool ret = target_monitor.Access != target_db.Access;
                if (target_db.Access != null)
                {
                    dictionary[$"{serial}_{target_monitor.PathTypeName}_Access"] = ret ?
                        string.Format("{0} -> {1}",
                            target_db.Access,
                            target_monitor.Access) :
                        target_monitor.Access;
                }
                result |= ret;
            }

            //  Owner
            if (target_monitor.IsOwner ?? false)
            {
                target_monitor.CheckOwner();
                bool ret = target_monitor.Owner != target_db.Owner;
                if (target_db.Owner != null)
                {
                    dictionary[$"{serial}_{target_monitor.PathTypeName}_Owner"] = ret ?
                        string.Format("{0} -> {1}",
                            target_db.Owner,
                            target_monitor.Owner) :
                        target_monitor.Owner;
                }
                result |= ret;
            }

            //  Inherited
            if (target_monitor.IsInherited ?? false)
            {
                target_monitor.CheckInherited();
                bool ret = target_monitor.Inherited != target_db.Inherited;
                if (target_db.Inherited != null)
                {
                    dictionary[$"{serial}_{target_monitor.PathTypeName}_Inherited"] = ret ?
                        string.Format("{0} -> {1}",
                            target_db.Inherited,
                            target_monitor.Inherited) :
                        target_monitor.Inherited.ToString();
                }
                result |= ret;
            }

            //  Attributes
            if (target_monitor.IsAttributes ?? false)
            {
                target_monitor.CheckAttributes();
                bool ret = !target_monitor.Attributes.SequenceEqual(target_db.Attributes ?? new bool[0] { });
                if (target_db.Attributes != null)
                {
                    dictionary[$"{serial}_{target_monitor.PathTypeName}_Attributes"] = ret ?
                        string.Format("{0} -> {1}",
                            MonitorFunctions.ToReadableAttributes(target_db.Attributes),
                            MonitorFunctions.ToReadableAttributes(target_monitor.Attributes)) :
                        MonitorFunctions.ToReadableAttributes(target_monitor.Attributes);
                }
                result |= ret;
            }

            //  ChildCount
            if ((target_monitor.IsChildCount ?? false) && depth == 0)
            {
                target_monitor.CheckChildCount();
                bool ret = !target_monitor.ChildCount.SequenceEqual(target_db.ChildCount ?? new int[0] { });
                if (target_db.ChildCount != null)
                {
                    dictionary[$"{serial}_{target_monitor.PathTypeName}_ChildCount"] = ret ?
                        string.Format("{0} -> {1}",
                            MonitorFunctions.ToReadableChildCount(target_db.ChildCount, target_monitor.PathType == PathType.Directory),
                            MonitorFunctions.ToReadableChildCount(target_monitor.ChildCount, target_monitor.PathType == PathType.Directory)) :
                        MonitorFunctions.ToReadableChildCount(target_monitor.ChildCount, target_monitor.PathType == PathType.Directory);
                }
                result |= ret;
            }

            return result;
        }

        /// <summary>
        /// レジストリキーのWatchチェック
        /// </summary>
        /// <param name="target_monitor"></param>
        /// <param name="target_db"></param>
        /// <param name="dictionary"></param>
        /// <param name="serial"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        internal static bool CheckRegistrykey(MonitorTarget target_monitor, MonitorTarget target_db, Dictionary<string, string> dictionary, int serial, int depth)
        {
            bool result = false;

            //  Access
            if (target_monitor.IsAccess ?? false)
            {
                target_monitor.CheckAccess();
                bool ret = target_monitor.Access != target_db.Access;
                if (target_db.Access != null)
                {
                    dictionary[$"{serial}_{target_monitor.PathTypeName}_Access"] = ret ?
                        string.Format("{0} -> {1}",
                            target_db.Access,
                            target_monitor.Access) :
                        target_monitor.Access;
                }
                result |= ret;
            }

            //  Owner
            if (target_monitor.IsOwner ?? false)
            {
                target_monitor.CheckOwner();
                bool ret = target_monitor.Owner != target_db.Owner;
                if (target_db.Owner != null)
                {
                    dictionary[$"{serial}_{target_monitor.PathTypeName}_Owner"] = ret ?
                        string.Format("{0} -> {1}",
                            target_db.Owner,
                            target_monitor.Owner) :
                        target_monitor.Owner;
                }
                result |= ret;
            }

            //  Inherited
            if (target_monitor.IsInherited ?? false)
            {
                target_monitor.CheckInherited();
                bool ret = target_monitor.Inherited != target_db.Inherited;
                if (target_db.Inherited != null)
                {
                    dictionary[$"{serial}_{target_monitor.PathTypeName}_Inherited"] = ret ?
                        string.Format("{0} -> {1}",
                            target_db.Inherited,
                            target_monitor.Inherited) :
                        target_monitor.Inherited.ToString();
                }
                result |= ret;
            }

            //  ChildCount
            if ((target_monitor.IsChildCount ?? false) && depth == 0)
            {
                target_monitor.CheckChildCount();
                bool ret = !target_monitor.ChildCount.SequenceEqual(target_db.ChildCount ?? new int[0] { });
                if (target_db.ChildCount != null)
                {
                    dictionary[$"{serial}_{target_monitor.PathTypeName}_ChildCount"] = ret ?
                        string.Format("{0} -> {1}",
                            MonitorFunctions.ToReadableChildCount(target_db.ChildCount, target_monitor.PathType == PathType.Directory),
                            MonitorFunctions.ToReadableChildCount(target_monitor.ChildCount, target_monitor.PathType == PathType.Directory)) :
                        MonitorFunctions.ToReadableChildCount(target_monitor.ChildCount, target_monitor.PathType == PathType.Directory);
                }
                result |= ret;
            }

            return result;
        }

        /// <summary>
        /// レジストリ値のWatchチェック
        /// </summary>
        /// <param name="target_monitor"></param>
        /// <param name="target_db"></param>
        /// <param name="dictionary"></param>
        /// <param name="serial"></param>
        /// <returns></returns>
        internal static bool CheckRegistryValue(MonitorTarget target_monitor, MonitorTarget target_db, Dictionary<string, string> dictionary, int serial)
        {
            bool result = false;

            //  MD5Hash
            if (target_monitor.IsMD5Hash ?? false)
            {
                target_monitor.CheckMD5Hash();
                bool ret = target_monitor.MD5Hash != target_db.MD5Hash;
                if (target_db.MD5Hash != null)
                {
                    dictionary[$"{serial}_{target_monitor.PathTypeName}_MD5Hash"] = ret ?
                        string.Format("{0} -> {1}",
                            target_db.MD5Hash,
                            target_monitor.MD5Hash) :
                        target_monitor.MD5Hash;
                }
                result |= ret;
            }

            //  SHA256Hash
            if (target_monitor.IsSHA256Hash ?? false)
            {
                target_monitor.CheckSHA256Hash();
                bool ret = target_monitor.SHA256Hash != target_db.SHA256Hash;
                if (target_db.SHA256Hash != null)
                {
                    dictionary[$"{serial}_{target_monitor.PathTypeName}_SHA256Hash"] = ret ?
                        string.Format("{0} -> {1}",
                            target_db.SHA256Hash,
                            target_monitor.SHA256Hash) :
                        target_monitor.SHA256Hash;
                }
                result |= ret;
            }

            //  SHA512Hash
            if (target_monitor.IsSHA512Hash ?? false)
            {
                target_monitor.CheckSHA512Hash();
                bool ret = target_monitor.SHA512Hash != target_db.SHA512Hash;
                if (target_db.SHA512Hash != null)
                {
                    dictionary[$"{serial}_{target_monitor.PathTypeName}_SHA512Hash"] = ret ?
                        string.Format("{0} -> {1}",
                            target_db.SHA512Hash,
                            target_monitor.SHA512Hash) :
                        target_monitor.SHA512Hash;
                }
                result |= ret;
            }

            //  RegistryType
            if(target_monitor.IsRegistryType ?? false)
            {
                target_monitor.CheckRegistryType();
                bool ret = target_monitor.RegistryType != target_db.RegistryType;
                if(target_db.RegistryType != null)
                {
                    dictionary[$"{serial}_{target_monitor.PathTypeName}_RegistryType"] = ret ?
                        string.Format("{0} -> {1}",
                            target_db.RegistryType,
                            target_monitor.RegistryType) :
                        target_monitor.RegistryType;
                }
                result |= ret;
            }

            return result;
        }
    }
}
