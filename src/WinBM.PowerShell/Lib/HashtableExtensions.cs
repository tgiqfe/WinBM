using System.Collections;
using System.Collections.Generic;

namespace WinBM.PowerShell.Lib
{
    /// <summary>
    /// Hashtable型の拡張メソッド
    /// </summary>
    static partial class HashtableExtensions
    {
        /// <summary>
        /// Hashtableの中身をDictionary&lt;string, string&gt;型に変換する。
        /// </summary>
        /// <param name="hashtable"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ToDictionary(this Hashtable hashtable)
        {
            if (hashtable == null)
            {
                return null;
            }

            var ret = new Dictionary<string, string>();
            foreach (DictionaryEntry entry in hashtable)
            {
                ret[entry.Key.ToString()] = entry.Value.ToString();
            }
            return ret;
        }
    }
}
