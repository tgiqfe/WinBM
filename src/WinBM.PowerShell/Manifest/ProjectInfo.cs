using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace WinBM.PowerShell.Manifest
{
    public enum Mode { Debug, Release }

    class ProjectInfo
    {
        //  プロジェクト名
        public readonly string ProjectName = Assembly.GetExecutingAssembly().GetName().Name;

        //  ターゲットFramework
        //const string _TargetFramework = "net5.0-windows";
        //const string _TargetFramework = "net5.0";
        const string _TargetFramework = "net6.0";

        //  デバッグ/リリースのモード指定
        public Mode Mode { get; set; }

        public string TargetDir { get { return string.Format(@"..\..\{0}\{1}", Mode, _TargetFramework); } }
        public string ModuleDir { get { return string.Format(@"..\..\{0}", ProjectName); } }
        public string ScriptDir { get { return string.Format(@"..\..\..\Script"); } }
        public string FormatDir { get { return string.Format(@"..\..\..\Format"); } }
        public string HelpDir { get { return string.Format(@"..\..\..\Help"); } }
        public string CmdletDir { get { return string.Format(@"..\..\..\Cmdlet"); } }
        public string PluginDir { get { return string.Format(@"..\..\..\Plugin"); } }

        public string DllFile { get { return string.Format(@"..\..\{0}\{1}\{2}.dll", Mode, _TargetFramework, ProjectName); } }
        public string Psd1File { get { return string.Format(@"..\..\{0}\{1}\{2}.psd1", Mode, _TargetFramework, ProjectName); } }
        public string Psm1File { get { return string.Format(@"..\..\{0}\{1}\{2}.psm1", Mode, _TargetFramework, ProjectName); } }

        public string Description { get; set; } = "Windows Building Manager";
        public string Author { get; set; } = "q";
        public string CompanyName { get; set; } = "q";
        public string Copyright { get; set; } = null;

        public string[] ExcludeCmdlet { get; set; } = new string[]{
            "SaveManifest.cs"
        };

        public ProjectInfo() { }
    }
}
