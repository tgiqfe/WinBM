using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Web.Lib
{
    class DownloadFileName
    {
        public static string GetFromURL(string url)
        {
            string bottomPath = url.Substring(url.LastIndexOf("/") + 1);
            string decodePath = HttpUtility.UrlDecode(bottomPath);

            if (decodePath.Contains("?"))
            {
                string preName = decodePath.Substring(0, decodePath.IndexOf("?"));
                string sufName = decodePath.Substring(decodePath.IndexOf("?") + 1);

                if (sufName.Contains("&") || sufName.Contains(";"))
                {
                    string[] paramArray = System.Text.RegularExpressions.Regex.Split(decodePath, "&|;").Select(x => x.Trim()).ToArray();

                    string fileNameParam = paramArray.FirstOrDefault(x => x.StartsWith("filename=", StringComparison.OrdinalIgnoreCase));
                    if (!string.IsNullOrEmpty(fileNameParam))
                    {
                        return fileNameParam.Substring(fileNameParam.IndexOf("=") + 1);
                    }
                }
                return preName;
            }
            return decodePath;
        }
    }
}
