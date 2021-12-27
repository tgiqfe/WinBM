
namespace OSVersion
{
    internal class Item
    {
        private static readonly string _WorkDir = Environment.UserName == "SYSTEM" ?
            Path.Combine(Environment.GetEnvironmentVariable("ProgramData"), "WinBM") :
            Path.Combine(Path.GetTempPath(), "WinBM");

        public const string OSVERSION_OSVERSIONINFODBFILE = "OSVersion_OSVersionInfoDBFile";

        public static string GetDefaultOSVersionInfoDbFile()
        {
            return System.IO.Path.Combine(_WorkDir, "OSVersion", "OSVersionInfoCollection.json");
        }
    }
}
