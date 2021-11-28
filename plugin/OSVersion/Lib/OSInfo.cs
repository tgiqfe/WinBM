using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSVersion.Lib.Windows;
using OSVersion.Lib.Linux;

namespace OSVersion.Lib
{
    internal class OSInfo : Arithmetic
    {
        /// <summary>
        /// OSの名前
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// OSファミリー (Windows/Mac/Linux)
        /// </summary>
        public OSFamily OSFamily { get; set; }

        /// <summary>
        /// OSのバージョンの名前。一番通りの良い名前。
        /// </summary>
        public string VersionName { get; set; }

        /// <summary>
        /// OSバージョン名のその他の名前。開発時のコードネームも含む
        /// </summary>
        public string[] Alias { get; set; }

        /// <summary>
        /// バージョン
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// エディション
        /// </summary>
        public Edition? Edition { get; set; }

        /// <summary>
        /// Linuxディストリビューション
        /// </summary>
        public Distribution? Distribution { get; set; }

        /// <summary>
        /// リリース日
        /// </summary>
        public DateTime ReleaseDate { get; set; }

        /// <summary>
        /// サポート終了日
        /// </summary>
        public DateTime? EndSupportDate { get; set; }

        /// <summary>
        /// サーバOSであるかどうか
        /// </summary>
        public bool IsServer { get; set; }

        /// <summary>
        /// 組み込み用OSであるかどうか
        /// </summary>
        public bool IsEmbedded { get; set; }

        /// <summary>
        /// Alismeticで判定時に使用するGroup
        /// </summary>
        public override string Group
        {
            get
            {
                return this.OSFamily == OSFamily.Linux ?
                    $"{OSFamily} {Distribution}" :
                    this.OSFamily.ToString();
            }
        }

        /// <summary>
        /// メジャーバージョン
        /// </summary>
        public string MajorVersion
        {
            get { return Version.Contains(".") ? Version.Split('.')[0] : Version; }
        }

        /// <summary>
        /// マイナーバージョン
        /// </summary>
        public string MinorVersion
        {
            get { return Version.Contains(".") && Version.Split('.').Length > 1 ? Version.Split('.')[1] : Version; }
        }

        /// <summary>
        /// ビルド番号
        /// </summary>
        public string BuildVersion
        {
            get { return Version.Contains(".") && Version.Split('.').Length > 2 ? Version.Split('.')[2] : Version; }
        }

        public string RevisionVersion
        {
            get { return Version.Contains(".") && Version.Split('.').Length > 3 ? Version.Split('.')[3] : Version; }
        }

        /// <summary>
        /// 引数無しコンストラクタ
        /// </summary>
        public OSInfo() { }

        /// <summary>
        /// 文字列化
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}{1} ver.{2}",
                Name,
                Edition == null || Edition == Windows.Edition.None ?
                    "" :
                    " " + Edition,
                VersionName);
        }
    }
}
