using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Audit.Lib;
using WinBM;
using WinBM.Task;
using IO.Lib;

namespace Audit.Work.File
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Watch : AuditTaskWork
    {
        [TaskParameter(Mandatory = true)]
        [Keys("id", "serial", "serialkey", "number", "uniquekey")]
        protected string _Id { get; set; }

        [TaskParameter(Mandatory = true, ResolvEnv = true, Delimiter = ';')]
        [Keys("path", "filepath", "target", "targetpath")]
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
        [Keys("isexists", "exists", "exist")]
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
        [Keys("invert", "not", "no", "none")]
        protected bool _Invert { get; set; }

        private int _serial;

        public override void MainProcess()
        {
            var dictionary = new Dictionary<string, string>();
            var collection = LoadWatchDB(_Id);

            foreach (string path in _Path)
            {
                _serial++;
                dictionary[$"file_{_serial}"] = path;
                WatchPath watch = _Begin ?
                    CreateForFile() :
                    collection.GetWatchPath(path) ?? CreateForFile();
                Success |= WatchFileCheck(watch, dictionary, path);
                collection.SetWatchPath(path, watch);
            }
            SaveWatchDB(collection, _Id);

            AddAudit(dictionary, this._Invert);
        }

        #region Create WatchPath

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
