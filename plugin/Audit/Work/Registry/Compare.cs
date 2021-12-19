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

        [TaskParameter]
        [Keys("maxdepth", "depth", "maxdeepth", "deepth")]
        protected int? _MaxDepth { get; set; }

        [TaskParameter]
        [Keys("invert", "not", "no", "none")]
        protected bool _Invert { get; set; }

        private int _serial = 0;

        private MonitorTarget CreateForRegistryKey(RegistryKey key, string pathTypeName)
        {
            return new MonitorTarget(PathType.Registry, key)
            {
                PathTypeName = pathTypeName,
                IsAccess = _IsAccess,
                IsOwner = _IsOwner,
                IsInherited = _IsInherited,
                IsChildCount = _IsChildCount,
            };
        }

        private MonitorTarget CreateForRegistryValue(RegistryKey key, string name, string pathTypeName)
        {
            return new MonitorTarget(PathType.Registry, key, name)
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
                    MonitorTarget targetA = CreateForRegistryValue(keyA, _NameA, "registryA");
                    MonitorTarget targetB = CreateForRegistryValue(keyB, _NameB, "registryB");
                    targetA.CheckExists();
                    targetB.CheckExists();

                    if ((targetA.Exists ?? false) && (targetB.Exists ?? false))
                    {
                        dictionary["registryA_Exists"] = _PathA + "\\" + _NameA;
                        dictionary["registryB_Exists"] = _PathB + "\\" + _NameB;
                        Success &= CompareFunctions.CheckRegistryValue(targetA, targetB, dictionary, _serial);
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
                    Success &= RecursiveTree(keyA, keyB, dictionary, 0);
                }
            }

            AddAudit(dictionary, this._Invert);
        }

        private bool RecursiveTree(RegistryKey keyA, RegistryKey keyB, Dictionary<string, string> dictionary, int depth)
        {
            bool ret = true;

            _serial++;
            MonitorTarget targetA = CreateForRegistryKey(keyA, "registryA");
            MonitorTarget targetB = CreateForRegistryKey(keyB, "registryB");
            targetA.CheckExists();
            targetB.CheckExists();
            if ((targetA.Exists ?? false) && (targetB.Exists ?? false))
            {
                dictionary[$"{_serial}_registryA_Exists"] = keyA.Name;
                dictionary[$"{_serial}_registryB_Exists"] = keyB.Name;
                ret &= CompareFunctions.CheckRegistryKey(targetA, targetB, dictionary, _serial, depth);

                if (depth < _MaxDepth)
                {
                    foreach (string childName in keyA.GetValueNames())
                    {
                        _serial++;
                        MonitorTarget targetA_leaf = CreateForRegistryValue(keyA, childName, "registryA");
                        MonitorTarget targetB_leaf = CreateForRegistryValue(keyB, childName, "registryB");
                        targetA_leaf.CheckExists();
                        targetB_leaf.CheckExists();

                        if (targetB_leaf.Exists ?? false)
                        {
                            dictionary[$"{_serial}_registryA_Exists"] = targetA_leaf.Path + "\\" + targetA_leaf.Name;
                            dictionary[$"{_serial}_registryB_Exists"] = targetB_leaf.Path + "\\" + targetB_leaf.Name;
                            ret &= CompareFunctions.CheckRegistryValue(targetA_leaf, targetB_leaf, dictionary, _serial);
                        }
                        else
                        {
                            dictionary[$"{_serial}_registryB_NotExists"] = keyB.Name + "\\" + childName;
                            ret = false;
                        }
                    }
                    foreach (string keyPath in keyA.GetSubKeyNames())
                    {
                        using (RegistryKey subRegKeyA = keyA.OpenSubKey(keyPath, false))
                        using (RegistryKey subRegKeyB = keyB.OpenSubKey(keyPath, false))
                        {
                            ret &= RecursiveTree(subRegKeyA, subRegKeyB, dictionary, depth + 1);
                        }
                    }
                }
            }
            else
            {
                if (!targetA.Exists ?? false)
                {
                    dictionary[$"{_serial}_registryA_NotExists"] = keyA?.Name ?? "(キー無し)";  //  Recurseする時にMonitorTargetで渡す方式に変更しないと、ここでパスを取得できない。後日変更
                    ret = false;
                }
                if (!targetB.Exists ?? false)
                {
                    dictionary[$"{_serial}_registryB_NotExists"] = keyB?.Name ?? "(キー無し)";
                    ret = false;
                }
            }

            return ret;
        }
    }
}
