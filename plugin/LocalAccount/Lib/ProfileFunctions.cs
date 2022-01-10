using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Management;
using Microsoft.Win32;
using System.Security.Principal;

namespace LocalAccount.Lib
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class ProfileFunctions
    {
        /// <summary>
        /// 指定したユーザーのプロファイルパスを取得
        /// </summary>
        /// <param name="userName"></param>
        /// <returns>プロファイルパス。存在しないユーザーの場合はnullを返す</returns>
        public static string GetProfilePath(string userName)
        {
            string sid = new NTAccount(userName).Translate(typeof(SecurityIdentifier))?.Value;
            if (sid == null)
            {
                return null;
            }

            return new ManagementClass("Win32_UserProfile").
                GetInstances().
                OfType<ManagementObject>().
                Where(x => x["SID"] as string == sid).
                Select(x => x["LocalPath"] as string).
                First();
        }

        /// <summary>
        /// Defaultプロファイルパスを取得
        /// </summary>
        /// <returns></returns>
        public static string GetDefaultProfilePath()
        {
            return Registry.GetValue(
                @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList",
                "Default",
                "") as string;
        }

        /// <summary>
        /// プロファイル削除
        /// </summary>
        /// <param name="userName"></param>
        public static void DeleteProfile(string userName)
        {
            string sid = new NTAccount(userName).Translate(typeof(SecurityIdentifier))?.Value;
            if (sid == null)
            {
                return;
            }
            DeleteProfileFromSID(sid);
        }

        /// <summary>
        /// プロファイル削除(SID直接指定)。システムアカウントは除外
        /// </summary>
        /// <param name="sid"></param>
        public static void DeleteProfileFromSID(string sid)
        {
            string[] sysSIDs = new ManagementClass("Win32_SystemAccount").
                GetInstances().
                OfType<ManagementObject>().
                Select(x => x["SID"] as string).
                ToArray();
            if (sysSIDs.All(x => x != sid))
            {
                var profile = new ManagementClass("Win32_UserProfile").
                    GetInstances().
                    OfType<ManagementObject>().
                    FirstOrDefault(x => sid == (x["SID"]?.ToString()));
                profile.Delete();
            }
        }

        /// <summary>
        /// 除外対象のユーザーとシステムアカウント以外の全プロファイルを削除
        /// </summary>
        /// <param name="exclude"></param>
        public static void DeleteProfileAll(string[] exclude)
        {
            string[] excludeSIDs = exclude.
                Select(x => new NTAccount(x).Translate(typeof(SecurityIdentifier))?.Value).
                Where(x => !string.IsNullOrEmpty(x)).
                ToArray();
            string[] sysSIDs = new ManagementClass("Win32_SystemAccount").
                GetInstances().
                OfType<ManagementObject>().
                Select(x => x["SID"] as string).
                ToArray();

            foreach (var profile in new ManagementClass("Win32_UserProfile").
                GetInstances().
                OfType<ManagementObject>())
            {
                string sid = profile["SID"] as string;
                if (excludeSIDs.All(x => x != sid) && sysSIDs.All(x => x != sid))
                {
                    profile.Delete();
                }
            }
        }


        /// <summary>
        /// 現在ログオン中のユーザー一覧を取得
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetLoggedOnUsers()
        {
            List<string> userList = new List<string>();
            foreach (ManagementObject mo in new ManagementClass("Win32_LoggedOnUser").
                GetInstances().
                OfType<ManagementObject>())
            {
                //  DWM-*やUMFD-*という名前のユーザーが取得できるけれど、
                //  NameプロパティにアクセスしようとするだけでManagementExceptionする。
                //  Nameプロパティのアクセスより前に判別する手段が見つからないので、
                //  try～catchで囲うだけにしておく。
                try
                {
                    ManagementObject moA = new ManagementObject(mo["Antecedent"] as string);
                    userList.Add(moA["Name"] as string);
                }
                catch { }
            }
            return userList;
        }
    }
}
