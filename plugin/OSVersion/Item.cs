
namespace OSVersion
{
    internal class Item
    {
        public const string OSVERSION_OSVERSIONINFODBFILE = "OSVersion_OSVersionInfoDBFile";

        public static string GetDefaultOSVersionInfoDbFile()
        {
            return System.IO.Path.Combine(WinBM.GlobalSetting.WorkDir, "OSVersionInfoCollection.json");
        }
    }
}
