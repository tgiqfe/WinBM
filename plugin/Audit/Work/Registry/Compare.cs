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

namespace Audit.Work.Registry
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Compare : AuditTaskWork
    {
        [TaskParameter(Mandatory = true, ResolvEnv = true)]
        [Keys("keya", "key1", "registrykeya", "registrykey2", "registrya", "registry1", "rega", "reg1", "sourcepath", "srcpath", "src", "source", "path", "registrypath")]
        protected string _RegistryPathA { get; set; }

        [TaskParameter(Mandatory = true, ResolvEnv = true)]
        [Keys("keyb", "key2", "registrykeyb", "registrykey2", "registryb", "registry2", "regb", "reg2", "destinationpath", "dstpath", "dst", "destination")]
        protected string _RegistryPathB { get; set; }

        [TaskParameter(ResolvEnv = true)]
        [Keys("namea", "name1", "sourcename", "srcname", "registryname", "regname", "paramname")]
        protected string _RegistryNameA { get; set; }

        [TaskParameter(ResolvEnv = true)]
        [Keys("nameb", "name2", "destinationname", "dstname")]
        protected string _RegistryNameB { get; set; }

        //  ################################

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

        //  ChildCount比較も追加予定

        [TaskParameter(MandatoryAny = 1)]
        [Keys("isregistrytype", "isvaluekind", "isregtype", "valuekind", "kind", "type", "registrytype", "regtype")]
        protected bool? _IsRegistryType { get; set; }

        /// <summary>
        /// 存在チェックのみに使用するパラメータ。その他の比較処理の過程で確認できる為、Exists用の特別な作業は無し
        /// </summary>
        [TaskParameter(MandatoryAny = 8)]
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

        public override void MainProcess()
        {
            //  MaxDeepth無指定の場合は[5]をセット
            _MaxDepth ??= 5;

            var dictionary = new Dictionary<string, string>();
            dictionary["keyA"] = _RegistryPathA;
            dictionary["keyB"] = _RegistryPathB;
            if (!string.IsNullOrEmpty(_RegistryNameA))
            {
                _RegistryNameB ??= _RegistryNameA;
                dictionary["nameA"] = _RegistryNameA;
                dictionary["nameB"] = _RegistryNameB;
            }

            using (var regKeyA = RegistryControl.GetRegistryKey(_RegistryPathA, false, false))
            using (var regKeyB = RegistryControl.GetRegistryKey(_RegistryPathB, false, false))
            {
                bool bothExist = true;
                if (regKeyA == null)
                {
                    dictionary["NotExists_keyA"] = _RegistryPathA;
                    bothExist = false;
                }
                if (regKeyB == null)
                {
                    dictionary["NotExists_keyB"] = _RegistryPathB;
                    bothExist = false;
                }
                if (bothExist)
                {
                    this.Success = true;
                    if (string.IsNullOrEmpty(_RegistryNameA))
                    {
                        //  レジストリキーチェック
                        RecursiveTree(regKeyA, regKeyB, dictionary, 0);
                    }
                    else
                    {
                        //  レジストリ値のみチェック
                        if (regKeyA.GetValueNames().Any(x => x.Equals(_RegistryNameA)) &&
                            regKeyB.GetValueNames().Any(x => x.Equals(_RegistryNameB)))
                        {
                            _serial++;
                            if (_IsRegistryType ?? false) { Success &= CompareType(regKeyA, regKeyB, _RegistryNameA, _RegistryNameB, dictionary); }
                            if (_IsMD5Hash ?? false) { Success &= CompareHash(regKeyA, regKeyB, _RegistryNameA, _RegistryNameB, dictionary, "md5"); }
                            if (_IsSHA256Hash ?? false) { Success &= CompareHash(regKeyA, regKeyB, _RegistryNameA, _RegistryNameB, dictionary, "sha256"); }
                            if (_IsSHA512Hash ?? false) { Success &= CompareHash(regKeyA, regKeyB, _RegistryNameA, _RegistryNameB, dictionary, "sha512"); }
                        }
                        else
                        {
                            dictionary["NotExists_nameA"] = _RegistryNameA;
                            dictionary["NotExists_nameB"] = _RegistryNameB;
                        }
                    }
                }
            }

            AddAudit(dictionary, this._Invert);
        }

        private void RecursiveTree(RegistryKey targetKeyA, RegistryKey targetKeyB, Dictionary<string, string> dictionary, int depth)
        {
            //  レジストリキー情報の比較
            string checkKeyPath = targetKeyA.Name.Replace(_RegistryPathA, "");
            _serial++;
            dictionary[$"RegistryKey_{_serial}"] = checkKeyPath;

            if (_IsAccess ?? false) { Success &= CompareAccess(targetKeyA, targetKeyB, dictionary); }
            if (_IsOwner ?? false) { Success &= CompareOwner(targetKeyA, targetKeyB, dictionary); }
            if (_IsInherited ?? false) { Success &= CompareInherited(targetKeyA, targetKeyB, dictionary); }
            if (!this.Success) { return; }

            //  レジストリ値の比較
            string[] targetKeyA_files = targetKeyA.GetValueNames();
            string[] targetKeyB_files = targetKeyB.GetValueNames();
            if (targetKeyA_files.SequenceEqual(targetKeyB_files))
            {
                foreach (string name in targetKeyA_files)
                {
                    _serial++;
                    dictionary[$"RegistryName_{_serial}"] = name;

                    if (_IsRegistryType ?? false) { Success &= CompareType(targetKeyA, targetKeyB, name, name, dictionary); }
                    if (_IsMD5Hash ?? false) { Success &= CompareHash(targetKeyA, targetKeyB, name, name, dictionary, "md5"); }
                    if (_IsSHA256Hash ?? false) { Success &= CompareHash(targetKeyA, targetKeyB, name, name, dictionary, "sha256"); }
                    if (_IsSHA512Hash ?? false) { Success &= CompareHash(targetKeyA, targetKeyB, name, name, dictionary, "sha512"); }
                    if (!this.Success) { return; }
                }
            }
            else
            {
                _serial++;
                dictionary[$"NotMatchValues_{_serial}"] = checkKeyPath;
                this.Success = false;
            }
            if (!this.Success) { return; }

            //  配下のキーの比較
            string[] targetKeyA_keys = targetKeyA.GetSubKeyNames();
            string[] targetKeyB_keys = targetKeyB.GetSubKeyNames();
            if (targetKeyA_keys.SequenceEqual(targetKeyB_keys))
            {
                if (depth < _MaxDepth)
                {
                    foreach (string childKey in targetKeyA_keys)
                    {
                        RecursiveTree(targetKeyA.OpenSubKey(childKey, false), targetKeyB.OpenSubKey(childKey), dictionary, depth + 1);
                        if (!this.Success) { return; }
                    }
                }
            }
            else
            {
                _serial++;
                dictionary[$"NotMatchSubKeys_{_serial}"] = checkKeyPath;
                this.Success = false;
            }
        }

        #region Compare RegistryType
        /// <summary>
        /// レジストリタイプ比較
        /// </summary>
        /// <param name="regKeyA"></param>
        /// <param name="regKeyB"></param>
        /// <param name="nameA"></param>
        /// <param name="nameB"></param>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        private bool CompareType(RegistryKey regKeyA, RegistryKey regKeyB, string nameA, string nameB, Dictionary<string, string> dictionary)
        {
            RegistryValueKind ret_nameA = regKeyA.GetValueKind(nameA);
            RegistryValueKind ret_nameB = regKeyB.GetValueKind(nameB);

            string checkTarget = "RegistryType";
            dictionary[$"nameA_{checkTarget}_{_serial}"] = RegistryControl.ValueKindToString(ret_nameA);
            dictionary[$"nameB_{checkTarget}_{_serial}"] = RegistryControl.ValueKindToString(ret_nameB);
            return ret_nameA == ret_nameB;
        }

        #endregion
        #region Compare Access

        /// <summary>
        /// レジストリキーのアクセス権比較
        /// </summary>
        /// <param name="regKeyA"></param>
        /// <param name="regKeyB"></param>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        private bool CompareAccess(RegistryKey regKeyA, RegistryKey regKeyB, Dictionary<string, string> dictionary)
        {
            /*
            RegistrySecurity securityA = regKeyA.GetAccessControl();
            RegistrySecurity securityB = regKeyB.GetAccessControl();

            string ret_keyA = RegistryControl.AccessRulesToString(
                securityA.GetAccessRules(true, false, typeof(NTAccount)));
            string ret_keyB = RegistryControl.AccessRulesToString(
                securityB.GetAccessRules(true, false, typeof(NTAccount)));
            */

            string ret_keyA = AccessRuleSummary.RegistryKeyToAccessString(regKeyA);
            string ret_keyB = AccessRuleSummary.RegistryKeyToAccessString(regKeyB);

            string checkTarget = "Access";
            dictionary[$"keyA_{checkTarget}_{_serial}"] = ret_keyA.ToString();
            dictionary[$"keyB_{checkTarget}_{_serial}"] = ret_keyB.ToString();
            return ret_keyA == ret_keyB;
        }

        #endregion
        #region Compare Owner

        /// <summary>
        /// レジストリキーの所有者比較
        /// </summary>
        /// <param name="regKeyA"></param>
        /// <param name="regKeyB"></param>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        private bool CompareOwner(RegistryKey regKeyA, RegistryKey regKeyB, Dictionary<string, string> dictionary)
        {
            RegistrySecurity securityA = regKeyA.GetAccessControl();
            RegistrySecurity securityB = regKeyB.GetAccessControl();

            string ret_keyA = securityA.GetOwner(typeof(NTAccount)).Value;
            string ret_keyB = securityB.GetOwner(typeof(NTAccount)).Value;

            string checkTarget = "Owner";
            dictionary[$"keyA_{checkTarget}_{_serial}"] = ret_keyA.ToString();
            dictionary[$"keyB_{checkTarget}_{_serial}"] = ret_keyB.ToString();
            return ret_keyA == ret_keyB;
        }

        #endregion
        #region Compare Inherited

        /// <summary>
        /// レジストリキーのアクセス兼継承有無を比較
        /// </summary>
        /// <param name="regKeyA"></param>
        /// <param name="regKeyB"></param>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        private bool CompareInherited(RegistryKey regKeyA, RegistryKey regKeyB, Dictionary<string, string> dictionary)
        {
            RegistrySecurity securityA = regKeyA.GetAccessControl();
            RegistrySecurity securityB = regKeyB.GetAccessControl();

            string ret_keyA = (!securityA.AreAccessRulesProtected).ToString();
            string ret_keyB = (!securityB.AreAccessRulesProtected).ToString();

            string checkTarget = "Inherited";
            dictionary[$"keyA_{checkTarget}_{_serial}"] = ret_keyA.ToString();
            dictionary[$"keyB_{checkTarget}_{_serial}"] = ret_keyB.ToString();
            return ret_keyA == ret_keyB;
        }

        #endregion
        #region Compare Hash

        /// <summary>
        /// レジストリ値のハッシュを比較
        /// </summary>
        /// <param name="regKeyA"></param>
        /// <param name="regKeyB"></param>
        /// <param name="nameA"></param>
        /// <param name="nameB"></param>
        /// <param name="dictionary"></param>
        /// <param name="hashType">ハッシュタイプの種類。md5/sha256/sha512</param>
        /// <returns></returns>
        private bool CompareHash(RegistryKey regKeyA, RegistryKey regKeyB, string nameA, string nameB, Dictionary<string, string> dictionary, string hashType)
        {
            string checkTarget = "";
            HashAlgorithm hashAlgA = null;
            HashAlgorithm hashAlgB = null;
            switch (hashType)
            {
                case "md5":
                    checkTarget = "MD5Hash";
                    //hashAlgA = new MD5CryptoServiceProvider();
                    //hashAlgB = new MD5CryptoServiceProvider();
                    hashAlgA = MD5.Create();
                    hashAlgB = MD5.Create();
                    break;
                case "sha256":
                    checkTarget = "SHA256Hash";
                    //hashAlgA = new SHA256CryptoServiceProvider();
                    //hashAlgB = new SHA256CryptoServiceProvider();
                    hashAlgA = SHA256.Create();
                    hashAlgB = SHA256.Create();
                    break;
                case "sha512":
                    checkTarget = "SHA512Hash";
                    //hashAlgA = new SHA512CryptoServiceProvider();
                    //hashAlgB = new SHA512CryptoServiceProvider();
                    hashAlgA = SHA512.Create();
                    hashAlgB = SHA512.Create();
                    break;
            }

            byte[] bytesA = RegistryControl.RegistryValueToBytes(regKeyA, nameA, null, true);
            byte[] bytesB = RegistryControl.RegistryValueToBytes(regKeyB, nameB, null, true);

            string ret_nameA = BitConverter.ToString(hashAlgA.ComputeHash(bytesA)).Replace("-", "");
            hashAlgA.Clear();
            string ret_nameB = BitConverter.ToString(hashAlgB.ComputeHash(bytesB)).Replace("-", "");
            hashAlgB.Clear();

            dictionary[$"nameA_{checkTarget}_{_serial}"] = ret_nameA.ToString();
            dictionary[$"nameB_{checkTarget}_{_serial}"] = ret_nameB.ToString();
            return ret_nameA == ret_nameB;
        }

        #endregion
    }
}
