using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WinBM;
using WinBM.Task;
using Audit.Lib;
using IO.Lib;
using System.Security.Principal;
using System.Security.AccessControl;
using System.Security.Cryptography;

namespace Audit.Work.Directory
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Watch : AuditTaskWork
    {
        [TaskParameter(Mandatory = true)]
        [Keys("serial", "uniquekey", "id")]
        protected string _Serial { get; set; }

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
        [Keys("maxdepth", "depth", "maxdeepth", "deepth")]
        protected int? _MaxDepth { get; set; }

        [TaskParameter]
        [Keys("begin", "start")]
        protected bool _IsStart { get; set; }

        [TaskParameter]
        [Keys("invert", "not", "no", "none")]
        protected bool _Invert { get; set; }

        private int _serial = 0;
        private string _checkingRootDir = "";

        public override void MainProcess()
        {
            //  MaxDeph無指定の場合は[5]をセット
            _MaxDepth ??= 5;

            var dictionary = new Dictionary<string, string>();
            dictionary["watchTarget"] = string.Join(", ", _Path);

            WatchDataCollection collection = LoadWatchDB(_Serial);

            //  IsStartもしくはWatchDataCollectionが無い場合、Watch開始にする為Success=true
            Success = _IsStart || collection == null;

            foreach (string path in _Path)
            {
                WatchData watch = collection.GetWatchData(path);
                Success |= WatchExists_dir(path, dictionary, watch);

                if (System.IO.Directory.Exists(path))
                {
                    _checkingRootDir = path;
                    RecursiveTree(path, dictionary, watch, 0);
                }
                collection.SetWatchData(path, watch);
            }

            /*
             * 
             * このあたりで、前回Watch時に在って、今回Watch時に無くなっているファイル/フォルダーの確認
             * 
             */

            SaveWatchDB(_Serial, collection);

            AddAudit(dictionary, this._Invert);
        }

        private void RecursiveTree(string path, Dictionary<string, string> dictionary, WatchData watch, int depth)
        {
            string checkPath = path.Replace(_checkingRootDir, "");
            _serial++;
            dictionary[$"Directory_{_serial}"] = checkPath;

            //  ディレクトリ情報をWatch
            if ((_IsCreationTime ?? false) || watch.CreationTime != null) { Success |= WatchTimeStamp_dir(path, dictionary, watch, "creation"); }
            if ((_IsLastWriteTime ?? false) || watch.LastWriteTime != null) { Success |= WatchTimeStamp_dir(path, dictionary, watch, "lastwrite"); }
            if ((_IsLastAccessTime ?? false) || watch.LastAccessTime != null) { Success |= WatchTimeStamp_dir(path, dictionary, watch, "lastaccess"); }
            if ((_IsAccess ?? false) || watch.Access != null) { Success |= WatchAccess_dir(path, dictionary, watch); }
            if ((_IsOwner ?? false) || watch.Owner != null) { Success |= WatchOwner_dir(path, dictionary, watch); }
            if ((_IsInherited ?? false) || watch.Inherited != null) { Success |= WatchInherited_dir(path, dictionary, watch); }
            if ((_IsAttributes ?? false) || watch.Attributes != null) { Success |= WatchAttributes_dir(path, dictionary, watch); }

            //  配下のファイル情報をWatch
            foreach (string filePath in System.IO.Directory.GetFiles(path))
            {
                string checkFilePath = filePath.Replace(_checkingRootDir, "");
                _serial++;
                dictionary[$"File_{_serial}"] = checkFilePath;

                if ((_IsCreationTime ?? false) || watch.CreationTime != null) { Success |= WatchTimeStamp(filePath, dictionary, watch, "creation"); }
                if ((_IsLastWriteTime ?? false) || watch.LastWriteTime != null) { Success |= WatchTimeStamp(filePath, dictionary, watch, "lastwrite"); }
                if ((_IsLastAccessTime ?? false) || watch.LastAccessTime != null) { Success |= WatchTimeStamp(filePath, dictionary, watch, "lastaccess"); }
                if ((_IsAccess ?? false) || watch.Access != null) { Success |= WatchAccess(filePath, dictionary, watch); }
                if ((_IsOwner ?? false) || watch.Owner != null) { Success |= WatchOwner(filePath, dictionary, watch); }
                if ((_IsInherited ?? false) || watch.Inherited != null) { Success |= WatchInherited(filePath, dictionary, watch); }
                if ((_IsAttributes ?? false) || watch.Attributes != null) { Success |= WatchAttributes(filePath, dictionary, watch); }
                if ((_IsMD5Hash ?? false) || watch.MD5Hash != null) { Success |= WatchHash(filePath, dictionary, watch, "md5"); }
                if ((_IsSHA256Hash ?? false) || watch.SHA256Hash != null) { Success |= WatchHash(filePath, dictionary, watch, "sha256"); }
                if ((_IsSHA512Hash ?? false) || watch.SHA512Hash != null) { Success |= WatchHash(filePath, dictionary, watch, "sha512"); }
                if ((_IsSize ?? false) || watch.Size != null) { Success |= WatchSize(filePath, dictionary, watch); }
            }


        }

        #region Watch Exists

        private bool WatchExists_dir(string path, Dictionary<string, string> dictionary, WatchData watch)
        {
            bool exists = System.IO.Directory.Exists(path);

            dictionary[$"directory_Exists_{_serial}"] = exists.ToString();
            bool ret = watch.Exists == null || watch.Exists == exists;
            watch.Exists = exists;
            return ret;
        }

        private bool WatchExists(string path, Dictionary<string, string> dictionary, WatchData watch)
        {
            bool exists = System.IO.File.Exists(path);

            dictionary[$"file_Exists_{_serial}"] = exists.ToString();

            if (watch.Exists == null || watch.Exists == exists)
            {
                watch.Exists = exists;
                return true;
            }

            //  前回Watch時存在していて、今回存在しない場合はWatchDataをクリア
            if ((bool)watch.Exists && !exists)
            {
                watch = new WatchData();
                watch.Exists = exists;
            }

            return false;
        }

        #endregion
        #region Watch Date

        private bool WatchTimeStamp_dir(string path, Dictionary<string, string> dictionary, WatchData watch, string dateType)
        {
            Func<DateTime, string> processDate = (date) =>
            {
                if (_IsDateOnly)
                {
                    return date.ToString("yyyy/MM/dd");
                }
                else if (_IsTimeOnly)
                {
                    return date.ToString("HH:mm:ss");
                }
                else
                {
                    return date.ToString("yyyy/MM/dd HH:mm:ss");
                }
            };

            bool ret = false;
            switch (dateType)
            {
                case "creation":
                    string creation = processDate(System.IO.Directory.GetCreationTime(path));
                    dictionary[$"directory_CreationTime_{_serial}"] = creation;
                    ret = watch.CreationTime == null || watch.CreationTime == creation;
                    watch.CreationTime = creation;
                    break;
                case "lastwrite":
                    string lastWrite = processDate(System.IO.Directory.GetLastWriteTime(path));
                    dictionary[$"directory_LastWriteTime_{_serial}"] = lastWrite;
                    ret = watch.LastWriteTime == null || watch.LastWriteTime == lastWrite;
                    watch.LastWriteTime = lastWrite;
                    break;
                case "lastaccess":
                    string lastaccess = processDate(System.IO.Directory.GetLastAccessTime(path));
                    dictionary[$"directory_LastAccess_{_serial}"] = lastaccess;
                    ret = watch.LastAccessTime == null || watch.LastAccessTime == lastaccess;
                    watch.LastAccessTime = lastaccess;
                    break;
            }

            return ret;
        }

        private bool WatchTimeStamp(string path, Dictionary<string, string> dictionary, WatchData watch, string dateType)
        {
            Func<DateTime, string> processDate = (date) =>
            {
                if (_IsDateOnly)
                {
                    return date.ToString("yyyy/MM/dd");
                }
                else if (_IsTimeOnly)
                {
                    return date.ToString("HH:mm:ss");
                }
                else
                {
                    return date.ToString("yyyy/MM/dd HH:mm:ss");
                }
            };

            bool ret = false;
            switch (dateType)
            {
                case "creation":
                    string creation = processDate(System.IO.File.GetCreationTime(path));
                    dictionary[$"file_CreationTime_{_serial}"] = creation;
                    ret = watch.CreationTime == null || watch.CreationTime == creation;
                    watch.CreationTime = creation;
                    break;
                case "lastwrite":
                    string lastWrite = processDate(System.IO.File.GetCreationTime(path));
                    dictionary[$"file_LastWriteTime_{_serial}"] = lastWrite;
                    ret = watch.LastWriteTime == null || watch.LastWriteTime == lastWrite;
                    watch.LastWriteTime = lastWrite;
                    break;
                case "lastaccess":
                    string lastAccess = processDate(System.IO.File.GetCreationTime(path));
                    dictionary[$"file_LastWriteTime_{_serial}"] = lastAccess;
                    ret = watch.LastAccessTime == null || watch.LastAccessTime == lastAccess;
                    watch.LastAccessTime = lastAccess;
                    break;
            }

            return ret;
        }

        #endregion
        #region Watch Access

        private bool WatchAccess_dir(string path, Dictionary<string, string> dictionary, WatchData watch)
        {
            string access = AccessRuleSummary.DirectoryToAccessString(path);

            dictionary[$"directory_Access_{_serial}"] = access;
            bool ret = watch.Access == null || watch.Access == access;
            watch.Access = access;
            return ret;
        }

        private bool WatchAccess(string path, Dictionary<string, string> dictionary, WatchData watch)
        {
            string access = AccessRuleSummary.FileToAccessString(path);

            dictionary[$"file_Access_{_serial}"] = access;
            bool ret = watch.Access == null || watch.Access == access;
            watch.Access = access;
            return ret;
        }

        #endregion
        #region Watch Owner

        private bool WatchOwner_dir(string path, Dictionary<string, string> dictionary, WatchData watch)
        {
            string owner = new DirectoryInfo(path).GetAccessControl().GetOwner(typeof(NTAccount)).Value;

            dictionary[$"directory_Owner_{_serial}"] = owner;
            bool ret = watch.Owner == null || watch.Owner == owner;
            watch.Owner = owner;
            return ret;
        }

        private bool WatchOwner(string path, Dictionary<string, string> dictionary, WatchData watch)
        {
            string owner = new FileInfo(path).GetAccessControl().GetOwner(typeof(NTAccount)).Value;

            dictionary[$"file_Owner_{_serial}"] = owner;
            bool ret = watch.Owner == null || watch.Owner == owner;
            watch.Owner = owner;
            return ret;
        }

        #endregion
        #region Watch Inherited

        private bool WatchInherited_dir(string path, Dictionary<string, string> dictionary, WatchData watch)
        {
            bool inherited = !new DirectoryInfo(path).GetAccessControl().AreAccessRulesProtected;

            dictionary[$"directory_Inherited_{_serial}"] = inherited.ToString();
            bool ret = watch.Inherited == null || watch.Inherited == inherited;
            watch.Inherited = inherited;
            return ret;
        }

        private bool WatchInherited(string path, Dictionary<string, string> dictionary, WatchData watch)
        {
            bool inherited = !new FileInfo(path).GetAccessControl().AreAccessRulesProtected;

            dictionary[$"file_Inherited_{_serial}"] = inherited.ToString();
            bool ret = watch.Inherited == null || watch.Inherited == inherited;
            watch.Inherited = inherited;
            return ret;
        }

        #endregion
        #region Watch Attributes

        private bool WatchAttributes_dir(string path, Dictionary<string, string> dictionary, WatchData watch)
        {
            FileAttributes attr = System.IO.File.GetAttributes(path);
            string[] attrArray = new string[]
            {
                (attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly ? "ReadOnly" : null,
                (attr & FileAttributes.Hidden) == FileAttributes.Hidden ? "Hidden" : null,
                (attr & FileAttributes.System) == FileAttributes.System ? "System" : null
            }.Where(x => x != null).ToArray();

            dictionary[$"directory_Attributes_{_serial}"] = string.Join(", ", attrArray);
            bool ret = watch.Attributes == null || watch.Attributes.SequenceEqual(attrArray);
            watch.Attributes = attrArray;
            return ret;
        }

        private bool WatchAttributes(string path, Dictionary<string, string> dictionary, WatchData watch)
        {
            FileAttributes attr = System.IO.File.GetAttributes(path);
            string[] attrArray = new string[]
            {
                (attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly ? "ReadOnly" : null,
                (attr & FileAttributes.Hidden) == FileAttributes.Hidden ? "Hidden" : null,
                (attr & FileAttributes.System) == FileAttributes.System ? "System" : null
            }.Where(x => x != null).ToArray();

            dictionary[$"file_Attributes_{_serial}"] = string.Join(", ", attrArray);
            bool ret = watch.Attributes == null || watch.Attributes.SequenceEqual(attrArray);
            watch.Attributes = attrArray;
            return ret;
        }

        #endregion
        #region Watch Hash

        //  CheckHash_dirは無し

        private bool WatchHash(string path, Dictionary<string, string> dictionary, WatchData watch, string hashType)
        {
            using (var fs = new System.IO.FileStream(path, FileMode.Open, FileAccess.Read))
            {
                switch (hashType)
                {
                    case "md5":
                        HashAlgorithm md5 = MD5.Create();
                        string text_md5 = BitConverter.ToString(md5.ComputeHash(fs)).Replace("-", "");
                        md5.Clear();

                        dictionary[$"file_MD5Hash_{_serial}"] = text_md5;
                        bool ret_md5 = watch.MD5Hash == null || watch.MD5Hash == text_md5;
                        watch.MD5Hash = text_md5;
                        return ret_md5;
                    case "sha256":
                        HashAlgorithm sha256 = SHA256.Create();
                        string text_sha256 = BitConverter.ToString(sha256.ComputeHash(fs)).Replace("-", "");
                        sha256.Clear();

                        dictionary[$"file_MD5Hash_{_serial}"] = text_sha256;
                        bool ret_sha256 = watch.SHA256Hash == null || watch.SHA256Hash == text_sha256;
                        watch.SHA256Hash = text_sha256;
                        return ret_sha256;
                    case "sha512":
                        HashAlgorithm sha512 = SHA512.Create();
                        string text_sha512 = BitConverter.ToString(sha512.ComputeHash(fs)).Replace("-", "");
                        sha512.Clear();

                        dictionary[$"file_MD5Hash_{_serial}"] = text_sha512;
                        bool ret_sha512 = watch.SHA512Hash == null || watch.SHA512Hash == text_sha512;
                        watch.SHA512Hash = text_sha512;
                        return ret_sha512;
                }
            }

            return false;
        }

        #endregion
        #region Watch Size

        //  WatchSize_dirは無し

        private bool WatchSize(string path, Dictionary<string, string> dictionary, WatchData watch)
        {
            long size = new FileInfo(path).Length;
            dictionary[$"file_Size_{_serial}"] = size.ToString();
            bool ret = watch.Size == null || watch.Size == size;
            watch.Size = size;
            return ret;
        }

        #endregion
    }
}
