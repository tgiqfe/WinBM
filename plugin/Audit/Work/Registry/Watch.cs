using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Audit.Lib;
using WinBM;
using WinBM.Task;
using IO.Lib;
using Microsoft.Win32;

namespace Audit.Work.Registry
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Watch : AuditTaskWork
    {
        [TaskParameter(Mandatory = true)]
        [Keys("id", "serial", "serialkey", "number", "uniquekey")]
        protected string _Id { get; set; }

        [TaskParameter(Mandatory = true, ResolvEnv = true, Delimiter = ';')]
        [Keys("path", "registrypath", "targetpath", "key", "registrykey", "targetkey", "regkey", "target")]
        protected string[] _Path { get; set; }

        [TaskParameter(ResolvEnv = true, Delimiter = '\n')]
        [Keys("name", "name", "namae", "registryname", "regname", "paramname")]
        protected string[] _Name { get; set; }

        //  ################################

        [TaskParameter(MandatoryAny = 1)]
        [Keys("isaccess", "access", "acl")]
        protected bool? _IsAccess { get; set; }

        [TaskParameter(MandatoryAny = 2)]
        [Keys("isowner", "owner", "own")]
        protected bool? _IsOwner { get; set; }

        [TaskParameter(MandatoryAny = 3)]
        [Keys("isinherited", "inherited", "inherit", "inheritance")]
        protected bool? _IsInherited { get; set; }

        [TaskParameter(MandatoryAny = 4)]
        [Keys("ismd5hash", "md5hash", "md5")]
        protected bool? _IsMD5Hash { get; set; }

        [TaskParameter(MandatoryAny = 5)]
        [Keys("issha256hash", "sha256hash", "sha256", "hash")]
        protected bool? _IsSHA256Hash { get; set; }

        [TaskParameter(MandatoryAny = 6)]
        [Keys("issha512hash", "sha512hash", "sha512")]
        protected bool? _IsSHA512Hash { get; set; }

        [TaskParameter(MandatoryAny = 7)]
        [Keys("ischildcount", "childcount")]
        protected bool? _IsChildCount { get; set; }

        [TaskParameter(MandatoryAny = 8)]
        [Keys("isregistrytype", "isvaluekind", "isregtype", "valuekind", "kind", "type", "registrytype", "regtype")]
        protected bool? _IsRegistryType { get; set; }

        [TaskParameter(MandatoryAny = 9)]
        [Keys("isexists", "exists", "exist")]
        protected bool? _IsExists { get; set; }

        //  ################################

        [TaskParameter]
        [Keys("begin", "start")]
        protected bool _Begin { get; set; }

        [TaskParameter]
        [Keys("maxdepth", "depth", "maxdeepth", "deepth")]
        protected int? _MaxDepth { get; set; }

        [TaskParameter]
        [Keys("invert", "not", "no", "none")]
        protected bool _Invert { get; set; }

        private int _serial;
        private string _checkingPath;
        
        public override void MainProcess()
        {
            _MaxDepth ??= 5;
            var dictionary = new Dictionary<string, string>();
            var collection = LoadWatchDB(_Id);

            if (_Name?.Length > 0)
            {
                //  レジストリ値のWatch
                string keyPath = _Path[0];
                using (RegistryKey regKey = RegistryControl.GetRegistryKey(keyPath, false, false))
                {
                    foreach (string name in _Name)
                    {
                        _serial++;
                        string regPath = WatchPath.REGPATH_PREFIX + keyPath + "\\" + name;
                        dictionary[$"registry_{_serial}"] = regPath.Replace(_checkingPath, "");
                        WatchPath watch = _Begin ?
                            CreateForRegistryValue() :
                            collection.GetWatchPath(regPath) ?? CreateForRegistryValue();
                        Success |= WatchRegistryValueCheck(watch, dictionary, regKey, name);
                        collection.SetWatchPath(regPath, watch);
                    }
                }
            }
            else
            {
                //  レジストリキーのWatch
                foreach (string path in _Path)
                {
                    _checkingPath = path;

                    using (RegistryKey regKey = RegistryControl.GetRegistryKey(path, false, false))
                    {
                        //  指定したレジストリキーが存在しない場合
                        if (regKey == null)
                        {
                            string keyPath = path;

                            _serial++;
                            dictionary[$"registry_{_serial}"] = (path == _checkingPath) ?
                                path :
                                keyPath.Replace(_checkingPath, "");
                            WatchPath watch = _Begin ?
                                CreateForRegistryKey() :
                                collection.GetWatchPath(keyPath) ?? CreateForRegistryKey();
                            Success |= WatchRegistryKeyCheck(watch, dictionary, regKey);
                            collection.SetWatchPath(keyPath, watch);
                            continue;
                        }
                        Success |= RecursiveTree(collection, dictionary, regKey, 0);
                    }
                }
                foreach (string uncheckedPath in collection.GetUncheckedKeys())
                {
                    _serial++;
                    dictionary[$"remove_{_serial}"] = uncheckedPath;
                    collection.Remove(uncheckedPath);
                    Success = true;
                }
            }
            SaveWatchDB(collection, _Id);

            AddAudit(dictionary, this._Invert);
        }

        private bool RecursiveTree(WatchPathCollection collection, Dictionary<string, string> dictionary, RegistryKey regKey, int depth)
        {
            bool ret = false;
            string keyPath = regKey.Name;

            _serial++;
            dictionary[$"registry_{_serial}"] = (regKey.Name == _checkingPath) ?
                regKey.Name :
                keyPath.Replace(_checkingPath, "");
            WatchPath watch = _Begin ?
                CreateForRegistryKey() :
                collection.GetWatchPath(keyPath) ?? CreateForRegistryKey();
            ret |= WatchRegistryKeyCheck(watch, dictionary, regKey);
            collection.SetWatchPath(keyPath, watch);

            if (depth < _MaxDepth)
            {
                foreach (string name in regKey.GetValueNames())
                {
                    _serial++;
                    string regPath = WatchPath.REGPATH_PREFIX + keyPath + "\\" + name;
                    dictionary[$"registry_{_serial}"] = regPath.Replace(_checkingPath, "");
                    WatchPath childWatch = _Begin ?
                        CreateForRegistryValue() :
                        collection.GetWatchPath(regPath) ?? CreateForRegistryValue();
                    ret |= WatchRegistryValueCheck(childWatch, dictionary, regKey, name);
                    collection.SetWatchPath(regPath, childWatch);
                }
                foreach (string key in regKey.GetSubKeyNames())
                {
                    ret |= RecursiveTree(collection, dictionary, regKey.OpenSubKey(key, false), depth + 1);
                }
            }

            return ret;
        }

        #region Create WatchPath

        /*
        private WatchPath CreateForRegistryKey(WatchPathCollection collection, string path)
        {
            if (_Begin)
            {
                return new WatchPath(PathType.Registry)
                {
                    IsAccess = _IsAccess,
                    IsOwner = _IsOwner,
                    IsInherited = _IsInherited,
                    IsChildCount = _IsChildCount,
                };
            }

            var watch = collection.GetWatchPath(path);
            if (watch == null)
            {
                return new WatchPath(PathType.Registry)
                {
                    IsAccess = _IsAccess,
                    IsOwner = _IsOwner,
                    IsInherited = _IsInherited,
                    IsChildCount = _IsChildCount,
                };
            }

            if (_IsAccess != null) { watch.IsAccess = _IsAccess; }
            if (_IsOwner != null) { watch.IsOwner = _IsOwner; }
            if (_IsInherited != null) { watch.IsInherited = _IsInherited; }
            if (_IsChildCount != null) { watch.IsChildCount = _IsChildCount; }
            return watch;
        }
        */

        private WatchPath CreateForRegistryKey()
        {
            return new WatchPath(PathType.Registry)
            {
                IsAccess = _IsAccess,
                IsOwner = _IsOwner,
                IsInherited = _IsInherited,
                IsChildCount = _IsChildCount,
            };
        }

        /*
        private WatchPath CreateForRegistryValue(WatchPathCollection collection, string path)
        {
            if (_Begin)
            {
                return new WatchPath(PathType.Registry)
                {
                    IsMD5Hash = _IsMD5Hash,
                    IsSHA256Hash = _IsSHA256Hash,
                    IsSHA512Hash = _IsSHA512Hash,
                    IsRegistryType = _IsRegistryType,
                };
            }

            var watch = collection.GetWatchPath(path);
            if (watch == null)
            {
                return new WatchPath(PathType.Registry)
                {
                    IsMD5Hash = _IsMD5Hash,
                    IsSHA256Hash = _IsSHA256Hash,
                    IsSHA512Hash = _IsSHA512Hash,
                    IsRegistryType = _IsRegistryType,
                };
            }

            if (_IsMD5Hash != null) { watch.IsMD5Hash = _IsMD5Hash; }
            if (_IsSHA256Hash != null) { watch.IsSHA256Hash = _IsSHA256Hash; }
            if (_IsSHA512Hash != null) { watch.IsSHA512Hash = _IsSHA512Hash; }
            if (_IsRegistryType != null) { watch.IsRegistryType = _IsRegistryType; }
            return watch;
        }
        */

        private WatchPath CreateForRegistryValue()
        {
            return new WatchPath(PathType.Registry)
            {
                IsMD5Hash = _IsMD5Hash,
                IsSHA256Hash = _IsSHA256Hash,
                IsSHA512Hash = _IsSHA512Hash,
                IsRegistryType = _IsRegistryType,
            };
        }

        #endregion
        #region Check WatchPath

        private bool WatchRegistryKeyCheck(WatchPath watch, Dictionary<string, string> dictionary, RegistryKey regKey)
        {
            bool ret = MonitorExists.WatchRegistryKey(watch, dictionary, _serial, regKey);
            ret |= MonitorSecurity.WatchRegistryKeyAccess(watch, dictionary, _serial, regKey);
            ret |= MonitorSecurity.WatchRegistryKeyOwner(watch, dictionary, _serial, regKey);
            ret |= MonitorSecurity.WatchRegistryKeyInherited(watch, dictionary, _serial, regKey);
            ret |= MonitorChildCount.WatchRegistryKey(watch, dictionary, _serial, regKey);

            return ret;
        }

        private bool WatchRegistryValueCheck(WatchPath watch, Dictionary<string, string> dictionary, RegistryKey regKey, string name)
        {
            bool ret = MonitorExists.WatchRegistryValue(watch, dictionary, _serial, regKey, name);
            ret |= MonitorHash.WatchRegistryValueMD5Hash(watch, dictionary, _serial, regKey, name);
            ret |= MonitorHash.WatchRegistryValueSHA256Hash(watch, dictionary, _serial, regKey, name);
            ret |= MonitorHash.WatchRegistryValueSHA512Hash(watch, dictionary, _serial, regKey, name);
            ret |= MonitorRegistryType.WatchRegistryValue(watch, dictionary, _serial, regKey, name);

            return ret;
        }

        #endregion
    }
}
