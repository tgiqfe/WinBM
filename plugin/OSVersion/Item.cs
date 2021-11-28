
namespace OSVersion
{
    internal class Item
    {
        public const string OSVERSION_OSINFODBFILE = "OSVersion_OSInfoDBFile";

        public static string GetDefaultOSInfoDbFile()
        {
            return System.IO.Path.Combine(WinBM.GlobalSetting.WorkDir, "OSInfoCollection.json");
        }
    }
}
