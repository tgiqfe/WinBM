using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IO.Lib;

namespace Audit.Lib
{
    internal class ComparePath
    {
        /// <summary>
        /// ファイル/ディレクトリ/レジストリいずれか
        /// </summary>
        public PathType PathType { get; set; }

        /// <summary>
        /// 比較する対象のパス(レジストリ値の場合はNameも)
        /// </summary>
        public string PathA { get; set; }
        public string PathB { get; set; }
        public string NameA { get; set; }
        public string NameB { get; set; }

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

        public ComparePath(PathType pathType)
        {
            PathType = pathType;    
        }
    }
}
