namespace WinBM
{
    public enum LogLevel
    {
        /// <summary>
        /// 初期値。特に指定の無い状態
        /// </summary>
        None = -2,

        /// <summary>
        /// デバッグ情報。try～catch句でcatch(Exception){}内でのみ使用する程度を想定
        /// </summary>
        Debug = -1,

        /// <summary>
        /// 通常情報
        /// </summary>
        Info = 0,

        /// <summary>
        /// Requireで前提条件チェックの失敗時に使用
        /// </summary>
        Attention = 1,

        /// <summary>
        /// 軽度の問題。パラメータ不足、等
        /// </summary>
        Warn = 2,

        /// <summary>
        /// 重度の問題
        /// </summary>
        Error = 3,
    }
}
