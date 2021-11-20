using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IO.Lib;

namespace Audit.Lib
{
    /// <summary>
    /// Watch関連のクラス。Registry関連で詰まったので、一旦凍結。
    /// </summary>
    public class WatchData
    {
        public PathType PathType { get; set; }

        public string CreationTime { get; set; }
        public string LastWriteTime { get; set; }
        public string LastAccessTime { get; set; }
        public string Access { get; set; }
        public string Owner { get; set; }
        public bool? Inherited { get; set; }
        public string[] Attributes { get; set; }
        public string MD5Hash { get; set; }
        public string SHA256Hash { get; set; }
        public string SHA512Hash { get; set; }
        public long? Size { get; set; }
        public string RegistryType { get; set; }
        public bool? Exists { get; set; }

        public WatchData(PathType pathType)
        {
            PathType = pathType;
        }
    }

    public class WatchDataCollection : Dictionary<string, WatchData>
    {
        public WatchData GetWatchData(string path, PathType pathType)
        {
            string matchKey = this.Keys.FirstOrDefault(x => x.Equals(path, StringComparison.OrdinalIgnoreCase));
            return matchKey == null ?
                new WatchData(pathType) :
                this[matchKey];
        }

        public void SetWatchData(string path, WatchData watchData)
        {
            this[path] = watchData;
        }
    }
}
