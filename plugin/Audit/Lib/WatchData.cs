using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audit.Lib
{
    public class WatchData
    {
        public DateTime? CreationTime { get; set; }
        public DateTime? LastWriteTime { get; set; }
        public DateTime? LastAccessTime { get; set; }
        public string Access { get; set; }
        public string Owner { get; set; }
        public bool? Inherited { get; set; }
        public string[] Attributes { get; set; }
        public string MD5Hash { get; set; }
        public string SHA256Hash { get; set; }
        public string SHA512Hash { get; set; }
        public long? Size { get; set; }
        public bool Exists { get; set; }
    }

    public class WatchDataCollection : Dictionary<string, WatchData>
    {
        //  key⇒ファイル/ディレクトリ/レジストリのパス
        //  Value ⇒WatchData

        public WatchData GetParameter(string key)
        {
            string matchKey = this.Keys.FirstOrDefault(x => x.Equals(key, StringComparison.OrdinalIgnoreCase));
            return matchKey == null ? 
                new WatchData() : 
                this[matchKey];
        }

        public void SetCreationTime(string key, DateTime creationTime)
        {
            GetParameter(key).CreationTime = creationTime;
        }
    }
}
