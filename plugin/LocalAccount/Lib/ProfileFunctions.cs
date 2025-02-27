﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Management;
using Microsoft.Win32;
using System.Security.Principal;
using System.IO;

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
            try
            {
                string sid = new NTAccount(userName).Translate(typeof(SecurityIdentifier)).Value;
                return new ManagementClass("Win32_UserProfile").
                    GetInstances().
                    OfType<ManagementObject>().
                    Where(x => x["SID"] as string == sid).
                    Select(x => x["LocalPath"] as string).
                    First();
            }
            catch
            {
                return null;
            }
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
        /// Win32_LoggedOnUserでは、一度ログオンしたユーザーの情報が再起動するまで残ってしまう為、
        /// query userコマンドで確認する。
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetLoggedOnUsers()
        {
            var list = new List<string>();

            string ret = "";
            using (var proc = new Process())
            {
                proc.StartInfo.FileName = "query.exe";
                proc.StartInfo.Arguments = "user";
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.Start();

                ret = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();
            }

            using (var sr = new StringReader(ret))
            {
                string readLine = "";
                bool during = false;
                while ((readLine = sr.ReadLine()) != null)
                {
                    string[] fields = readLine.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    if (fields.Length > 0)
                    {
                        if (during)
                        {
                            string account = fields[0].TrimStart('>');
                            list.Add(account);
                        }
                        else if (fields[0] == "ユーザー名")
                        {
                            during = true;
                        }
                    }
                }
            }

            return list;
        }
    }
}
