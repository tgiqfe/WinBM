using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using Microsoft.Win32;
using IO.Lib;
using System.Security.Cryptography;
using System.Security.AccessControl;
using System.Security.Principal;
using Audit.Lib.Monitor;

namespace Audit.Work.Registry
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Compare : AuditTaskWork
    {
        [TaskParameter(Mandatory = true, ResolvEnv = true)]
        [Keys("keya", "key1", "registrykeya", "registrykey2", "registrya", "registry1", "rega", "reg1", "sourcepath", "srcpath", "src", "source", "path", "registrypath", "patha")]
        protected string _PathA { get; set; }

        [TaskParameter(Mandatory = true, ResolvEnv = true)]
        [Keys("keyb", "key2", "registrykeyb", "registrykey2", "registryb", "registry2", "regb", "reg2", "destinationpath", "dstpath", "dst", "destination", "pathb")]
        protected string _PathB { get; set; }

        [TaskParameter(ResolvEnv = true)]
        [Keys("namea", "name1", "sourcename", "srcname", "registryname", "regname", "paramname")]
        protected string _NameA { get; set; }

        [TaskParameter(ResolvEnv = true)]
        [Keys("nameb", "name2", "destinationname", "dstname")]
        protected string _NameB { get; set; }

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

        /// <summary>
        /// 存在チェックのみに使用するパラメータ。その他の比較処理の過程で確認できる為、Exists用の特別な作業は無し
        /// </summary>
        [TaskParameter(MandatoryAny = 9)]
        [Keys("isexists", "exists", "exist")]
        protected bool? _IsExists { get; set; }

        //  ################################

        [TaskParameter(Unsigned = true)]
        [Keys("maxdepth", "depth", "maxdeepth", "deepth")]
        protected int? _MaxDepth { get; set; }

        [TaskParameter]
        [Keys("invert", "not", "no", "none")]
        protected bool _Invert { get; set; }

        private int _serial = 0;

        private MonitorTargetPair CreateMonitorTargetPair(MonitorTarget targetA, MonitorTarget targetB)
        {
            return new MonitorTargetPair(targetA, targetB)
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

        public override void MainProcess()
        {
            //  MaxDepth無指定の場合は[5]をセット
            _MaxDepth ??= 5;

            var dictionary = new Dictionary<string, string>();
            this.Success = true;

            if ((_NameA == null && _NameB != null) || (_NameA != null && _NameB == null))
            {
                Manager.WriteLog(LogLevel.Error, "Failed parameter, Both name parameter required.");
                return;
            }

            if (_NameA != null && _NameB != null)
            {
                _serial++;
                using (RegistryKey keyA = RegistryControl.GetRegistryKey(_PathA, false, false))
                using (RegistryKey keyB = RegistryControl.GetRegistryKey(_PathB, false, false))
                {
                    MonitorTarget targetA = new MonitorTarget(PathType.Registry, _PathA, "registryA", keyA, _NameA);
                    MonitorTarget targetB = new MonitorTarget(PathType.Registry, _PathB, "registryB", keyB, _NameB);
                    targetA.CheckExists();
                    targetB.CheckExists();

                    if ((targetA.Exists ?? false) && (targetB.Exists ?? false))
                    {
                        dictionary[$"{_serial}_registryA_Exists"] = _PathA + "\\" + _NameA;
                        dictionary[$"{_serial}_registryB_Exists"] = _PathB + "\\" + _NameB;

                        var targetPair = CreateMonitorTargetPair(targetA, targetB);
                        Success &= targetPair.CheckRegistryValue(dictionary, _serial);
                    }
                    else
                    {
                        if (!targetA.Exists ?? false)
                        {
                            dictionary["registryA_NotExists"] = _PathA;
                            Success = false;
                        }
                        if (!targetB.Exists ?? false)
                        {
                            dictionary["registryB_NotExists"] = _PathB;
                            Success = false;
                        }
                    }
                }
            }
            else
            {
                using (RegistryKey keyA = RegistryControl.GetRegistryKey(_PathA, false, false))
                using (RegistryKey keyB = RegistryControl.GetRegistryKey(_PathB, false, false))
                {
                    Success &= RecursiveTree(
                        new MonitorTarget(PathType.Registry, _PathA, "registryA", keyA),
                        new MonitorTarget(PathType.Registry, _PathB, "registryB", keyB),
                         dictionary,
                         0);
                }
            }

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
                dictionary[$"{_serial}_registryA_Exists"] = targetA.Path;
                dictionary[$"{_serial}_registryB_Exists"] = targetB.Path;
                ret &= CompareFunctions.CheckRegistryKey(targetA, targetB, dictionary, _serial, depth);

                if (depth < _MaxDepth)
                {
                    var leaves = targetA.Key.GetValueNames().ToList();
                    leaves.AddRange(targetB.Key.GetValueNames());
                    foreach (string childName in leaves.Distinct())
                    {
                        _serial++;
                        MonitorTarget targetA_leaf = new MonitorTarget(PathType.Registry, targetA.Path, "registryA", targetA.Key, childName);
                        MonitorTarget targetB_leaf = new MonitorTarget(PathType.Registry, targetB.Path, "registryB", targetB.Key, childName);
                        targetA_leaf.CheckExists();
                        targetB_leaf.CheckExists();

                        if ((targetA_leaf.Exists ?? false) && (targetB_leaf.Exists ?? false))
                        {
                            dictionary[$"{_serial}_registryA_Exists"] = targetA_leaf.Path + "\\" + targetA_leaf.Name;
                            dictionary[$"{_serial}_registryB_Exists"] = targetB_leaf.Path + "\\" + targetB_leaf.Name;
                            ret &= CompareFunctions.CheckRegistryValue(targetA_leaf, targetB_leaf, dictionary, _serial);
                        }
                        else
                        {
                            if (targetA_leaf.Exists ?? false)
                            {
                                dictionary[$"{_serial}_registryA_NotExists"] = targetA_leaf.Path + "\\" + targetA_leaf.Name;
                                ret = false;
                            }
                            if (targetB_leaf.Exists ?? false)
                            {
                                dictionary[$"{_serial}_registryB_NotExists"] = targetB_leaf.Path + "\\" + targetB_leaf.Name;
                                ret = false;
                            }
                        }
                    }

                    var containers = targetA.Key.GetSubKeyNames().ToList();
                    containers.AddRange(targetB.Key.GetSubKeyNames());
                    foreach (string keyPath in containers.Distinct())
                    {
                        using (RegistryKey subRegKeyA = targetA.Key.OpenSubKey(keyPath, false))
                        using (RegistryKey subRegKeyB = targetB.Key.OpenSubKey(keyPath, false))
                        {
                            ret &= RecursiveTree(
                                new MonitorTarget(PathType.Registry, Path.Combine(targetA.Path, keyPath), "registryA", subRegKeyA),
                                new MonitorTarget(PathType.Registry, Path.Combine(targetB.Path, keyPath), "registryB", subRegKeyB),
                                dictionary,
                                depth + 1);
                        }
                    }
                }
            }
            else
            {
                if (!targetA.Exists ?? false)
                {
                    dictionary[$"{_serial}_registryA_NotExists"] = targetA.Path;
                    ret = false;
                }
                if (!targetB.Exists ?? false)
                {
                    dictionary[$"{_serial}_registryB_NotExists"] = targetB.Path;
                    ret = false;
                }
            }

            return ret;
        }
    }
}
