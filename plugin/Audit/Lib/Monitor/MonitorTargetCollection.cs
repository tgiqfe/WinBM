using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Win32;
using IO.Lib;

namespace Audit.Lib.Monitor
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class MonitorTargetCollection
    {
        public bool? IsCreationTime { get; set; }
        public bool? IsLastWriteTime { get; set; }
        public bool? IsLastAccessTime { get; set; }
        public bool? IsAccess { get; set; }
        public bool? IsOwner { get; set; }
        public bool? IsInherited { get; set; }
        public bool? IsAttributes { get; set; }
        public bool? IsMD5Hash { get; set; }
        public bool? IsSHA256Hash { get; set; }
        public bool? IsSHA512Hash { get; set; }
        public bool? IsSize { get; set; }
        public bool? IsChildCount { get; set; }
        public bool? IsRegistryType { get; set; }
        public bool? IsDateOnly { get; set; }
        public bool? IsTimeOnly { get; set; }

        public string[] PrevPaths { get; set; }
        public string[] PrevNames { get; set; }

        public Dictionary<string, MonitorTarget> Targets { get; set; }

        const string REGPATH_PREFIX = "[reg]";

        public MonitorTargetCollection()
        {
            this.Targets = new Dictionary<string, MonitorTarget>();
        }

        #region Get/Set MonitorTarget

        public MonitorTarget GetMonitorTarget(string path)
        {
            string matchKey = this.Targets.Keys.FirstOrDefault(x => x.Equals(path, StringComparison.OrdinalIgnoreCase));
            return matchKey == null ? null : this.Targets[matchKey];
        }

        public MonitorTarget GetMonitorTarget(string path, string name)
        {
            string regPath = REGPATH_PREFIX + path + "\\" + name;
            string matchKey = this.Targets.Keys.FirstOrDefault(x => x.Equals(regPath, StringComparison.OrdinalIgnoreCase));
            return matchKey == null ? null : this.Targets[matchKey];
        }

        public void SetMonitorTarget(string path, MonitorTarget target)
        {
            this.Targets[path] = target;
            this._CheckedKeys.Add(path);
        }

        public void SetMonitorTarget(string path, string name, MonitorTarget target)
        {
            string regPath = REGPATH_PREFIX + path + "\\" + name;
            this.Targets[regPath] = target;
            this._CheckedKeys.Add(regPath);
        }

        #endregion
        #region UncheckKey

        private List<string> _CheckedKeys = new List<string>();

        public IEnumerable<string> GetUncheckedKeys()
        {
            return this.Targets.Keys.Where(x => !_CheckedKeys.Any(y => y.Equals(x, StringComparison.OrdinalIgnoreCase)));
        }

        #endregion
        #region Load/Save

        public static MonitorTargetCollection Load(string dbDir, string id)
        {
            try
            {
                using (var sr = new StreamReader(Path.Combine(dbDir, id), Encoding.UTF8))
                {
                    return JsonSerializer.Deserialize<MonitorTargetCollection>(sr.ReadToEnd());
                }
            }
            catch { }
            return new MonitorTargetCollection();
        }

        public void Save(string dbDir, string id)
        {
            if (!Directory.Exists(dbDir))
            {
                Directory.CreateDirectory(dbDir);
            }
            using (var sw = new StreamWriter(Path.Combine(dbDir, id), false, Encoding.UTF8))
            {
                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions()
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                });
                sw.WriteLine(json);
            }
        }

        #endregion
        #region Check method

        public bool CheckFile(MonitorTarget target, Dictionary<string, string> dictionary, int serial)
        {
            bool ret = false;

            MonitorTarget target_db = this.Targets.ContainsKey(target.Path) ?
                this.Targets[target.Path] :
                new MonitorTarget(PathType.File, target.Path, "file");

            //  CreationTime
            if (IsCreationTime ?? false)
            {
                target.CreationTime = MonitorFunctions.GetCreationTime(target.FileInfo, this.IsDateOnly ?? false, this.IsTimeOnly ?? false);
                bool result = target.CreationTime != target_db.CreationTime;
                if (target_db.CreationTime != null)
                {
                    dictionary[$"{serial}_{target.PathTypeName}_CreationTime"] = result ?
                        string.Format("{0} -> {1}",
                            target_db.CreationTime,
                            target.CreationTime) :
                        target.CreationTime;
                }
                ret |= result;
            }

            //  LastWriteTime
            if (IsLastWriteTime ?? false)
            {
                target.LastWriteTime = MonitorFunctions.GetLastWriteTime(target.FileInfo, this.IsDateOnly ?? false, this.IsTimeOnly ?? false);
                bool result = target.LastWriteTime != target_db.LastWriteTime;
                if (target_db.LastWriteTime != null)
                {
                    dictionary[$"{serial}_{target.PathTypeName}_LastWriteTime"] = result ?
                        string.Format("{0} -> {1}",
                            target_db.LastWriteTime,
                            target.LastWriteTime) :
                        target.LastWriteTime;
                }
                ret |= result;
            }

            //  LastAccessTime
            if (IsLastAccessTime ?? false)
            {
                target.LastAccessTime = MonitorFunctions.GetLastAccessTime(target.FileInfo, this.IsDateOnly ?? false, this.IsTimeOnly ?? false);
                bool result = target.LastAccessTime != target_db.LastAccessTime;
                if (target_db.LastAccessTime != null)
                {
                    dictionary[$"{serial}_{target.PathTypeName}_LastAccessTime"] = result ?
                        string.Format("{0} -> {1}",
                            target_db.LastAccessTime,
                            target.LastAccessTime) :
                        target.LastAccessTime;
                }
                ret |= result;
            }

            //  Access
            if (IsAccess ?? false)
            {
                target.Access = AccessRuleSummary.FileToAccessString(target.FileInfo);
                bool result = target.Access != target_db.Access;
                if (target_db.Access != null)
                {
                    dictionary[$"{serial}_{target.PathTypeName}_Access"] = result ?
                        string.Format("{0} -> {1}",
                            target_db.Access,
                            target.Access) :
                        target.Access;
                }
                ret |= result;
            }

            //  Owner
            if (IsOwner ?? false)
            {
                target.Owner = target.FileInfo.GetAccessControl().GetOwner(typeof(System.Security.Principal.NTAccount)).Value;
                bool result = target.Owner != target_db.Owner;
                if (target_db.Owner != null)
                {
                    dictionary[$"{serial}_{target.PathTypeName}_Owner"] = result ?
                        string.Format("{0} -> {1}",
                            target_db.Owner,
                            target.Owner) :
                        target.Owner;
                }
                ret |= result;
            }

            //  Inherited
            if (IsInherited ?? false)
            {
                target.Inherited = !target.FileInfo.GetAccessControl().AreAccessRulesProtected;
                bool result = target.Inherited != target_db.Inherited;
                if (target_db.Inherited != null)
                {
                    dictionary[$"{serial}_{target.PathTypeName}_Inherited"] = result ?
                        string.Format("{0} -> {1}",
                            target_db.Inherited,
                            target.Inherited) :
                        target.Inherited.ToString();
                }
                ret |= result;
            }

            //  Attributes
            if (IsAttributes ?? false)
            {
                target.Attributes = MonitorFunctions.GetAttributes(target.Path);
                bool result = !target.Attributes.SequenceEqual(target_db.Attributes ?? new bool[0] { });
                if (target_db.Attributes != null)
                {
                    dictionary[$"{serial}_{target.PathTypeName}_Attributes"] = result ?
                        string.Format("{0} -> {1}",
                            MonitorFunctions.ToReadableAttributes(target_db.Attributes),
                            MonitorFunctions.ToReadableAttributes(target.Attributes)) :
                        MonitorFunctions.ToReadableAttributes(target.Attributes);
                }
                ret |= result;
            }

            //  MD5Hash
            if (IsMD5Hash ?? false)
            {
                target.MD5Hash = MonitorFunctions.GetHash(target.Path, System.Security.Cryptography.MD5.Create());
                bool result = target.MD5Hash != target_db.MD5Hash;
                if (target_db.MD5Hash != null)
                {
                    dictionary[$"{serial}_{target.PathTypeName}_MD5Hash"] = result ?
                        string.Format("{0} -> {1}",
                            target_db.MD5Hash,
                            target.MD5Hash) :
                        target.MD5Hash;
                }
                ret |= result;
            }

            //  SHA256
            if (IsSHA256Hash ?? false)
            {
                target.SHA256Hash = MonitorFunctions.GetHash(target.Path, System.Security.Cryptography.SHA256.Create());
                bool result = target.SHA256Hash != target_db.SHA256Hash;
                if (target_db.SHA256Hash != null)
                {
                    dictionary[$"{serial}_{target.PathTypeName}_SHA256Hash"] = result ?
                        string.Format("{0} -> {1}",
                            target_db.SHA256Hash,
                            target.SHA256Hash) :
                        target.SHA256Hash;
                }
                ret |= result;
            }

            //  SHA512
            if (IsSHA512Hash ?? false)
            {
                target.SHA512Hash = MonitorFunctions.GetHash(target.Path, System.Security.Cryptography.SHA512.Create());
                bool result = target.SHA512Hash != target_db.SHA512Hash;
                if (target_db.SHA512Hash != null)
                {
                    dictionary[$"{serial}_{target.PathTypeName}_SHA512Hash"] = result ?
                        string.Format("{0} -> {1}",
                            target_db.SHA512Hash,
                            target.SHA512Hash) :
                        target.SHA512Hash;
                }
                ret |= result;
            }

            //  Size
            if (IsSize ?? false)
            {
                target.Size = target.FileInfo.Length;
                bool result = target.Size != target_db.Size;
                if (target_db.Size != null)
                {
                    dictionary[$"{serial}_{target.PathTypeName}_Size"] = result ?
                        string.Format("{0}({1}) -> {2}({3})",
                            target_db.Size,
                            MonitorFunctions.ToReadableSize(target_db.Size ?? 0),
                            target.Size,
                            MonitorFunctions.ToReadableSize(target.Size ?? 0)) :
                        string.Format("{0}({1})",
                            target.Size,
                            MonitorFunctions.ToReadableSize(target.Size ?? 0));
                }
                ret |= result;
            }

            return ret;
        }

        public bool CheckDirectory(MonitorTarget target, Dictionary<string, string> dictionary, int serial, int depth)
        {
            bool ret = false;

            MonitorTarget target_db = this.Targets.ContainsKey(target.Path) ?
                this.Targets[target.Path] :
                new MonitorTarget(PathType.Directory, target.Path, "directory");

            //  CreationTime
            if (IsCreationTime ?? false)
            {
                target.CreationTime = MonitorFunctions.GetCreationTime(target.DirectoryInfo, this.IsDateOnly ?? false, this.IsTimeOnly ?? false);
                bool result = target.CreationTime != target_db.CreationTime;
                if (target_db.CreationTime != null)
                {
                    dictionary[$"{serial}_{target.PathTypeName}_CreationTime"] = result ?
                        string.Format("{0} -> {1}",
                            target_db.CreationTime,
                            target.CreationTime) :
                        target.CreationTime;
                }
                ret |= result;
            }

            //  LastWriteTime
            if (IsLastWriteTime ?? false)
            {
                target.LastWriteTime = MonitorFunctions.GetLastWriteTime(target.DirectoryInfo, this.IsDateOnly ?? false, this.IsTimeOnly ?? false);
                bool result = target.LastWriteTime != target_db.LastWriteTime;
                if (target_db.LastWriteTime != null)
                {
                    dictionary[$"{serial}_{target.PathTypeName}_LastWriteTime"] = result ?
                        string.Format("{0} -> {1}",
                            target_db.LastWriteTime,
                            target.LastWriteTime) :
                        target.LastWriteTime;
                }
                ret |= result;
            }

            //  LastAccessTime
            if (IsLastAccessTime ?? false)
            {
                target.LastAccessTime = MonitorFunctions.GetLastAccessTime(target.DirectoryInfo, this.IsDateOnly ?? false, this.IsTimeOnly ?? false);
                bool result = target.LastAccessTime != target_db.LastAccessTime;
                if (target_db.LastAccessTime != null)
                {
                    dictionary[$"{serial}_{target.PathTypeName}_LastAccessTime"] = result ?
                        string.Format("{0} -> {1}",
                            target_db.LastAccessTime,
                            target.LastAccessTime) :
                        target.LastAccessTime;
                }
                ret |= result;
            }

            //  Access
            if (IsAccess ?? false)
            {
                target.Access = AccessRuleSummary.DirectoryToAccessString(target.DirectoryInfo);
                bool result = target.Access != target_db.Access;
                if (target_db.Access != null)
                {
                    dictionary[$"{serial}_{target.PathTypeName}_Access"] = result ?
                        string.Format("{0} -> {1}",
                            target_db.Access,
                            target.Access) :
                        target.Access;
                }
                ret |= result;
            }

            //  Owner
            if (IsOwner ?? false)
            {
                target.Owner = target.DirectoryInfo.GetAccessControl().GetOwner(typeof(System.Security.Principal.NTAccount)).Value;
                bool result = target.Owner != target_db.Owner;
                if (target_db.Owner != null)
                {
                    dictionary[$"{serial}_{target.PathTypeName}_Owner"] = result ?
                        string.Format("{0} -> {1}",
                            target_db.Owner,
                            target.Owner) :
                        target.Owner;
                }
                ret |= result;
            }

            //  Inherited
            if (IsInherited ?? false)
            {
                target.Inherited = !target.DirectoryInfo.GetAccessControl().AreAccessRulesProtected;
                bool result = target.Inherited != target_db.Inherited;
                if (target_db.Inherited != null)
                {
                    dictionary[$"{serial}_{target.PathTypeName}_Inherited"] = result ?
                        string.Format("{0} -> {1}",
                            target_db.Inherited,
                            target.Inherited) :
                        target.Inherited.ToString();
                }
                ret |= result;
            }

            //  Attributes
            if (IsAttributes ?? false)
            {
                target.Attributes = MonitorFunctions.GetAttributes(target.Path);
                bool result = !target.Attributes.SequenceEqual(target_db.Attributes ?? new bool[0] { });
                if (target_db.Attributes != null)
                {
                    dictionary[$"{serial}_{target.PathTypeName}_Attributes"] = result ?
                        string.Format("{0} -> {1}",
                            MonitorFunctions.ToReadableAttributes(target_db.Attributes),
                            MonitorFunctions.ToReadableAttributes(target.Attributes)) :
                        MonitorFunctions.ToReadableAttributes(target.Attributes);
                }
                ret |= result;
            }

            //  ChildCount
            if ((IsChildCount ?? false) && depth == 0)
            {
                target.ChildCount = MonitorFunctions.GetDirectoryChildCount(target.Path);
                bool result = !target.ChildCount.SequenceEqual(target_db.ChildCount ?? new int[0] { });
                if (target_db.ChildCount != null)
                {
                    dictionary[$"{serial}_{target.PathTypeName}_ChildCount"] = result ?
                        string.Format("{0} -> {1}",
                            MonitorFunctions.ToReadableChildCount(target_db.ChildCount, target.PathType == PathType.Directory),
                            MonitorFunctions.ToReadableChildCount(target.ChildCount, target.PathType == PathType.Directory)) :
                        MonitorFunctions.ToReadableChildCount(target.ChildCount, target.PathType == PathType.Directory);
                }
                ret |= result;
            }

            return ret;
        }

        public bool CheckRegistryKey(MonitorTarget target, Dictionary<string, string> dictionary, int serial, int depth)
        {
            bool ret = false;

            MonitorTarget target_db = this.Targets.ContainsKey(target.Path) ?
                this.Targets[target.Path] :
                new MonitorTarget(PathType.Registry, target.Path, "registry", target.Key);

            //  Access
            if (IsAccess ?? false)
            {
                target.Access = AccessRuleSummary.RegistryKeyToAccessString(target.Key);
                bool result = target.Access != target_db.Access;
                if (target_db.Access != null)
                {
                    dictionary[$"{serial}_{target.PathTypeName}_Access"] = result ?
                        string.Format("{0} -> {1}",
                            target_db.Access,
                            target.Access) :
                        target.Access;
                }
                ret |= result;
            }

            //  Owner
            if (IsOwner ?? false)
            {
                target.Owner = target.Key.GetAccessControl().GetOwner(typeof(System.Security.Principal.NTAccount)).Value;
                bool result = target.Owner != target_db.Owner;
                if (target_db.Owner != null)
                {
                    dictionary[$"{serial}_{target.PathTypeName}_Owner"] = result ?
                        string.Format("{0} -> {1}",
                            target_db.Owner,
                            target.Owner) :
                        target.Owner;
                }
                ret |= result;
            }

            //  Inherited
            if (IsInherited ?? false)
            {
                target.Inherited = !target.Key.GetAccessControl().AreAccessRulesProtected;
                bool result = target.Inherited != target_db.Inherited;
                if (target_db.Inherited != null)
                {
                    dictionary[$"{serial}_{target.PathTypeName}_Inherited"] = result ?
                        string.Format("{0} -> {1}",
                            target_db.Inherited,
                            target.Inherited) :
                        target.Inherited.ToString();
                }
                ret |= result;
            }

            //  ChildCount
            if ((IsChildCount ?? false) && depth == 0)
            {
                target.ChildCount = MonitorFunctions.GetRegistryKeyChildCount(target.Key);
                bool result = !target.ChildCount.SequenceEqual(target_db.ChildCount ?? new int[0] { });
                if (target_db.ChildCount != null)
                {
                    dictionary[$"{serial}_{target.PathTypeName}_ChildCount"] = result ?
                        string.Format("{0} -> {1}",
                            MonitorFunctions.ToReadableChildCount(target_db.ChildCount, target.PathType == PathType.Directory),
                            MonitorFunctions.ToReadableChildCount(target.ChildCount, target.PathType == PathType.Directory)) :
                        MonitorFunctions.ToReadableChildCount(target.ChildCount, target.PathType == PathType.Directory);
                }
                ret |= result;
            }

            return ret;
        }

        public bool CheckRegistryValue(MonitorTarget target, Dictionary<string, string> dictionary, int serial)
        {
            bool ret = false;

            string regPath = REGPATH_PREFIX + target.Path + "\\" + target.Name;
            MonitorTarget target_db = this.Targets.ContainsKey(regPath) ?
                this.Targets[regPath] :
                new MonitorTarget(PathType.Registry, target.Path, "registry", target.Key, target.Name);

            //  MD5Hash
            if (IsMD5Hash ?? false)
            {
                target.MD5Hash = MonitorFunctions.GetHash(target.Key, target.Name, System.Security.Cryptography.MD5.Create());
                bool result = target.MD5Hash != target_db.MD5Hash;
                if (target_db.MD5Hash != null)
                {
                    dictionary[$"{serial}_{target.PathTypeName}_MD5Hash"] = result ?
                        string.Format("{0} -> {1}",
                            target_db.MD5Hash,
                            target.MD5Hash) :
                        target.MD5Hash;
                }
                ret |= result;
            }

            //  SHA256Hash
            if (IsSHA256Hash ?? false)
            {
                target.SHA256Hash = MonitorFunctions.GetHash(target.Key, target.Name, System.Security.Cryptography.SHA256.Create());
                bool result = target.SHA256Hash != target_db.SHA256Hash;
                if (target_db.SHA256Hash != null)
                {
                    dictionary[$"{serial}_{target.PathTypeName}_SHA256Hash"] = result ?
                        string.Format("{0} -> {1}",
                            target_db.SHA256Hash,
                            target.SHA256Hash) :
                        target.SHA256Hash;
                }
                ret |= result;
            }

            //  SHA512Hash
            if (IsSHA512Hash ?? false)
            {
                target.SHA512Hash = MonitorFunctions.GetHash(target.Key, target.Name, System.Security.Cryptography.SHA512.Create());
                bool result = target.SHA512Hash != target_db.SHA512Hash;
                if (target_db.SHA512Hash != null)
                {
                    dictionary[$"{serial}_{target.PathTypeName}_SHA512Hash"] = result ?
                        string.Format("{0} -> {1}",
                            target_db.SHA512Hash,
                            target.SHA512Hash) :
                        target.SHA512Hash;
                }
                ret |= result;
            }

            //  RegistryType
            if (IsRegistryType ?? false)
            {
                target.RegistryType = RegistryControl.ValueKindToString(target.Key.GetValueKind(target.Name));
                bool result = target.RegistryType != target_db.RegistryType;
                if (target_db.RegistryType != null)
                {
                    dictionary[$"{serial}_{target.PathTypeName}_RegistryType"] = result ?
                        string.Format("{0} -> {1}",
                            target_db.RegistryType,
                            target.RegistryType) :
                        target.RegistryType;
                }
                ret |= result;
            }

            return ret;
        }

        #endregion
    }
}
