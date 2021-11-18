using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Principal;
using System.Security.AccessControl;

namespace IO.Lib
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class DirectoryControl
    {
        /// <summary>
        /// 文字列からFileSystemAccessのListを取得
        /// </summary>
        /// <param name="accessString"></param>
        /// <returns></returns>
        public static List<FileSystemAccessRule> StringToAccessRules(string accessString)
        {
            List<FileSystemAccessRule> ruleList = new List<FileSystemAccessRule>();
            foreach (string ruleStr in accessString.Split('/'))
            {
                string[] fields = ruleStr.Split(';');
                if (fields.Length >= 5)
                {
                    ruleList.Add(new FileSystemAccessRule(
                        new NTAccount(fields[0]),
                        Enum.TryParse(fields[1], out FileSystemRights tempRights) ? tempRights : FileSystemRights.ReadAndExecute,
                        Enum.TryParse(fields[2], out InheritanceFlags tempInhrFlags) ? tempInhrFlags : InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                        Enum.TryParse(fields[3], out PropagationFlags tempPrpgFlags) ? tempPrpgFlags : PropagationFlags.None,
                        Enum.TryParse(fields[4], out AccessControlType tempType) ? tempType : AccessControlType.Allow));
                }
            }
            return ruleList;
        }

        /// <summary>
        /// AccessRuleのリストから文字列を取得
        /// </summary>
        /// <param name="rules"></param>
        /// <returns></returns>
        public static string AccessRulesToString(AuthorizationRuleCollection rules)
        {
            List<string> accessRuleList = new List<string>();
            foreach (FileSystemAccessRule rule in rules)
            {
                string tempRights = rule.FileSystemRights == FileSystemRights.FullControl ?
                    "FullControl" :
                    (rule.FileSystemRights & (~FileSystemRights.Synchronize)).ToString();
                accessRuleList.Add(string.Format(
                    "{0};{1};{2};{3};{4}",
                    rule.IdentityReference.Value,
                    tempRights,
                    rule.InheritanceFlags,
                    rule.PropagationFlags,
                    rule.AccessControlType));
            }
            return string.Join("/", accessRuleList);
        }
    }
}
