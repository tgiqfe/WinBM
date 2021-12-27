
namespace Audit
{
    internal class Item
    {
        private static readonly string _WorkDir = Environment.UserName == "SYSTEM" ?
            Path.Combine(Environment.GetEnvironmentVariable("ProgramData"), "WinBM") :
            Path.Combine(Path.GetTempPath(), "WinBM");

        public const string AUDIT_MONITORFILE = "Audit_MonitorFile";
        public const string AUDIT_SINCEDBFILE = "Audit_SinceDBFile";
        public const string AUDIT_WATCHDBDIR = "Audit_WatchDBDir";

        public static string GetDefaultMonitorFile()
        {
            return System.IO.Path.Combine(_WorkDir, "Audit", "AuditMonitor.json");
        }
        public static string GetDefaultSinceDBFile()
        {
            return System.IO.Path.Combine(_WorkDir, "Audit", "SinceDB.json");
        }
        public static string GetDefaultWatchDBDir()
        {
            return System.IO.Path.Combine(_WorkDir, "Audit", "WatchDB");
        }
    }
}
