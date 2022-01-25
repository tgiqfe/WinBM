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

        #region Get children

        /// <summary>
        /// ジャンクション(orシンボリックリンク)を除外して、配下のファイルを全取得
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetAllFiles(string path)
        {
            try
            {
                return Directory.GetFiles(path, "*", SearchOption.AllDirectories);
            }
            catch (UnauthorizedAccessException)
            {
                var list = new List<string>();
                Action<string> recurseDirectory = null;
                recurseDirectory = (targetDir) =>
                {
                    Directory.GetFiles(targetDir).
                        Where(x => (File.GetAttributes(x) & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint).
                        ToList().
                        ForEach(x => list.Add(x));
                    Directory.GetDirectories(targetDir).
                        Where(x => (File.GetAttributes(x) & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint).
                        ToList().
                        ForEach(x => recurseDirectory(x));
                };
                recurseDirectory(path);

                return list;
            }
        }

        /// <summary>
        /// ジャンクション(orシンボリックリンク)を除外して、配下のフォルダーを全取得
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetAllDirectories(string path)
        {
            try
            {
                return Directory.GetDirectories(path, "*", SearchOption.AllDirectories); ;
            }
            catch (UnauthorizedAccessException)
            {
                var list = new List<string>();
                Action<string> recurseDirectory = null;
                recurseDirectory = (targetDir) =>
                {
                    Directory.GetDirectories(targetDir).
                        Where(x => (File.GetAttributes(x) & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint).
                        ToList().
                        ForEach(x =>
                        {
                            list.Add(x);
                            recurseDirectory(x);
                        });
                };
                recurseDirectory(path);

                return list;
            }
        }

        /// <summary>
        /// ファイルとフォルーのリストを両方格納する為のクラス
        /// </summary>
        public class DirectoryChildren
        {
            public List<string> Files { get; set; }
            public List<string> Directories { get; set; }
            public DirectoryChildren()
            {
                this.Files = new List<string>();
                this.Directories = new List<string>();
            }
        }

        /// <summary>
        /// ジャンクション(orシンボリックリンク)を除外して、配下のファイル/フォルダーを全取得
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static DirectoryChildren GetAllChildren(string path)
        {
            var children = new DirectoryChildren();
            try
            {
                children.Files.AddRange(
                    Directory.GetFiles(path, "*", SearchOption.AllDirectories));
                children.Directories.AddRange(
                    Directory.GetDirectories(path, "*", SearchOption.AllDirectories));
            }
            catch (UnauthorizedAccessException)
            {
                Action<string> recurseDirectory = null;
                recurseDirectory = (targetDir) =>
                {
                    Directory.GetFiles(targetDir).
                        Where(x => (File.GetAttributes(x) & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint).
                        ToList().
                        ForEach(x => children.Files.Add(x));
                    Directory.GetDirectories(targetDir).
                        Where(x => (File.GetAttributes(x) & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint).
                        ToList().
                        ForEach(x =>
                        {
                            children.Directories.Add(x);
                            recurseDirectory(x);
                        });
                };
                recurseDirectory(path);
            }
            return children;
        }

        #endregion
    }
}
