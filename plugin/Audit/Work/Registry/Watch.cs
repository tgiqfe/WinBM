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
using Audit.Lib.Monitor;

namespace Audit.Work.Registry
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Watch : AuditTaskWork
    {
        [TaskParameter(Mandatory = true)]
        [Keys("id", "serial", "serialkey", "number", "uniquekey")]
        protected string _Id { get; set; }

        [TaskParameter(ResolvEnv = true, Delimiter = ';')]
        [Keys("path", "registrypath", "targetpath", "key", "registrykey", "targetkey", "regkey", "target")]
        protected string[] _Path { get; set; }

        [TaskParameter(ResolvEnv = true, Delimiter = '\n')]
        [Keys("name", "name", "namae", "registryname", "regname", "paramname")]
        protected string[] _Name { get; set; }

        //  ################################

        [TaskParameter]
        [Keys("isaccess", "access", "acl")]
        protected bool? _IsAccess { get; set; }

        [TaskParameter]
        [Keys("isowner", "owner", "own")]
        protected bool? _IsOwner { get; set; }

        [TaskParameter]
        [Keys("isinherited", "inherited", "inherit", "inheritance")]
        protected bool? _IsInherited { get; set; }

        [TaskParameter]
        [Keys("ismd5hash", "md5hash", "md5")]
        protected bool? _IsMD5Hash { get; set; }

        [TaskParameter]
        [Keys("issha256hash", "sha256hash", "sha256", "hash")]
        protected bool? _IsSHA256Hash { get; set; }

        [TaskParameter]
        [Keys("issha512hash", "sha512hash", "sha512")]
        protected bool? _IsSHA512Hash { get; set; }

        [TaskParameter]
        [Keys("ischildcount", "childcount")]
        protected bool? _IsChildCount { get; set; }

        [TaskParameter]
        [Keys("isregistrytype", "isvaluekind", "isregtype", "valuekind", "kind", "type", "registrytype", "regtype")]
        protected bool? _IsRegistryType { get; set; }

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

        private int _serial = 0;

        private MonitorTargetCollection CreateMonitorTargetCollection()
        {
            return new MonitorTargetCollection()
            {
                //IsCreationTime = _IsCreationTime,
                //IsLastWriteTime = _IsLastWriteTime,
                //IsLastAccessTime = _IsLastAccessTime,
                IsAccess = _IsAccess,
                IsOwner = _IsOwner,
                IsInherited = _IsInherited,
                //IsAttributes = _IsAttributes,
                IsMD5Hash = _IsMD5Hash,
                IsSHA256Hash = _IsSHA256Hash,
                IsSHA512Hash = _IsSHA512Hash,
                //IsSize = _IsSize,
                IsChildCount = _IsChildCount,
                IsRegistryType = _IsRegistryType,
                //IsDateOnly = _IsDateOnly,
                //IsTimeOnly = _IsTimeOnly,
            };
        }

        private MonitorTargetCollection MergeMonitorTargetCollection(MonitorTargetCollection collection)
        {
            //if (_IsCreationTime != null) { collection.IsCreationTime = _IsCreationTime; }
            //if (_IsLastWriteTime != null) { collection.IsLastWriteTime = _IsLastWriteTime; }
            //if (_IsLastAccessTime != null) { collection.IsLastAccessTime = _IsLastAccessTime; }
            if (_IsAccess != null) { collection.IsAccess = _IsAccess; }
            if (_IsOwner != null) { collection.IsOwner = _IsOwner; }
            if (_IsInherited != null) { collection.IsInherited = _IsInherited; }
            //if (_IsAttributes != null) { collection.IsAttributes = _IsAttributes; }
            if (_IsMD5Hash != null) { collection.IsMD5Hash = _IsMD5Hash; }
            if (_IsSHA256Hash != null) { collection.IsSHA256Hash = _IsSHA256Hash; }
            if (_IsSHA512Hash != null) { collection.IsSHA512Hash = _IsSHA512Hash; }
            //if (_IsSize != null) { collection.IsSize = _IsSize; }
            if (_IsChildCount != null) { collection.IsChildCount = _IsChildCount; }
            if (_IsRegistryType != null) { collection.IsRegistryType = _IsRegistryType; }
            //if (_IsDateOnly != null) { collection.IsDateOnly = _IsDateOnly; }
            //if (_IsTimeOnly != null) { collection.IsTimeOnly = _IsTimeOnly; }

            if (collection.PrevNames?.Length > 0)
            {
                if (collection.PrevPaths?.Length > 0)
                {
                    this._Path = collection.PrevPaths;
                }
                var tempNames = collection.PrevNames.ToList();
                if (_Name?.Length > 0)
                {
                    tempNames.AddRange(_Name);
                }
                this._Name = tempNames.Distinct().ToArray();
            }
            else if (collection.PrevPaths?.Length > 0)
            {
                var tempPaths = collection.PrevPaths.ToList();
                if (_Path?.Length > 0)
                {
                    tempPaths.AddRange(_Path);
                }
                this._Path = tempPaths.Distinct().ToArray();
            }

            return collection;
        }

        public override void MainProcess()
        {
            var dictionary = new Dictionary<string, string>();
            var collection = _Begin ?
                CreateMonitorTargetCollection() :
                MergeMonitorTargetCollection(MonitorTargetCollection.Load(GetWatchDBDirectory(), _Id));
            this._MaxDepth ??= 5;

            if (_Begin || (collection.Targets.Count == 0))
            {
                this.Success = true;
                if (_Path == null || _Path.Length == 0)
                {
                    Manager.WriteLog(LogLevel.Error, "Failed parameter, Path parameter is required.");
                    return;
                }
            }

            if (_Name?.Length > 0)
            {
                string keyPath = _Path[0];
                using (RegistryKey regKey = RegistryControl.GetRegistryKey(keyPath, false, false))
                {
                    foreach (string name in _Name)
                    {
                        _serial++;
                        dictionary[$"{_serial}_registry"] = regKey.Name + "\\" + name;

                        MonitorTarget target_leaf = new MonitorTarget(PathType.Registry, keyPath, "registry", regKey, name);
                        target_leaf.CheckExists();
                        if (target_leaf.Exists ?? false)
                        {
                            Success |= collection.CheckRegistryValue(target_leaf, dictionary, _serial);
                        }
                        collection.SetMonitorTarget(keyPath, name, target_leaf);
                    }
                }
                collection.PrevPaths = _Path;
                collection.PrevNames = _Name;
            }
            else
            {
                foreach (string path in _Path)
                {
                    using (RegistryKey regKey = RegistryControl.GetRegistryKey(path, false, false))
                    {
                        Success |= RecursiveTree(
                            collection,
                            new MonitorTarget(PathType.Registry, path, "registry", regKey),
                            dictionary,
                            0);
                    }
                }
                foreach (string uncheckedPath in collection.GetUncheckedKeys())
                {
                    _serial++;
                    dictionary[$"{_serial}_remove"] = uncheckedPath;
                    collection.Targets.Remove(uncheckedPath);
                    Success = true;
                }
                collection.PrevPaths = _Path;
            }
            collection.Save(GetWatchDBDirectory(), _Id);

            AddAudit(dictionary, this._Invert);
        }

        private bool RecursiveTree(MonitorTargetCollection collection, MonitorTarget target, Dictionary<string, string> dictionary, int depth)
        {
            bool ret = false;

            _serial++;
            dictionary[$"{_serial}_registry"] = target.Path;
            target.CheckExists();
            if (target.Exists ?? false)
            {
                ret |= collection.CheckRegistryKey(target, dictionary, _serial, depth);
            }
            collection.SetMonitorTarget(target.Path, target);

            if (depth < _MaxDepth && (target.Exists ?? false))
            {
                foreach (string name in target.Key.GetValueNames())
                {
                    _serial++;
                    dictionary[$"{_serial}_registry"] = target.Path + "\\" + name;

                    MonitorTarget target_leaf = new MonitorTarget(PathType.Registry, target.Path, "registry", target.Key, name);
                    target_leaf.CheckExists();
                    ret |= collection.CheckRegistryValue(target_leaf, dictionary, _serial);
                    collection.SetMonitorTarget(target_leaf.Path, name, target_leaf);
                }
                foreach (string keyPath in target.Key.GetSubKeyNames())
                {
                    using (RegistryKey subRegKey = target.Key.OpenSubKey(keyPath, false))
                    {
                        ret |= RecursiveTree(
                            collection,
                            new MonitorTarget(PathType.Registry, Path.Combine(target.Path, keyPath), "registry", subRegKey),
                            dictionary,
                            depth + 1);
                    }
                }
            }

            return ret;
        }
    }
}
