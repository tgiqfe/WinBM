using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WinBM;
using WinBM.Task;
using Audit.Lib;

namespace Audit.Work.File
{
    /// <summary>
    /// 対象のパスの変化を監視。
    /// 監視開始する場合は、常にSuccess
    /// 監視2回目以降は、いずれかの項目について変化が確認できたらSuccess
    /// 複数パスを監視する場合、どれか1つでも変化が確認できたらSuccess
    /// </summary>
    internal class Watch : AuditTaskWork
    {
        [TaskParameter(Mandatory = true)]
        [Keys("serial", "uniquekey", "id")]
        protected string _Serial { get; set; }

        [TaskParameter(Mandatory = true, ResolvEnv = true, Delimiter = ';')]
        [Keys("path", "filepath", "target", "targetpath")]
        protected string[] _Path { get; set; }

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
        [Keys("begin", "start")]
        protected bool _IsStart { get; set; }

        [TaskParameter]
        [Keys("invert", "not", "no", "none")]
        protected bool _Invert { get; set; }

        private int _serial = 0;
        private WatchDataCollection _collection = null;

        public override void MainProcess()
        {
            this.Success = true;
            var dictionary = new Dictionary<string, string>();
            dictionary["watchTarget"] = string.Join(", ", _Path);

            //  複数ファイルを指定している場合、どれか1つでも変更した場合はSuccess
            //  WatchDataが無い場合は監視開始
            _collection = _IsStart ? new WatchDataCollection() : LoadWatchDB(_Serial);

            foreach (string path in _Path)
            {
                _serial++;
                WatchData watch = _collection.GetWatchData(path);

                if (System.IO.File.Exists(path))
                {
                    Success &= WatchExists(path, dictionary, watch);
                    if (_IsCreationTime ?? false) { Success &= WatchTimeStamp(path, dictionary, watch, "creation"); }
                    if (_IsLastWriteTime ?? false) { Success &= WatchTimeStamp(path, dictionary, watch, "lastwrite"); }
                    if (_IsLastAccessTime ?? false) { Success &= WatchTimeStamp(path, dictionary, watch, "lastaccess"); }
                }
                _collection.SetWatchData(path, watch);
            }

            AddAudit(dictionary, this._Invert);
        }

        #region Watch Exists

        /// <summary>
        /// 存在有無チェック
        /// </summary>
        /// <param name="path"></param>
        /// <param name="dictionary"></param>
        /// <param name="watch"></param>
        /// <returns></returns>
        private bool WatchExists(string path, Dictionary<string, string> dictionary, WatchData watch)
        {
            bool exists = System.IO.File.Exists(path);

            dictionary[$"file_Exists_{_serial}"] = exists.ToString();
            bool ret = watch.Exists == null || watch.Exists == exists;
            watch.Exists = exists;
            return ret;
        }

        #endregion
        #region Watch Date

        /// <summary>
        /// 作成/更新/最終アクセス日時チェック
        /// </summary>
        /// <param name="path"></param>
        /// <param name="dictionary"></param>
        /// <param name="watch"></param>
        /// <param name="dateType"></param>
        /// <returns></returns>
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

            switch (dateType)
            {
                case "creation":
                    string creation = processDate(System.IO.File.GetCreationTime(path));

                    dictionary[$"file_CreationTime_{_serial}"] = creation;
                    bool ret_creation = watch.CreationTime == null || watch.CreationTime == creation;
                    watch.CreationTime = creation;
                    return ret_creation;
                case "lastwrite":
                    string lastWrite = processDate(System.IO.File.GetCreationTime(path));

                    dictionary[$"file_LastWriteTime_{_serial}"] = lastWrite;
                    bool ret_lastWrite = watch.LastWriteTime == null || watch.LastWriteTime == lastWrite;
                    watch.LastWriteTime = lastWrite;
                    return ret_lastWrite;
                case "lastaccess":
                    string lastAccess = processDate(System.IO.File.GetCreationTime(path));

                    dictionary[$"file_LastWriteTime_{_serial}"] = lastAccess;
                    bool ret_lastAccess = watch.LastAccessTime == null || watch.LastAccessTime == lastAccess;
                    watch.LastAccessTime = lastAccess;
                    return ret_lastAccess;
            }

            return false;
        }

        #endregion
        #region Watch Access

        private bool watchAccess(string path, Dictionary<string, string> dictionary, WatchData watch)
        {

            return false;
        }

        #endregion
    }
}
