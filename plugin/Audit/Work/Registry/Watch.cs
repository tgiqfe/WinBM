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
using Audit.Lib;

namespace Audit.Work.Registry
{
    /// <summary>
    /// Watch関連のクラス。Registry関連で詰まったので、一旦凍結。
    /// </summary>
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Watch : AuditTaskWork
    {
        [TaskParameter(Mandatory = true, ResolvEnv = true)]
        [Keys("path", "registrypath", "targetpath", "key", "registrykey", "targetkey", "regkey", "target")]
        protected string[] _Path { get; set; }

        [TaskParameter(ResolvEnv = true, Delimiter = '\n')]
        [Keys("name", "name", "namae", "registryname", "regname", "paramname")]
        protected string[] _Name { get; set; }

        //  ################################

        [TaskParameter(MandatoryAny = 1)]
        [Keys("istype", "isvaluekind", "isregtype", "valuekind", "kind", "type", "regtype")]
        protected bool? _IsType { get; set; }

        [TaskParameter(MandatoryAny = 2)]
        [Keys("isaccess", "access", "acl")]
        protected bool? _IsAccess { get; set; }

        [TaskParameter(MandatoryAny = 3)]
        [Keys("isowner", "owner", "own")]
        protected bool? _IsOwner { get; set; }

        [TaskParameter(MandatoryAny = 4)]
        [Keys("isinherited", "inherited", "inherit", "inheritance")]
        protected bool? _IsInherited { get; set; }

        [TaskParameter(MandatoryAny = 5)]
        [Keys("ismd5hash", "md5hash", "md5")]
        protected bool? _IsMD5Hash { get; set; }

        [TaskParameter(MandatoryAny = 6)]
        [Keys("issha256hash", "sha256hash", "sha256", "hash")]
        protected bool? _IsSHA256Hash { get; set; }

        [TaskParameter(MandatoryAny = 7)]
        [Keys("issha512hash", "sha512hash", "sha512")]
        protected bool? _IsSHA512Hash { get; set; }

        [TaskParameter(MandatoryAny = 8)]
        [Keys("exists", "exist")]
        protected bool? _IsExists { get; set; }

        //  ################################

        [TaskParameter]
        [Keys("maxdepth", "depth", "maxdeepth", "deepth")]
        protected int? _MaxDepth { get; set; }

        [TaskParameter]
        [Keys("begin", "start")]
        protected bool _IsStart { get; set; }

        [TaskParameter]
        [Keys("invert", "not", "no", "none")]
        protected bool _Invert { get; set; }

        private int _serial = 0;

        public override void MainProcess()
        {
            //  MaxDeepth無指定の場合は[5]をセット
            _MaxDepth ??= 5;

            var dictionary = new Dictionary<string, string>();
            if (_Name?.Length > 0)
            {
                //  レジストリ値をWatch
                dictionary["watchTarget_key"] = _Path[0];
                dictionary["watchTarget_name"] = String.Join(", ", _Name);

                using(RegistryKey regKey = RegistryControl.GetRegistryKey(_Path[0], false, false))
                {
                    if(regKey == null)
                    {
                        _serial++;
                        dictionary[$"registryKey_NotExists_{_serial}"] = regKey.Name;
                    }
                    else
                    {

                    }
                }
            }
            else
            {
                //  レジストリキーをwatch
                dictionary["watchTarget"] = string.Join(", ", _Path);
            }

            AddAudit(dictionary, this._Invert);
        }

    }
}
