
namespace WinBM.PowerShell.Lib.TestWinBMYaml
{
    internal enum LineType
    {
        None,
        Comment,
        Kind,
        Metadata,
        EnvSpec,
        EnvSpecParam,
        ConfigSpec,
        ConfigSpecParam,
        OutputSpec,
        OutputSpecParam,
        JobRequire,
        JobrequireParam,
        JobWork,
        JobWorkParam,
    }
}
