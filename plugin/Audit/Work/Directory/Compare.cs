using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using System.IO;
using System.Security.Principal;
using System.Security.AccessControl;
using System.Security.Cryptography;
using IO.Lib;
using Audit.Lib.Monitor;

namespace Audit.Work.Directory
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Compare : AuditTaskWork
    {
        [TaskParameter(Mandatory = true, ResolvEnv = true)]
        [Keys("directorya", "directory1", "dira", "dir2", "sourcepath", "srcpath", "src", "source", "path", "directorypath", "patha")]
        protected string _PathA { get; set; }

        [TaskParameter(Mandatory = true, ResolvEnv = true)]
        [Keys("directoryb", "directory2", "dirb", "dir2", "destinationpath", "dstpath", "dst", "destination", "pathb")]
        protected string _PathB { get; set; }

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
        [Keys("ischildcount", "childcount")]
        protected bool? _IsChildCount { get; set; }

        [TaskParameter(MandatoryAny = 12)]
        [Keys("issize", "size")]
        protected bool? _IsSize { get; set; }

        /// <summary>
        /// 存在チェックのみに使用するパラメータ。その他の比較処理の過程で確認できる為、Exists用の特別な作業は無し
        /// </summary>
        [TaskParameter(MandatoryAny = 13)]
        [Keys("isexists", "exists", "exist")]
        protected bool? _IsExists { get; set; }

        //  ################################

        /// <summary>
        /// IsCreationTime,IsLastWriteTime,IsAccessTimeのいずれかを指定した場合のみ使用
        /// 日付のみを判定する
        /// </summary>
        [TaskParameter]
        [Keys("dateonly", "date")]
        protected bool _IsDateOnly { get; set; }

        /// IsCreationTime,IsLastWriteTime,IsAccessTimeのいずれかを指定した場合のみ使用
        /// 時間のみを判定する。
        /// IsDateOnly,IsTimeOnlyの両方を指定した場合は、IsDateOnlyを優先する。
        [TaskParameter]
        [Keys("timeonly", "time")]
        protected bool _IsTimeOnly { get; set; }

        [TaskParameter]
        [Keys("maxdepth", "depth", "maxdeepth", "deepth")]
        protected int? _MaxDepth { get; set; }

        [TaskParameter]
        [Keys("invert", "not", "no", "none")]
        protected bool _Invert { get; set; }

        private int _serial = 0;

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
            return new MonitorTarget(PathType.Directory, path)
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
            //  MaxDepth無指定の場合は[5]をセット
            _MaxDepth ??= 5;

            var dictionary = new Dictionary<string, string>();
            dictionary["directoryA"] = _PathA;
            dictionary["directoryB"] = _PathB;
            this.Success = true;

            Success &= RecursiveTree(
                CreateForDirectory(_PathA, "directoryA"),
                CreateForDirectory(_PathB, "directoryB"),
                dictionary,
                0);

            //Success &= RecursiveTree(_PathA, _PathB, dictionary, 0);

            AddAudit(dictionary, this._Invert);
        }

        private bool RecursiveTree(MonitorTarget targetA, MonitorTarget targetB, Dictionary<string, string> dictionary, int depth)
        {
            bool ret = true;

            _serial++;
            targetA.CheckExists();
            targetB.CheckExists();
            if ((targetA.Exists ?? false) && (targetB.Exists ?? false))
            {
                dictionary[$"{_serial}_directoryA_Exists"] = targetA.Path;
                dictionary[$"{_serial}_directoryB_Exists"] = targetB.Path;
                ret &= CompareFunctions.CheckDirectory(targetA, targetB, dictionary, _serial, depth);

                if (depth < _MaxDepth)
                {
                    foreach (string childPathA in System.IO.Directory.GetFiles(targetA.Path))
                    {
                        _serial++;
                        string childPathB = Path.Combine(targetB.Path, Path.GetFileName(childPathA));
                        MonitorTarget targetA_leaf = CreateForFile(childPathA, "fileA");
                        MonitorTarget targetB_leaf = CreateForFile(childPathB, "fileB");
                        targetA_leaf.CheckExists();
                        targetB_leaf.CheckExists();

                        if (targetB_leaf.Exists ?? false)
                        {
                            dictionary[$"{_serial}_fileA_Exists"] = childPathA;
                            dictionary[$"{_serial}_fileB_Exists"] = childPathB;
                            ret &= CompareFunctions.CheckFile(targetA_leaf, targetB_leaf, dictionary, _serial);
                        }
                        else
                        {
                            dictionary[$"{_serial}_fileB_NotExists"] = childPathB;
                            ret = false;
                        }
                    }
                    foreach (string childPath in System.IO.Directory.GetDirectories(targetA.Path))
                    {
                        ret &= RecursiveTree(
                            CreateForDirectory(childPath, "directoryA"),
                            CreateForDirectory(Path.Combine(targetB.Path, Path.GetFileName(childPath)), "directoryB"),
                            dictionary,
                            depth + 1);
                    }
                }
            }
            else
            {
                if (!targetA.Exists ?? false)
                {
                    dictionary[$"{_serial}_directoryA_NotExists"] = targetA.Path;
                    ret = false;
                }
                if (!targetB.Exists ?? false)
                {
                    dictionary[$"{_serial}_directoryB_NotExists"] = targetB.Path;
                    ret = false;
                }
            }



            return ret;
        }

        /*
        private bool RecursiveTree(string pathA, string pathB, Dictionary<string, string> dictionary, int depth)
        {
            bool ret = true;

            _serial++;
            MonitorTarget targetA = CreateForDirectory(pathA, "directoryA");
            MonitorTarget targetB = CreateForDirectory(pathB, "directoryB");
            targetA.CheckExists();
            targetB.CheckExists();
            if ((targetA.Exists ?? false) && (targetB.Exists ?? false))
            {
                dictionary[$"{_serial}_directoryA_Exists"] = pathA;
                dictionary[$"{_serial}_directoryB_Exists"] = pathB;
                ret &= CompareFunctions.CheckDirectory(targetA, targetB, dictionary, _serial, depth);

                if (depth < _MaxDepth)
                {
                    foreach (string childPathA in System.IO.Directory.GetFiles(pathA))
                    {
                        _serial++;
                        string childPathB = Path.Combine(pathB, Path.GetFileName(childPathA));
                        MonitorTarget targetA_leaf = CreateForFile(childPathA, "fileA");
                        MonitorTarget targetB_leaf = CreateForFile(childPathB, "fileB");
                        targetA_leaf.CheckExists();
                        targetB_leaf.CheckExists();

                        if (targetB_leaf.Exists ?? false)
                        {
                            dictionary[$"{_serial}_fileA_Exists"] = childPathA;
                            dictionary[$"{_serial}_fileB_Exists"] = childPathB;
                            ret &= CompareFunctions.CheckFile(targetA_leaf, targetB_leaf, dictionary, _serial);
                        }
                        else
                        {
                            dictionary[$"{_serial}_fileB_NotExists"] = childPathB;
                            ret = false;
                        }
                    }
                    foreach (string childPath in System.IO.Directory.GetDirectories(pathA))
                    {
                        ret &= RecursiveTree(
                            childPath,
                            Path.Combine(pathB, Path.GetFileName(childPath)),
                            dictionary,
                            depth + 1);
                    }
                }
            }
            else
            {
                if (!targetA.Exists ?? false)
                {
                    dictionary[$"{_serial}_directoryA_NotExists"] = pathA;
                    ret = false;
                }
                if (!targetB.Exists ?? false)
                {
                    dictionary[$"{_serial}_directoryB_NotExists"] = pathB;
                    ret = false;
                }
            }

            return ret;
        }
        */
    }
}
