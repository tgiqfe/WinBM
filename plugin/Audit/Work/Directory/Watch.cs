﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Audit.Lib;
using WinBM;
using WinBM.Task;
using IO.Lib;
using Audit.Lib.Monitor;

namespace Audit.Work.Directory
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Watch : AuditTaskWork
    {
        [TaskParameter(Mandatory = true)]
        [Keys("id", "serial", "serialkey", "number", "uniquekey")]
        protected string _Id { get; set; }

        [TaskParameter(Resolv = true, Delimiter = ';')]
        [Keys("path", "directorypath", "folderpath", "dirpath", "target", "targetpath")]
        protected string[] _Path { get; set; }

        //  ################################

        [TaskParameter]
        [Keys("iscreationtime", "creationtime", "iscreation", "creation", "iscreationdate", "creationdate")]
        protected bool? _IsCreationTime { get; set; }

        [TaskParameter]
        [Keys("islastwritetime", "lastwritetime", "islastwrite", "lastwrite", "islastwritedate", "lastwritedate", "islastwrite", "modifytime", "modifydate", "modtime", "moddate")]
        protected bool? _IsLastWriteTime { get; set; }

        [TaskParameter]
        [Keys("islastaccesstime", "lastaccesstime", "islastaccess", "lastaccess", "islastaccessdate", "lastaccessdate")]
        protected bool? _IsLastAccessTime { get; set; }

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
        [Keys("isattributes", "isattribute", "attributes", "attribute", "attribs", "attrib", "attrs", "attr")]
        protected bool? _IsAttributes { get; set; }

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
        [Keys("issize", "size")]
        protected bool? _IsSize { get; set; }

        [TaskParameter]
        [Keys("ischildcount", "childcount")]
        protected bool? _IsChildCount { get; set; }

        //  ################################

        [TaskParameter]
        [Keys("dateonly", "date")]
        protected bool? _IsDateOnly { get; set; }

        [TaskParameter]
        [Keys("timeonly", "time")]
        protected bool? _IsTimeOnly { get; set; }

        [TaskParameter]
        [Keys("begin", "start")]
        protected bool _Begin { get; set; }

        [TaskParameter(Unsigned = true)]
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
                IsCreationTime = _IsCreationTime,
                IsLastWriteTime = _IsLastWriteTime,
                IsLastAccessTime = _IsLastAccessTime,
                IsAccess = _IsAccess,
                IsOwner = _IsOwner,
                IsInherited = _IsInherited,
                IsAttributes = _IsAttributes,
                IsMD5Hash = _IsMD5Hash,
                IsSHA256Hash = _IsSHA256Hash,
                IsSHA512Hash = _IsSHA512Hash,
                IsSize = _IsSize,
                IsChildCount = _IsChildCount,
                //IsRegistryType = _IsRegistryType,
                IsDateOnly = _IsDateOnly,
                IsTimeOnly = _IsTimeOnly,
            };
        }

        private MonitorTargetCollection MergeMonitorTargetCollection(MonitorTargetCollection collection)
        {
            if (_IsCreationTime != null) { collection.IsCreationTime = _IsCreationTime; }
            if (_IsLastWriteTime != null) { collection.IsLastWriteTime = _IsLastWriteTime; }
            if (_IsLastAccessTime != null) { collection.IsLastAccessTime = _IsLastAccessTime; }
            if (_IsAccess != null) { collection.IsAccess = _IsAccess; }
            if (_IsOwner != null) { collection.IsOwner = _IsOwner; }
            if (_IsInherited != null) { collection.IsInherited = _IsInherited; }
            if (_IsAttributes != null) { collection.IsAttributes = _IsAttributes; }
            if (_IsMD5Hash != null) { collection.IsMD5Hash = _IsMD5Hash; }
            if (_IsSHA256Hash != null) { collection.IsSHA256Hash = _IsSHA256Hash; }
            if (_IsSHA512Hash != null) { collection.IsSHA512Hash = _IsSHA512Hash; }
            if (_IsSize != null) { collection.IsSize = _IsSize; }
            if (_IsChildCount != null) { collection.IsChildCount = _IsChildCount; }
            //if(_IsRegistryType != null){collection.IsRegistryType = _IsRegistryType;}
            if (_IsDateOnly != null) { collection.IsDateOnly = _IsDateOnly; }
            if (_IsTimeOnly != null) { collection.IsTimeOnly = _IsTimeOnly; }

            if (collection.PrevPaths?.Length > 0)
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

            foreach (string path in _Path)
            {
                Success |= RecursiveTree(
                    collection,
                    new MonitorTarget(PathType.Directory, path, "directory"),
                    dictionary,
                    0);
            }
            foreach (string uncheckedPath in collection.GetUncheckedKeys())
            {
                _serial++;
                dictionary[$"{_serial}_remove"] = uncheckedPath;
                collection.Targets.Remove(uncheckedPath);
                Success = true;
            }
            collection.PrevPaths = _Path;
            collection.Save(GetWatchDBDirectory(), _Id);

            AddAudit(dictionary, this._Invert);
        }

        private bool RecursiveTree(MonitorTargetCollection collection, MonitorTarget target, Dictionary<string, string> dictionary, int depth)
        {
            bool ret = false;

            _serial++;
            dictionary[$"{_serial}_directory"] = target.Path;
            target.CheckExists();
            if (target.Exists ?? false)
            {
                ret |= collection.CheckDirectory(target, dictionary, _serial, depth);
            }
            collection.SetMonitorTarget(target.Path, target);

            if (depth < _MaxDepth && (target.Exists ?? false))
            {
                foreach (string filePath in System.IO.Directory.GetFiles(target.Path))
                {
                    _serial++;
                    dictionary[$"{_serial}_file"] = filePath;

                    MonitorTarget target_leaf = new MonitorTarget(PathType.File, filePath, "file");
                    target_leaf.CheckExists();
                    ret |= collection.CheckFile(target_leaf, dictionary, _serial);
                    collection.SetMonitorTarget(filePath, target_leaf);
                }
                foreach (string dirPath in System.IO.Directory.GetDirectories(target.Path))
                {
                    ret |= RecursiveTree(
                        collection,
                        new MonitorTarget(PathType.Directory, dirPath, "directory"),
                        dictionary,
                        depth + 1);
                }
            }

            return ret;
        }
    }
}
