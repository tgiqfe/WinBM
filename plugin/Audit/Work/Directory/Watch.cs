using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Audit.Lib;
using WinBM;
using WinBM.Task;
using IO.Lib;

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

        [TaskParameter(MandatoryAny = 1)]
        [Keys("iscreationtime", "creationtime", "creation", "iscreationdate", "creationdate")]
        protected bool? _IsCreationTime { get; set; }

        [TaskParameter(MandatoryAny = 2)]
        [Keys("islastwritetime", "lastwritetime", "lastwrite", "islastwritedate", "lastwritedate", "modifytime", "modifydate", "modtime", "moddate")]
        protected bool? _IsLastWriteTime { get; set; }

        [TaskParameter(MandatoryAny = 3)]
        [Keys("islastaccesstime", "lastaccesstime", "lastaccess", "lastaccessdate", "lastaccess")]
        protected bool? _IsLastAccessTime { get; set; }

        [TaskParameter(MandatoryAny = 4)]
        [Keys("isaccess", "access", "acl")]
        protected bool? _IsAccess { get; set; }

        [TaskParameter(MandatoryAny = 5)]
        [Keys("isowner", "owner", "own")]
        protected bool? _IsOwner { get; set; }

        [TaskParameter(MandatoryAny = 6)]
        [Keys("isinherited", "inherited", "inherit", "inheritance")]
        protected bool? _IsInherited { get; set; }

        [TaskParameter(MandatoryAny = 7)]
        [Keys("isattributes", "isattribute", "attributes", "attribute", "attribs", "attrib", "attrs", "attr")]
        protected bool? _IsAttributes { get; set; }

        [TaskParameter(MandatoryAny = 8)]
        [Keys("ismd5hash", "md5hash", "md5")]
        protected bool? _IsMD5Hash { get; set; }

        [TaskParameter(MandatoryAny = 9)]
        [Keys("issha256hash", "sha256hash", "sha256", "hash")]
        protected bool? _IsSHA256Hash { get; set; }

        [TaskParameter(MandatoryAny = 10)]
        [Keys("issha512hash", "sha512hash", "sha512")]
        protected bool? _IsSHA512Hash { get; set; }

        [TaskParameter(MandatoryAny = 11)]
        [Keys("issize", "size")]
        protected bool? _IsSize { get; set; }

        [TaskParameter(MandatoryAny = 12)]
        [Keys("ischildcount", "childcount")]
        protected bool? _IsChildCount { get; set; }

        [TaskParameter(MandatoryAny = 13)]
        [Keys("exists", "exist")]
        protected bool? _IsExists { get; set; }

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

        private WatchPath CreateForDirectory(WatchPathCollection collection, string path)
        {
            if (_Begin)
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

            var watch = collection.GetWatchPath(path);
            if (watch == null)
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

            if (_IsCreationTime != null) { watch.IsCreationTime = _IsCreationTime; }
            if (_IsLastWriteTime != null) { watch.IsLastWriteTime = _IsLastWriteTime; }
            if (_IsLastAccessTime != null) { watch.IsLastAccessTime = _IsLastAccessTime; }
            if (_IsAccess != null) { watch.IsAccess = _IsAccess; }
            if (_IsOwner != null) { watch.IsOwner = _IsOwner; }
            if (_IsInherited != null) { watch.IsInherited = _IsInherited; }
            if (_IsAttributes != null) { watch.IsAttributes = _IsAttributes; }
            if (_IsChildCount != null) { watch.IsChildCount = _IsChildCount; }
            return watch;
        }

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

        private WatchPath CreateForFile(WatchPathCollection collection, string path)
        {
            if (_Begin)
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

            var watch = collection.GetWatchPath(path);
            if (watch == null)
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

            if (_IsCreationTime != null) { watch.IsCreationTime = _IsCreationTime; }
            if (_IsLastWriteTime != null) { watch.IsLastWriteTime = _IsLastWriteTime; }
            if (_IsLastAccessTime != null) { watch.IsLastAccessTime = _IsLastAccessTime; }
            if (_IsAccess != null) { watch.IsAccess = _IsAccess; }
            if (_IsOwner != null) { watch.IsOwner = _IsOwner; }
            if (_IsInherited != null) { watch.IsInherited = _IsInherited; }
            if (_IsAttributes != null) { watch.IsAttributes = _IsAttributes; }
            if (_IsMD5Hash != null) { watch.IsMD5Hash = _IsMD5Hash; }
            if (_IsSHA256Hash != null) { watch.IsSHA256Hash = _IsSHA256Hash; }
            if (_IsSHA512Hash != null) { watch.IsSHA512Hash = _IsSHA512Hash; }
            if (_IsSize != null) { watch.IsSize = _IsSize; }
            return watch;
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
    }
}
