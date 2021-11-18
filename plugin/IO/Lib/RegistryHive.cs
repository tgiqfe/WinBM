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

        /*
        private const int SE_PRIVILEGE_ENABLED = 0x00000002;
        private const string SE_RESTORE_NAME = "SeRestorePrivilege";
        private const string SE_BACKUP_NAME = "SeBackupPrivilege";
        */


        #region Const

        //  KeyCode
        private const uint HKEY_CLASSES_ROOT = 0x80000000;
        private const uint HKEY_CURRENT_USER = 0x80000001;
        private const uint HKEY_LOCAL_MACHINE = 0x80000002;
        private const uint HKEY_USERS = 0x80000003;

        #endregion

        /*
        //  DesiredAccess
        public const uint STANDARD_RIGHTS_REQUIRED = 0x000F0000;
        public const uint STANDARD_RIGHTS_READ = 0x00020000;
        public const uint TOKEN_ASSIGN_PRIMARY = 0x0001;
        public const uint TOKEN_DUPLICATE = 0x0002;
        public const uint TOKEN_IMPERSONATE = 0x0004;
        public const uint TOKEN_QUERY = 0x0008;
        public const uint TOKEN_QUERY_SOURCE = 0x0010;
        public const uint TOKEN_ADJUST_PRIVILEGES = 0x0020;
        public const uint TOKEN_ADJUST_GROUPS = 0x0040;
        public const uint TOKEN_ADJUST_DEFAULT = 0x0080;
        public const uint TOKEN_ADJUST_SESSIONID = 0x0100;
        public const uint TOKEN_READ = (STANDARD_RIGHTS_READ | TOKEN_QUERY);
        public const uint TOKEN_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | TOKEN_ASSIGN_PRIMARY |
            TOKEN_DUPLICATE | TOKEN_IMPERSONATE | TOKEN_QUERY | TOKEN_QUERY_SOURCE |
            TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_DEFAULT |
            TOKEN_ADJUST_SESSIONID);

        

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();
        [DllImport("advapi32.dll")]
        private static extern int OpenProcessToken(IntPtr processHandle, uint desiredAccess, ref IntPtr tokenhandle);
        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr handle);
        [DllImport("advapi32.dll", CharSet = CharSet.Ansi)]
        private static extern int LookupPrivilegeValueA(string lpsystemname, string lpname, [MarshalAs(UnmanagedType.Struct)] ref LUID lpLuid);
        [DllImport("advapi32.dll")]
        private static extern int AdjustTokenPrivileges(IntPtr tokenhandle, bool disableprivs, [MarshalAs(UnmanagedType.Struct)] ref TOKEN_PRIVILEGES newstate, int bufferlength, IntPtr preivousState, int returnlength);
        */

        [DllImport("advapi32.dll", CharSet = CharSet.Ansi)]
        private static extern int RegLoadKey(uint hKey, string lpSubKey, string lpFile);
        [DllImport("advapi32.dll", CharSet = CharSet.Ansi)]
        private static extern int RegUnLoadKey(uint hKey, string lpSubKey);

        /*
        [StructLayout(LayoutKind.Sequential)]
        struct LUID
        {
            public uint LowPart;
            public uint HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TOKEN_PRIVILEGES
        {
            public int PrivilegeCount;
            public LUID Luid;
            public int Attributes;
        }

        private static bool EnabledPrivilege()
        {
            IntPtr tokenHandle = IntPtr.Zero;
            if (OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES, ref tokenHandle) == 0)
            {
                return false;
            }

            AdjustTokenPrivilege(tokenHandle, SE_BACKUP_NAME);
            AdjustTokenPrivilege(tokenHandle, SE_RESTORE_NAME);
            CloseHandle(tokenHandle);

            return true;
        }

        private static bool AdjustTokenPrivilege(IntPtr tokenHandle, string lpname)
        {
            var luid = new LUID();
            if (LookupPrivilegeValueA(null, lpname, ref luid) == 0)
            {
                return false;
            }
            var serTokenp = new TOKEN_PRIVILEGES
            {
                PrivilegeCount = 1,
                Luid = luid,
                Attributes = SE_PRIVILEGE_ENABLED
            };
            if (AdjustTokenPrivileges(tokenHandle, false, ref serTokenp, 0, IntPtr.Zero, 0) == 0)
            {
                return false;
            }

            return true;
        }
        */

        /*
        /// <summary>
        /// レジストリハイブファイルをロード
        /// </summary>
        /// <param name="hivename"></param>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static bool LoadHive(string hivename, string filepath)
        {
            if (!EnabledPrivilege()) { return false; }
            return RegLoadKey(HKEY_USERS, hivename, filepath) == 0;
        }

        /// <summary>
        /// ロードしたキーをアンロード
        /// </summary>
        /// <param name="hivename"></param>
        /// <returns></returns>
        public static bool UnloadHive(string hivename)
        {
            if (!EnabledPrivilege()) { return false; }
            return RegUnLoadKey(HKEY_USERS, hivename) == 0;
        }
        */



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

