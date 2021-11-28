using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;

namespace Audit.Lib
{
    public class WatchPathCollection : Dictionary<string, WatchPath>
    {
        /// <summary>
        /// SetWatchPath()時に格納。
        /// 未チェックパスの確認要に使用。
        /// </summary>
        private List<string> _CheckedKeys = new List<string>();

        /// <summary>
        /// Targetのパス(文字列)から、WatchPathを取得
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public WatchPath GetWatchPath(string path)
        {
            string matchKey = this.Keys.FirstOrDefault(x => x.Equals(path, StringComparison.OrdinalIgnoreCase));
            return matchKey == null ? null : this[matchKey];
        }

        /// <summary>
        /// Targetに対して、WatchPathを格納
        /// </summary>
        /// <param name="path"></param>
        /// <param name="watchPath"></param>
        public void SetWatchPath(string path, WatchPath watchPath)
        {
            watchPath.FullPath = path;
            this[path] = watchPath;
            this._CheckedKeys.Add(path);
        }

        /// <summary>
        /// SetWatchPath()でまだ更新していないパスを返す。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetUncheckedKeys()
        {
            return this.Keys.Where(x => !_CheckedKeys.Any(y => y.Equals(x, StringComparison.OrdinalIgnoreCase)));
        }

        #region Load/Save

        /// <summary>
        /// 指定したIDからWatchDBを読み込み
        /// </summary>
        /// <param name="dbDir"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static WatchPathCollection Load(string dbDir, string id)
        {
            try
            {
                using (var sr = new StreamReader(Path.Combine(dbDir, id), Encoding.UTF8))
                {
                    return JsonSerializer.Deserialize<WatchPathCollection>(sr.ReadToEnd());
                }
            }
            catch { }
            return new WatchPathCollection();
        }

        /// <summary>
        /// 指定したIDでWatchDBを保存
        /// </summary>
        /// <param name="dbDir"></param>
        /// <param name="id"></param>
        public void Save(string dbDir, string id)
        {
            if (!Directory.Exists(dbDir))
            {
                Directory.CreateDirectory(dbDir);
            }
            using (var sw = new StreamWriter(Path.Combine(dbDir, id), false, Encoding.UTF8))
            {
                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions()
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                });
                sw.WriteLine(json);
            }
        }

        #endregion
    }
}
