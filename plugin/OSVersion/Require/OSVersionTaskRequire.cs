using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using OSVersion.Lib;

namespace OSVersion.Require
{
    internal class OSVersionTaskRequire : TaskJob
    {
        private static string _OSInfoDBFile = null;

        protected OSVersionInfoCollection LoadOSInfoDB()
        {
            if (_OSInfoDBFile == null)
            {
                if ((Manager.Setting.PluginParam?.ContainsKey(Item.OSVERSION_OSINFODBFILE) ?? false) &&
                    !string.IsNullOrEmpty(Manager.Setting.PluginParam[Item.OSVERSION_OSINFODBFILE]))
                {
                    _OSInfoDBFile = Manager.Setting.PluginParam[Item.OSVERSION_OSINFODBFILE];
                }
                else
                {
                    _OSInfoDBFile = Item.GetDefaultOSInfoDbFile();
                }
            }
            return OSVersionInfoCollection.Load(_OSInfoDBFile);
        }
    }
}
