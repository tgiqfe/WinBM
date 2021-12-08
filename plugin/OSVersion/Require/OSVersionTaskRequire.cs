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
        private static string _OSVersionInfoDBFile = null;

        protected OSVersionInfoCollection LoadOSVersionInfoDB()
        {
            if (_OSVersionInfoDBFile == null)
            {
                if ((Manager.Setting.PluginParam?.ContainsKey(Item.OSVERSION_OSVERSIONINFODBFILE) ?? false) &&
                    !string.IsNullOrEmpty(Manager.Setting.PluginParam[Item.OSVERSION_OSVERSIONINFODBFILE]))
                {
                    _OSVersionInfoDBFile = Manager.Setting.PluginParam[Item.OSVERSION_OSVERSIONINFODBFILE];
                }
                else
                {
                    _OSVersionInfoDBFile = Item.GetDefaultOSVersionInfoDbFile();
                }
            }
            return OSVersionInfoCollection.Load(_OSVersionInfoDBFile);
        }
    }
}
