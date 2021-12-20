﻿using System;
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

        [TaskParameter(Mandatory = true, ResolvEnv = true, Delimiter = ';')]
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

        private int _serial;

        private MonitorTarget CreateForRegistryKey(string path, RegistryKey key, string pathTypeName)
        {
            return new MonitorTarget(PathType.Registry, path, key)
            {
                PathTypeName = pathTypeName,
                IsAccess = _IsAccess,
                IsOwner = _IsOwner,
                IsInherited = _IsInherited,
                IsChildCount = _IsChildCount,
            };
        }

        private MonitorTarget CreateForRegistryValue(string path, RegistryKey key, string name, string pathTypeName)
        {
            return new MonitorTarget(PathType.Registry, path, key, name)
            {
                PathTypeName = pathTypeName,
                IsMD5Hash = _IsMD5Hash,
                IsSHA256Hash = _IsSHA256Hash,
                IsSHA512Hash = _IsSHA512Hash,
                IsRegistryType = _IsRegistryType,
            };
        }

        public override void MainProcess()
        {
            var dictionary = new Dictionary<string, string>();
            var collection = _Begin ?
                new MonitorTargetCollection() :
                MonitorTargetCollection.Load(GetWatchDBDirectory(), _Id);
            this._MaxDepth ??= 5;
            this.Success = _Begin || (collection.Count == 0);

            if (_Name?.Length > 0)
            {
                string keyPath = _Path[0];
                using (RegistryKey regKey = RegistryControl.GetRegistryKey(keyPath, false, false))
                {
                    foreach (string name in _Name)
                    {
                        _serial++;
                        dictionary[$"{_serial}_registry"] = regKey.Name + "\\" + name;
                        MonitorTarget target_dbfs =
                            collection.GetMonitorTarget(keyPath, name) ?? CreateForRegistryValue(keyPath, regKey, name, "registry");

                        MonitorTarget target_monitorfs = CreateForRegistryValue(keyPath, regKey, name, "registry");
                        target_monitorfs.Merge_is_Property(target_dbfs);
                        target_monitorfs.CheckExists();

                        if (target_monitorfs.Exists ?? false)
                        {
                            Success |= WatchFunctions.CheckRegistryValue(target_monitorfs, target_dbfs, dictionary, _serial);
                        }
                        collection.SetMonitorTarget(keyPath, name, target_monitorfs);
                    }
                }
            }
            else
            {
                foreach (string path in _Path)
                {
                    using (RegistryKey regKey = RegistryControl.GetRegistryKey(path, false, false))
                    {
                        Success |= RecursiveTree(
                            collection,
                            CreateForRegistryKey(path, regKey, "registry"),
                            dictionary,
                            0);
                    }
                }
                foreach (string uncheckedPath in collection.GetUncheckedKeys())
                {
                    _serial++;
                    dictionary[$"{_serial}_remove"] = uncheckedPath;
                    collection.Remove(uncheckedPath);
                    Success = true;
                }
            }
            collection.Save(GetWatchDBDirectory(), _Id);

            AddAudit(dictionary, this._Invert);
        }

        private bool RecursiveTree(MonitorTargetCollection collection, MonitorTarget target_monitor, Dictionary<string, string> dictionary, int depth)
        {
            bool ret = false;

            _serial++;
            dictionary[$"{_serial}_registry"] = target_monitor.Path;
            MonitorTarget target_db =
                collection.GetMonitorTarget(target_monitor.Path) ?? CreateForRegistryKey(target_monitor.Path, target_monitor.Key, "registry");

            target_monitor.Merge_is_Property(target_db);
            target_monitor.CheckExists();

            if (target_monitor.Exists ?? false)
            {
                ret |= WatchFunctions.CheckRegistrykey(target_monitor, target_db, dictionary, _serial, depth);
            }
            collection.SetMonitorTarget(target_monitor.Path, target_monitor);

            if (depth < _MaxDepth && (target_monitor.Exists ?? false))
            {
                foreach (string name in target_monitor.Key.GetValueNames())
                {
                    _serial++;
                    dictionary[$"{_serial}_registry"] = target_monitor.Path + "\\" + name;
                    MonitorTarget target_db_leaf =
                        collection.GetMonitorTarget(target_monitor.Path, name) ?? CreateForRegistryValue(target_monitor.Path, target_monitor.Key, name, "registry");

                    MonitorTarget target_monitor_leaf = CreateForRegistryValue(target_monitor.Path, target_monitor.Key, name, "registry");
                    target_monitor_leaf.Merge_is_Property(target_db_leaf);
                    target_monitor_leaf.CheckExists();

                    ret |= WatchFunctions.CheckRegistryValue(target_monitor_leaf, target_db_leaf, dictionary, _serial);
                    collection.SetMonitorTarget(target_monitor_leaf.Path, name, target_monitor_leaf);
                }
                foreach (string keyPath in target_monitor.Key.GetSubKeyNames())
                {
                    using (RegistryKey subRegKey = target_monitor.Key.OpenSubKey(keyPath, false))
                    {
                        ret |= RecursiveTree(
                            collection,
                            CreateForRegistryKey(Path.Combine(target_monitor.Path, keyPath), subRegKey, "registry"),
                            dictionary,
                            depth + 1);
                    }
                }
            }

            return ret;
        }
    }
}
