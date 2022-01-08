using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace IO.Lib
{
    class RegistryHive
    {
        #region Const

        //  KeyCode
        private const uint HKEY_CLASSES_ROOT = 0x80000000;
        private const uint HKEY_CURRENT_USER = 0x80000001;
        private const uint HKEY_LOCAL_MACHINE = 0x80000002;
        private const uint HKEY_USERS = 0x80000003;

        #endregion

        [DllImport("advapi32.dll", CharSet = CharSet.Ansi)]
        private static extern int RegLoadKey(uint hKey, string lpSubKey, string lpFile);
        [DllImport("advapi32.dll", CharSet = CharSet.Ansi)]
        private static extern int RegUnLoadKey(uint hKey, string lpSubKey);

        /// <summary>
        /// レジストリハイブファイルをロード
        /// </summary>
        /// <param name="hiveName"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool LoadHive2(string hiveName, string filePath)
        {
            if (hiveName.Contains("\\"))
            {
                switch (hiveName.Substring(0, hiveName.IndexOf("\\")))
                {
                    case "HKEY_USERS":
                    case "USERS":
                    case "HKU":
                        hiveName = hiveName.Substring(hiveName.LastIndexOf("\\") + 1);
                        break;
                    default:
                        return false;
                }
            }

            TokenManipulator.AddPrivilege(TokenManipulator.SE_BACKUP_NAME);
            TokenManipulator.AddPrivilege(TokenManipulator.SE_RESTORE_NAME);
            bool ret = RegLoadKey(HKEY_USERS, hiveName, filePath) == 0;
            TokenManipulator.RemovePrivilege(TokenManipulator.SE_BACKUP_NAME);
            TokenManipulator.RemovePrivilege(TokenManipulator.SE_RESTORE_NAME);

            return ret;
        }

        /// <summary>
        /// ロードしたキーをアンロード
        /// </summary>
        /// <param name="hiveName"></param>
        /// <returns></returns>
        public static bool UnloadHive2(string hiveName)
        {
            if (hiveName.Contains("\\"))
            {
                switch (hiveName.Substring(0, hiveName.IndexOf("\\")))
                {
                    case "HKEY_USERS":
                    case "USERS":
                    case "HKU":
                        hiveName = hiveName.Substring(hiveName.LastIndexOf("\\") + 1);
                        break;
                    default:
                        return false;
                }
            }

            TokenManipulator.AddPrivilege(TokenManipulator.SE_BACKUP_NAME);
            TokenManipulator.AddPrivilege(TokenManipulator.SE_RESTORE_NAME);
            bool ret = RegUnLoadKey(HKEY_USERS, hiveName) == 0;
            TokenManipulator.RemovePrivilege(TokenManipulator.SE_BACKUP_NAME);
            TokenManipulator.RemovePrivilege(TokenManipulator.SE_RESTORE_NAME);

            return ret;
        }
    }
}

