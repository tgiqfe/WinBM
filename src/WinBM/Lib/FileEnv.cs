﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinBM.Lib
{
    /// <summary>
    /// Recipeファイル内でのみ使用する環境変数を実装
    /// </summary>
    public class FileEnv
    {
        /// <summary>
        /// 対象Recipeファイル。
        /// ここで指定したファイル内でのみ使用可能な疑似環境変数とする
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 疑似環境変数名
        /// </summary>
        private string _Name = null;
        public string Name
        {
            get { return this._Name; }
            set { this._Name = "%" + value + "%"; }
        }

        /// <summary>
        /// 疑似環境変数の値
        /// </summary>
        public string Value { get; set; }

        public bool IsMathPath(string path)
        {
            return this.Path.Equals(path, StringComparison.OrdinalIgnoreCase);
        }

        public void Resolv(ref string text)
        {
            if (text.Contains(this.Name, StringComparison.OrdinalIgnoreCase))
            {
                text = System.Text.RegularExpressions.Regex.Replace(
                    text,
                    this.Name,
                    this.Value,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }
        }
    }
}
