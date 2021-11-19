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
    internal class Watch : AuditTaskWork
    {
        [TaskParameter(Mandatory = true)]
        [Keys("serial", "uniquekey")]
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
            var dictionary = new Dictionary<string, string>();
            dictionary["watchTarget"] = string.Join(", ", _Path);

            //  複数ファイルを指定している場合、どれか1つでも変更した場合はSuccess
            //  WatchDataが無い場合は監視開始
            _collection = _IsStart ? new WatchDataCollection() : LoadWatchDB(_Serial);



        }

        private void WatchTimeStamp(string path, Dictionary<string, string> dictionary, string dateType)
        {
            string checkTarget = "";
            DateTime? path_date = null;
            switch (dateType)
            {
                case "creation":
                    checkTarget = "CreationTime";
                    path_date = System.IO.File.GetCreationTime(path);




                    _collection.GetParameter(path).CreationTime = path_date;
                    break;
                case "lastwrite":
                    checkTarget = "LastWriteTime";
                    path_date = System.IO.File.GetLastWriteTime(path);
                    _collection.GetParameter(path).LastWriteTime = path_date;
                    break;
                case "lastaccess":
                    checkTarget = "LastAccessTime";
                    path_date = System.IO.File.GetLastAccessTime(path);
                    _collection.GetParameter(path).LastAccessTime = path_date;
                    break;
            }




        }
    }
}
