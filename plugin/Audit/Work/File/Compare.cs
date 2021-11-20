using System;
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

namespace Audit.Work.File
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Compare : AuditTaskWork
    {
        [TaskParameter(Mandatory = true, ResolvEnv = true)]
        [Keys("filea", "file1", "sourcepath", "srcpath", "src", "source", "path", "filepath")]
        protected string _FileA { get; set; }

        [TaskParameter(Mandatory = true, ResolvEnv = true)]
        [Keys("fileb", "file2", "destinationpath", "dstpath", "dst", "destination")]
        protected string _FileB { get; set; }

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
        [Keys("attributes", "attribute", "attribs", "attrib", "attrs", "attr")]
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
        [Keys("exists", "exist")]
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
        [Keys("invert", "not", "no", "none")]
        protected bool _Invert { get; set; }

        private int _serial = 0;

        public override void MainProcess()
        {
            var dictionary = new Dictionary<string, string>();
            dictionary["fileA"] = _FileA;
            dictionary["fileB"] = _FileB;

            if (System.IO.File.Exists(_FileA) && System.IO.File.Exists(_FileB))
            {
                this.Success = true;

                _serial++;
                if (_IsCreationTime ?? false) { Success &= CompareTimeStamp(_FileA, _FileB, dictionary, "creation"); }
                if (_IsLastWriteTime ?? false) { Success &= CompareTimeStamp(_FileA, _FileB, dictionary, "lastwrite"); }
                if (_IsLastAccessTime ?? false) { Success &= CompareTimeStamp(_FileA, _FileB, dictionary, "lastaccess"); }
                if (_IsAccess ?? false) { Success &= CompareAccess(_FileA, _FileB, dictionary); }
                if (_IsOwner ?? false) { Success &= CompareOwner(_FileA, _FileB, dictionary); }
                if (_IsInherited ?? false) { Success &= ComparekInherited(_FileA, _FileB, dictionary); }
                if (_IsAttributes ?? false) { Success &= CompareAttributes(_FileA, _FileB, dictionary); }
                if (_IsMD5Hash ?? false) { Success &= ComparekHash(_FileA, _FileB, dictionary, "md5"); }
                if (_IsSHA256Hash ?? false) { Success &= ComparekHash(_FileA, _FileB, dictionary, "sha256"); }
                if (_IsSHA512Hash ?? false) { Success &= ComparekHash(_FileA, _FileB, dictionary, "sha512"); }
                if (_IsSize ?? false) { Success &= CompareSize(_FileA, _FileB, dictionary); }
            }
            else
            {
                dictionary["NotExists_fileA"] = _FileA;
                dictionary["NotExists_fileB"] = _FileB;
            }

            AddAudit(dictionary, this._Invert);
        }

        #region Compare Date

        /// <summary>
        /// 作成/更新/最終アクセス日時チェック
        /// </summary>
        /// <param name="fileA"></param>
        /// <param name="fileB"></param>
        /// <param name="dictionary"></param>
        /// <param name="dateType"></param>
        /// <returns></returns>
        private bool CompareTimeStamp(string fileA, string fileB, Dictionary<string, string> dictionary, string dateType)
        {
            string checkTarget = "";
            DateTime? fileA_date = null;
            DateTime? fileB_date = null;

            switch (dateType)
            {
                case "creation":
                    checkTarget = "CreationTime";
                    fileA_date = System.IO.File.GetCreationTime(fileA);
                    fileB_date = System.IO.File.GetCreationTime(fileB);
                    break;
                case "lastwrite":
                    checkTarget = "LastWriteTime";
                    fileA_date = System.IO.File.GetLastWriteTime(fileA);
                    fileB_date = System.IO.File.GetLastWriteTime(fileB);
                    break;
                case "lastaccess":
                    checkTarget = "LastAccessTime";
                    fileA_date = System.IO.File.GetLastAccessTime(fileA);
                    fileB_date = System.IO.File.GetLastAccessTime(fileB);
                    break;
            }

            string ret_fileA = "";
            string ret_fileB = "";
            if (_IsDateOnly)    //  IsDateOnlyのほうを優先の為、先に判定
            {
                ret_fileA = fileA_date?.ToString("yyyy/MM/dd");
                ret_fileB = fileB_date?.ToString("yyyy/MM/dd");
            }
            else if (_IsTimeOnly)
            {
                ret_fileA = fileA_date?.ToString("HH:mm:ss");
                ret_fileB = fileB_date?.ToString("HH:mm:ss");
            }
            else
            {
                ret_fileA = fileA_date?.ToString("yyyy/MM/dd HH:mm:ss");
                ret_fileB = fileB_date?.ToString("yyyy/MM/dd HH:mm:ss");
            }

            dictionary[$"fileA_{checkTarget}_{_serial}"] = ret_fileA;
            dictionary[$"fileB_{checkTarget}_{_serial}"] = ret_fileB;
            return ret_fileA == ret_fileB;
        }

        #endregion
        #region Compare Access

        /// <summary>
        /// アクセス権チェック
        /// </summary>
        /// <param name="fileA"></param>
        /// <param name="fileB"></param>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        private bool CompareAccess(string fileA, string fileB, Dictionary<string, string> dictionary)
        {
            /*
            FileSecurity securityA = new System.IO.FileInfo(fileA).GetAccessControl();
            FileSecurity securityB = new System.IO.FileInfo(fileB).GetAccessControl();

            string ret_fileA = FileControl.AccessRulesToString(
                securityA.GetAccessRules(true, false, typeof(NTAccount)));
            string ret_fileB = FileControl.AccessRulesToString(
                securityB.GetAccessRules(true, false, typeof(NTAccount)));
            */
            string ret_fileA = AccessRuleSummary.FileToAccessString(fileA);
            string ret_fileB = AccessRuleSummary.FileToAccessString(fileB);

            string checkTarget = "Access";
            dictionary[$"fileA_{checkTarget}_{_serial}"] = ret_fileA;
            dictionary[$"fileB_{checkTarget}_{_serial}"] = ret_fileB;
            return ret_fileA == ret_fileB;
        }

        #endregion
        #region Compare Owner

        /// <summary>
        /// 所有者チェック
        /// </summary>
        /// <param name="fileA"></param>
        /// <param name="fileB"></param>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        private bool CompareOwner(string fileA, string fileB, Dictionary<string, string> dictionary)
        {
            FileSecurity securityA = new System.IO.FileInfo(fileA).GetAccessControl();
            FileSecurity securityB = new System.IO.FileInfo(fileB).GetAccessControl();

            string ret_fileA = securityA.GetOwner(typeof(NTAccount)).Value;
            string ret_fileB = securityB.GetOwner(typeof(NTAccount)).Value;

            string checkTarget = "Owner";
            dictionary[$"fileA_{checkTarget}_{_serial}"] = ret_fileA;
            dictionary[$"fileB_{checkTarget}_{_serial}"] = ret_fileB;
            return ret_fileA == ret_fileB;
        }

        #endregion
        #region Compare Inherited

        /// <summary>
        /// アクセス権継承有無チェック
        /// </summary>
        /// <param name="fileA"></param>
        /// <param name="fileB"></param>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        private bool ComparekInherited(string fileA, string fileB, Dictionary<string, string> dictionary)
        {
            FileSecurity securityA = new System.IO.FileInfo(fileA).GetAccessControl();
            FileSecurity securityB = new System.IO.FileInfo(fileB).GetAccessControl();

            bool ret_fileA = !securityA.AreAccessRulesProtected;
            bool ret_fileB = !securityB.AreAccessRulesProtected;

            string checkTarget = "Inherited";
            dictionary[$"fileA_{checkTarget}_{_serial}"] = ret_fileA.ToString();
            dictionary[$"fileB_{checkTarget}_{_serial}"] = ret_fileB.ToString();
            return ret_fileA == ret_fileB;
        }

        #endregion
        #region Compare Attributes

        /// <summary>
        /// 読み取り専用/隠し/システム属性のチェック
        /// </summary>
        /// <param name="fileA"></param>
        /// <param name="fileB"></param>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        private bool CompareAttributes(string fileA, string fileB, Dictionary<string, string> dictionary)
        {
            FileAttributes attrA = System.IO.File.GetAttributes(fileA);
            FileAttributes attrB = System.IO.File.GetAttributes(fileB);

            string ret_fileA = string.Format("[{0}]ReadOnly [{1}]Hidden [{2}]System",
                (attrA & FileAttributes.ReadOnly) == FileAttributes.ReadOnly ? "x" : " ",
                (attrA & FileAttributes.Hidden) == FileAttributes.Hidden ? "x" : " ",
                (attrA & FileAttributes.System) == FileAttributes.System ? "x" : " ");
            string ret_fileB = string.Format("[{0}]ReadOnly [{1}]Hidden [{2}]System",
                (attrB & FileAttributes.ReadOnly) == FileAttributes.ReadOnly ? "x" : " ",
                (attrB & FileAttributes.Hidden) == FileAttributes.Hidden ? "x" : " ",
                (attrB & FileAttributes.System) == FileAttributes.System ? "x" : " ");

            string checkTarget = "Attributes";
            dictionary[$"fileA_{checkTarget}_{_serial}"] = ret_fileA;
            dictionary[$"fileB_{checkTarget}_{_serial}"] = ret_fileB;
            return ret_fileA == ret_fileB;
        }

        #endregion
        #region Compare Hash

        /// <summary>
        /// ファイルのハッシュを比較
        /// </summary>
        /// <param name="fileA"></param>
        /// <param name="fileB"></param>
        /// <param name="dictionary"></param>
        /// <param name="hashType">ハッシュタイプの種類。md5/sha256/sha512</param>
        /// <returns></returns>
        private bool ComparekHash(string fileA, string fileB, Dictionary<string, string> dictionary, string hashType)
        {
            string checkTarget = "";
            HashAlgorithm hashAlgA = null;
            HashAlgorithm hashAlgB = null;
            switch (hashType)
            {
                case "md5":
                    checkTarget = "MD5Hash";
                    hashAlgA = MD5.Create();
                    hashAlgB = MD5.Create();
                    break;
                case "sha256":
                    checkTarget = "SHA256Hash";
                    hashAlgA = SHA256.Create();
                    hashAlgB = SHA256.Create();
                    break;
                case "sha512":
                    checkTarget = "SHA512Hash";
                    hashAlgA = SHA512.Create();
                    hashAlgB = SHA512.Create();
                    break;
            }

            string ret_fileA = "";
            string ret_fileB = "";
            using (var fs = new System.IO.FileStream(fileA, FileMode.Open, FileAccess.Read))
            {
                ret_fileA = BitConverter.ToString(hashAlgA.ComputeHash(fs)).Replace("-", "");
                hashAlgA.Clear();
            }
            using (var fs = new System.IO.FileStream(fileB, FileMode.Open, FileAccess.Read))
            {
                ret_fileB = BitConverter.ToString(hashAlgB.ComputeHash(fs)).Replace("-", "");
                hashAlgB.Clear();
            }

            dictionary[$"fileA_{checkTarget}_{_serial}"] = ret_fileA;
            dictionary[$"fileB_{checkTarget}_{_serial}"] = ret_fileB;
            return !string.IsNullOrEmpty(ret_fileA) &&
                !string.IsNullOrEmpty(ret_fileB) &&
                ret_fileA == ret_fileB;
        }

        #endregion
        #region Compare Size

        /// <summary>
        /// ファイルサイズをチェック
        /// </summary>
        /// <param name="fileA"></param>
        /// <param name="fileB"></param>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        private bool CompareSize(string fileA, string fileB, Dictionary<string, string> dictionary)
        {
            long ret_fileA = new System.IO.FileInfo(fileA).Length;
            long ret_fileB = new System.IO.FileInfo(fileB).Length;

            string checkTarget = "Size";
            dictionary[$"fileA_{checkTarget}_{_serial}"] = ret_fileA.ToString();
            dictionary[$"fileB_{checkTarget}_{_serial}"] = ret_fileB.ToString();
            return ret_fileA == ret_fileB;
        }

        #endregion
    }
}
