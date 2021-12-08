using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Principal;
using System.Security.AccessControl;
using Microsoft.Win32;
using System.Security.Cryptography;
using IO.Lib;

namespace Audit.Lib
{
    public class WatchPath
    {
        /// <summary>
        /// ファイル/ディレクトリ/レジストリいずれか
        /// </summary>
        public PathType PathType { get; set; }

        /// <summary>
        /// ファイル/ディレクトリ/レジストリキーの場合は、Targetへのパス
        /// レジストリ値の場合は、パスの頭に[registry]を付けて管理
        /// </summary>
        public string FullPath { get; set; }
        
        /// <summary>
        /// 監視中Targetの各情報。
        /// </summary>
        public string CreationTime { get; set; }
        public string LastWriteTime { get; set; }
        public string LastAccessTime { get; set; }
        public string Access { get; set; }
        public string Owner { get; set; }
        public bool? Inherited { get; set; }
        public bool[] Attributes { get; set; }
        public string MD5Hash { get; set; }
        public string SHA256Hash { get; set; }
        public string SHA512Hash { get; set; }
        public long? Size { get; set; }
        public int[] ChildCount { get; set; }
        public string RegistryType { get; set; }
        public bool? Exists { get; set; }

        /// <summary>
        /// 各情報について、監視対象であるかどうか
        /// </summary>
        public bool? IsCreationTime { get; set; }
        public bool? IsLastWriteTime { get; set; }
        public bool? IsLastAccessTime { get; set; }
        public bool? IsAccess { get; set; }
        public bool? IsOwner { get; set; }
        public bool? IsInherited { get; set; }
        public bool? IsAttributes { get; set; }
        public bool? IsMD5Hash { get; set; }
        public bool? IsSHA256Hash { get; set; }
        public bool? IsSHA512Hash { get; set; }
        public bool? IsSize { get; set; }
        public bool? IsChildCount { get; set; }
        public bool? IsRegistryType { get; set; }
        public bool? IsDateOnly { get; set; }
        public bool? IsTimeOnly { get; set; }

        public const string REGPATH_PREFIX = "[registry]";

        public WatchPath(PathType pathType)
        {
            PathType = pathType;
        }
    }
}
