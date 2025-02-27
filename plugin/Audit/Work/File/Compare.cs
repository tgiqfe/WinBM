﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Security.Cryptography;
using IO.Lib;
using Audit.Lib.Monitor;

namespace Audit.Work.File
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Compare : AuditTaskWork
    {
        [TaskParameter(Mandatory = true, Resolv = true)]
        [Keys("filea", "file1", "sourcepath", "srcpath", "src", "source", "path", "filepath", "patha")]
        protected string _PathA { get; set; }

        [TaskParameter(Mandatory = true, Resolv = true)]
        [Keys("fileb", "file2", "destinationpath", "dstpath", "dst", "destination", "pathb")]
        protected string _PathB { get; set; }

        //  ################################

        [TaskParameter(MandatoryAny = 1)]
        [Keys("iscreationtime", "creationtime", "iscreation", "creation", "iscreationdate", "creationdate")]
        protected bool? _IsCreationTime { get; set; }

        [TaskParameter(MandatoryAny = 2)]
        [Keys("islastwritetime", "lastwritetime", "islastwrite", "lastwrite", "islastwritedate", "lastwritedate", "islastwrite", "modifytime", "modifydate", "modtime", "moddate")]
        protected bool? _IsLastWriteTime { get; set; }

        [TaskParameter(MandatoryAny = 3)]
        [Keys("islastaccesstime", "lastaccesstime", "islastaccess", "lastaccess", "islastaccessdate", "lastaccessdate")]
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

        /// <summary>
        /// 存在チェックのみに使用するパラメータ。その他の比較処理の過程で確認できる為、Exists用の特別な作業は無し
        /// </summary>
        [TaskParameter(MandatoryAny = 12)]
        [Keys("isexists", "exists", "exist")]
        protected bool? _IsExists { get; set; }

        //  ################################

        /// <summary>
        /// IsCreationTime,IsLastWriteTime,IsAccessTimeのいずれかを指定した場合のみ使用
        /// 日付のみを判定する
        /// </summary>
        [TaskParameter]
        [Keys("dateonly", "date")]
        protected bool? _IsDateOnly { get; set; }

        /// IsCreationTime,IsLastWriteTime,IsAccessTimeのいずれかを指定した場合のみ使用
        /// 時間のみを判定する。
        /// IsDateOnly,IsTimeOnlyの両方を指定した場合は、IsDateOnlyを優先する。
        [TaskParameter]
        [Keys("timeonly", "time")]
        protected bool? _IsTimeOnly { get; set; }

        [TaskParameter]
        [Keys("invert", "not", "no", "none")]
        protected bool _Invert { get; set; }

        private int _serial = 1;

        private MonitorTargetPair CreateMonitorTargetPair(MonitorTarget targetA, MonitorTarget targetB)
        {
            return new MonitorTargetPair(targetA, targetB)
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
                //IsChildCount = _IsChildCount,
                //IsRegistryType = _IsRegistryType,
                IsDateOnly = _IsDateOnly,
                IsTimeOnly = _IsTimeOnly,
            };
        }

        public override void MainProcess()
        {
            var dictionary = new Dictionary<string, string>();
            dictionary["fileA"] = _PathA;
            dictionary["fileB"] = _PathB;
            this.Success = true;

            MonitorTarget targetA = new MonitorTarget(PathType.File, _PathA, "fileA");
            MonitorTarget targetB = new MonitorTarget(PathType.File, _PathB, "fileB");
            targetA.CheckExists();
            targetB.CheckExists();

            if ((targetA.Exists ?? false) && (targetB.Exists ?? false))
            {
                dictionary["fileA_Exists"] = _PathA;
                dictionary["fileB_Exists"] = _PathB;

                var targetPair = CreateMonitorTargetPair(targetA, targetB);
                Success &= targetPair.CheckFile(dictionary, _serial);
            }
            else
            {
                if (!targetA.Exists ?? false)
                {
                    dictionary["fileA_NotExists"] = _PathA;
                    Success = false;
                }
                if (!targetB.Exists ?? false)
                {
                    dictionary["fileB_NotExists"] = _PathB;
                    Success = false;
                }
            }

            AddAudit(dictionary, this._Invert);
        }
    }
}
