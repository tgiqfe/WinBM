﻿
namespace WinBM.PowerShell.Lib.TestWinBMYaml
{
    internal enum LineType
    {
        None,
        Comment,
        Kind,
        Metadata,
        InitSpec,
        InitSpecParam,
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
