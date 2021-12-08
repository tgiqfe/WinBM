﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using IO.Lib;

namespace Audit.Lib
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class MonitorRegistryType
    {
        #region Compare method



        #endregion
        #region Watch method

        public static bool WatchRegistryValue(
            WatchPath watch, Dictionary<string, string> dictionary, int serial, RegistryKey regKey, string name)
        {
            bool ret = false;
            if (watch.IsRegistryType ?? false)
            {
                if (regKey != null && regKey.GetValueNames().Any(x => x.Equals(name, StringComparison.OrdinalIgnoreCase)))
                {
                    string ret_string = RegistryControl.ValueKindToString(regKey.GetValueKind(name));
                    ret = ret_string != watch.RegistryType;
                    if (watch.RegistryType != null)
                    {
                        string pathType = "registry";
                        string checkTarget = "RegistryType";
                        dictionary[$"{pathType}_{checkTarget}_{serial}"] = ret ?
                            $"{watch.RegistryType} -> {ret_string}" :
                            ret_string;
                    }
                    watch.RegistryType = ret_string;
                }
                else
                {
                    watch.RegistryType = null;
                }
            }
            return ret;
        }


        #endregion
        #region Get method

        #endregion
    }
}
