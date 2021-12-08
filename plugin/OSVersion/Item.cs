
namespace OSVersion
{
    internal class Item
    {
        public const string OSVERSION_OSINFODBFILE = "OSVersion_OSVersionInfoDBFile";

        public static string GetDefaultOSInfoDbFile()
        {
            return System.IO.Path.Combine(WinBM.GlobalSetting.WorkDir, "OSVersionInfoCollection.json");
        }
    }
}
