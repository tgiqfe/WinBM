using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using System.IO;
using System.Diagnostics;
using System.Management;
using Microsoft.Win32;
using System.Security.Principal;

namespace LocalAccount.Work.Profile
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Backup : TaskJob
    {
        [TaskParameter(Mandatory = true, Resolv = true)]
        [Keys("account", "acount", "username", "user")]
        protected string _UserName { get; set; }

        [TaskParameter(Mandatory = true, Resolv = true)]
        [Keys("output", "outputpath", "outputdirectory", "outputdir",
            "bakcup", "backuppath", "backupdirectory", "backupdir",
            "destination", "destinationpath", "destinationdirectory", "destinationdir",
            "dst", "dstpath", "dstdirectory", "dstdir")]
        protected string _BackupPath { get; set; }

        [TaskParameter]
        [Keys("force", "forse")]
        protected bool _Force { get; set; }

        public override void MainProcess()
        {
            if (!_Force && Directory.Exists(_BackupPath))
            {
                Manager.WriteLog(LogLevel.Info, "Backup directory is already exists.");
                Success = true;
                return;
            }

            //  UserNameがEmptyの場合、Defaultユーザープロファイルとして扱う
            string profilePath = _UserName == "" ?
                Registry.GetValue(
                    @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList",
                    "Default",
                    "") as string :
                getUserProfilePath(_UserName);

            string getUserProfilePath(string userName)
            {
                string sid = new NTAccount(userName).Translate(typeof(SecurityIdentifier)).Value;
                return new ManagementClass("Win32_UserProfile").
                    GetInstances().
                    OfType<ManagementObject>().
                    Where(x => x["SID"] as string == sid).
                    Select(x => x["LocalPath"] as string).
                    First();
            }

            if (!Directory.Exists(profilePath))
            {
                Manager.WriteLog(LogLevel.Warn, "Target directory is missing. \"{0}\"", profilePath);
                return;
            }

            using (Process proc = new Process())
            {
                proc.StartInfo.FileName = "robocopy.exe";
                proc.StartInfo.Arguments =
                    $"\"{profilePath}\" \"{_BackupPath}\" /COPY:DAT /MIR /E /XJD /R:0 /W:0 /XJF /NP";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.CreateNoWindow = true;
                proc.Start();
                proc.WaitForExit();
            }

            if (Directory.Exists(_BackupPath))
            {
                File.SetAttributes(_BackupPath, File.GetAttributes(_BackupPath) | FileAttributes.Hidden);
                Success = true;
            }
        }
    }
}
