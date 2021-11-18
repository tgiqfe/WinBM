using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IO.Lib
{
    class FileEncoding
    {
        /// <summary>
        /// 文字コードを取得。
        /// UTF-8以外の文字コードを指定した場合は、RegisterProvider実行後して再取得。
        /// </summary>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static Encoding Get(string encoding)
        {
            if (!string.IsNullOrEmpty(encoding))
            {
                try
                {
                    return Encoding.GetEncoding(encoding);
                }
                catch
                {
                    try
                    {
                        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                        return Encoding.GetEncoding(encoding);
                    }
                    catch { }
                }
            }
            return Encoding.UTF8;
        }
    }
}
