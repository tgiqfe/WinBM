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

        [TaskParameter(Mandatory = true, ResolvEnv = true, Delimiter = ';')]
        [Keys("path", "directorypath", "folderpath", "dirpath", "target", "targetpath")]
        protected string[] _Path { get; set; }

        //  ################################

        [TaskParameter]
        [Keys("iscreationtime", "creationtime", "creation", "iscreationdate", "creationdate")]
        protected bool? _IsCreationTime { get; set; }

        [TaskParameter]
        [Keys("islastwritetime", "lastwritetime", "lastwrite", "islastwritedate", "lastwritedate", "modifytime", "modifydate", "modtime", "moddate")]
        protected bool? _IsLastWriteTime { get; set; }

        [TaskParameter]
        [Keys("islastaccesstime", "lastaccesstime", "lastaccess", "lastaccessdate", "lastaccess")]
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
        protected bool _IsDateOnly { get; set; }

        [TaskParameter]
        [Keys("timeonly", "time")]
        protected bool _IsTimeOnly { get; set; }

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

        private MonitorTarget CreateForFile(string path, string pathTypeName)
        {
            return new MonitorTarget(PathType.File, path)
            {
                PathTypeName = pathTypeName,
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
                IsDateOnly = _IsDateOnly,
                IsTimeOnly = _IsTimeOnly,
            };
        }

        private MonitorTarget CreateForDirectory(string path, string pathTypeName)
        {
            return new MonitorTarget(PathType.File, path)
            {
                PathTypeName = pathTypeName,
                IsCreationTime = _IsCreationTime,
                IsLastWriteTime = _IsLastWriteTime,
                IsLastAccessTime = _IsLastAccessTime,
                IsAccess = _IsAccess,
                IsOwner = _IsOwner,
                IsInherited = _IsInherited,
                IsAttributes = _IsAttributes,
                IsChildCount = _IsChildCount,
                IsDateOnly = _IsDateOnly,
                IsTimeOnly = _IsTimeOnly,
            };
        }

        public override void MainProcess()
        {
            var dictionary = new Dictionary<string, string>();
            var collection = MonitorTargetCollection.Load(GetWatchDBDirectory(), _Id);
            _MaxDepth ??= 5;

            foreach (string path in _Path)
            {
                _checkingPath = path;
                Success |= RecursiveTree(collection, dictionary, path, 0);
            }
            foreach (string uncheckedPath in collection.GetUncheckedKeys())
            {
                _serial++;
                dictionary[$"{_serial}_remove"] = uncheckedPath;
                collection.Remove(uncheckedPath);
                Success = true;
            }
            collection.Save(GetWatchDBDirectory(), _Id);
        }

        private bool RecursiveTree(MonitorTargetCollection collection, Dictionary<string, string> dictionary, string path, int depth)
        {
            bool ret = false;

            _serial++;
            dictionary[$"{_serial}_directory"] = (path == _checkingPath) ?
                path :
                path.Replace(_checkingPath, "");
            MonitorTarget target_db = _Begin ?
                CreateForDirectory(path, "directory") :
                collection.GetMonitoredTarget(path) ?? CreateForDirectory(path, "directory");

            MonitorTarget target_monitor = CreateForDirectory(path, "directory");
            target_monitor.Merge_is_Property(target_db);
            target_monitor.CheckExists();

            if (target_monitor.Exists ?? false)
            {
                ret |= WatchFunctions.CheckDirectory(target_monitor, target_db, dictionary, _serial, depth);
            }
            collection.SetMonitoredTarget(path, target_monitor);

            if (depth < _MaxDepth && (target_monitor.Exists ?? false))
            {
                foreach (string filePath in System.IO.Directory.GetFiles(path))
                {
                    _serial++;
                    dictionary[$"{_serial}_file"] = filePath.Replace(_checkingPath, "");
                    MonitorTarget target_db_leaf = _Begin ?
                        CreateForFile(filePath, "file") :
                        collection.GetMonitoredTarget(filePath) ?? CreateForFile(path, "file");

                    MonitorTarget target_monitor_leaf = CreateForDirectory(filePath, "file");
                    target_monitor_leaf.Merge_is_Property(target_db_leaf);
                    target_monitor_leaf.CheckExists();

                    if (target_monitor_leaf.Exists ?? false)
                    {
                        ret |= WatchFunctions.CheckFile(target_monitor_leaf, target_db_leaf, dictionary, _serial);
                    }
                    collection.SetMonitoredTarget(filePath, target_monitor_leaf);
                }
                foreach (string dirPath in System.IO.Directory.GetDirectories(path))
                {
                    ret |= RecursiveTree(collection, dictionary, dirPath, depth + 1);
                }
            }

            return ret;
        }



        /*
        public override void MainProcess()
        {
            _MaxDepth ??= 5;
            var dictionary = new Dictionary<string, string>();
            var collection = LoadWatchDB(_Id);

            foreach (string path in _Path)
            {
                _checkingPath = path;
                Success |= RecursiveTree(collection, dictionary, path, 0);
            }
            foreach (string uncheckedPath in collection.GetUncheckedKeys())
            {
                _serial++;
                dictionary[$"remove_{_serial}"] = uncheckedPath;
                collection.Remove(uncheckedPath);
                Success = true;
            }
            SaveWatchDB(collection, _Id);

            AddAudit(dictionary, this._Invert);
        }

        private bool RecursiveTree(WatchPathCollection collection, Dictionary<string, string> dictionary, string path, int depth)
        {
            bool ret = false;

            _serial++;
            dictionary[$"directory_{_serial}"] = (path == _checkingPath) ?
                path :
                path.Replace(_checkingPath, "");
            WatchPath watch = _Begin ?
                CreateForDirectory() :
                collection.GetWatchPath(path) ?? CreateForDirectory();
            ret |= WatchDirectoryCheck(watch, dictionary, path);
            collection.SetWatchPath(path, watch);

            if (depth < _MaxDepth)
            {
                foreach (string filePath in System.IO.Directory.GetFiles(path))
                {
                    _serial++;
                    dictionary[$"file_{_serial}"] = filePath.Replace(_checkingPath, "");
                    WatchPath childWatch = _Begin ?
                        CreateForFile() :
                        collection.GetWatchPath(filePath) ?? CreateForFile();
                    ret |= WatchFileCheck(childWatch, dictionary, filePath);
                    collection.SetWatchPath(filePath, childWatch);
                }
                foreach (string dir in System.IO.Directory.GetDirectories(path))
                {
                    ret |= RecursiveTree(collection, dictionary, dir, depth + 1);
                }
            }

            return ret;
        }

        #region Create WatchPath

        private WatchPath CreateForDirectory()
        {
            return new WatchPath(PathType.Directory)
            {
                IsCreationTime = _IsCreationTime,
                IsLastWriteTime = _IsLastWriteTime,
                IsLastAccessTime = _IsLastAccessTime,
                IsAccess = _IsAccess,
                IsOwner = _IsOwner,
                IsInherited = _IsInherited,
                IsAttributes = _IsAttributes,
                IsChildCount = _IsChildCount,
            };
        }

        private WatchPath CreateForFile()
        {
            return new WatchPath(PathType.File)
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
            };
        }

        #endregion
        #region Check WatchPath

        private bool WatchDirectoryCheck(WatchPath watch, Dictionary<string, string> dictionary, string path)
        {
            var info = new DirectoryInfo(path);
            bool ret = MonitorExists.WatchDirectory(watch, dictionary, _serial, info);
            ret |= MonitorTimeStamp.WatchDirectoryCreationTime(watch, dictionary, _serial, info);
            ret |= MonitorTimeStamp.WatchDirectoryLastWriteTime(watch, dictionary, _serial, info);
            ret |= MonitorTimeStamp.WatchDirectoryLastAccessTime(watch, dictionary, _serial, info);
            ret |= MonitorSecurity.WatchDirectoryAccess(watch, dictionary, _serial, info);
            ret |= MonitorSecurity.WatchDirectoryOwner(watch, dictionary, _serial, info);
            ret |= MonitorSecurity.WatchDirectoryInherited(watch, dictionary, _serial, info);
            ret |= MonitorAttributes.WatchDirectory(watch, dictionary, _serial, path);
            ret |= MonitorChildCount.WatchDirectory(watch, dictionary, _serial, path);
            return ret;
        }

        private bool WatchFileCheck(WatchPath watch, Dictionary<string, string> dictionary, string path)
        {
            var info = new FileInfo(path);
            bool ret = MonitorExists.WatchFile(watch, dictionary, _serial, info);
            ret |= MonitorTimeStamp.WatchFileCreationTime(watch, dictionary, _serial, info);
            ret |= MonitorTimeStamp.WatchFileLastWriteTime(watch, dictionary, _serial, info);
            ret |= MonitorTimeStamp.WatchFileLastAccessTime(watch, dictionary, _serial, info);
            ret |= MonitorSecurity.WatchFileAccess(watch, dictionary, _serial, info);
            ret |= MonitorSecurity.WatchFileOwner(watch, dictionary, _serial, info);
            ret |= MonitorSecurity.WatchFileInherited(watch, dictionary, _serial, info);
            ret |= MonitorAttributes.WatchFile(watch, dictionary, _serial, path);
            ret |= MonitorHash.WatchFileMD5Hash(watch, dictionary, _serial, path);
            ret |= MonitorHash.WatchFileSHA256Hash(watch, dictionary, _serial, path);
            ret |= MonitorHash.WatchFileSHA512Hash(watch, dictionary, _serial, path);
            ret |= MonitorSize.WatchFile(watch, dictionary, _serial, info);
            return ret;
        }

        #endregion

        */
    }
}
