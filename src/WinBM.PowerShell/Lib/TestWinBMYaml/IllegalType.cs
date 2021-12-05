
namespace WinBM.PowerShell.Lib.TestWinBMYaml
{
    internal enum IllegalType
    {
        None,       //  Illegal箇所無し
        Key,        //  キーの記述間違い
        Value,      //  値の記述間違い
        Dll,        //  DLLファイルの指定間違い ※現在DLL判定チェックは未実装
    }
}
