using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinBM.Task
{
    public class ValidateEnumSetAttribute : Attribute
    {
        private string[] _ValidateSet { get; set; }

        /// <summary>
        /// 許可する文字列を指定。マッチしなければNG。
        /// ToString()した結果で判定する為、string,Enum,int,long,double,boolでチェック可能。
        /// string[]やDictionary&lt;string, string&gt;は無理
        /// </summary>
        /// <param name="validateSet"></param>
        public ValidateEnumSetAttribute(params string[] validateSet)
        {
            this._ValidateSet = validateSet;
        }

        public bool Contains(string enumStr)
        {
            return _ValidateSet.Contains(enumStr, StringComparer.OrdinalIgnoreCase);
        }
    }
}
