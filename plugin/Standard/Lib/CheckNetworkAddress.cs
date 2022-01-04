using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;

namespace Standard.Lib
{
    internal class CheckNetworkAddress
    {
        private List<byte[]> _ipAddressBytesList = null;
        private List<int[]> _ipAddressIntList = null;
        private List<string> _ipAddressStringList = null;
        public List<byte[]> IPAddressBytesList { get { return this._ipAddressBytesList; } }
        public List<int[]> IPAddressIntList { get { return this._ipAddressIntList; } }
        public List<string> IPAddressStringList { get { return this._ipAddressStringList; } }

        private Regex _reg_nwaddr = new Regex(
            @"^((\d\d?|1\d\d|2[0-4]\d|25[0-5])\.){3}(\d\d?|1\d\d|2[0-4]\d|25[0-5])/(\d+|((\d\d?|1\d\d|2[0-4]\d|25[0-5])\.){3}(\d\d?|1\d\d|2[0-4]\d|25[0-5]))$");

        /// <summary>
        /// コンストラクタ呼び出し時に現在のIPアドレスを取得
        /// </summary>
        public CheckNetworkAddress()
        {
            _ipAddressBytesList = new List<byte[]>();
            _ipAddressStringList = new List<string>();
            _ipAddressIntList = new List<int[]>();

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                    nic.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
                {
                    continue;
                }
                IPAddress[] ipAddresses = nic.GetIPProperties().UnicastAddresses.
                    Where(x => x.Address.AddressFamily == AddressFamily.InterNetwork).
                    Select(x => x.Address).ToArray();
                foreach (IPAddress ipAddress in ipAddresses)
                {
                    byte[] bytes = ipAddress.GetAddressBytes();
                    _ipAddressBytesList.Add(bytes);
                    _ipAddressIntList.Add(new int[4] { bytes[0], bytes[1], bytes[2], bytes[3] });
                    _ipAddressStringList.Add(ipAddress.ToString());
                }
            }
        }

        /// <summary>
        /// ネットワークアドレスとの一致チェック
        /// </summary>
        /// <param name="networkAddress"></param>
        /// <returns></returns>
        public bool IsMatch(string networkAddress)
        {
            if (networkAddress == null) { return false; }
            return IsMatch(new string[1] { networkAddress });
        }

        /// <summary>
        /// ネットワークアドレスと一致チェック
        /// </summary>
        /// <param name="networkAddresses"></param>
        /// <returns></returns>
        public bool IsMatch(string[] networkAddresses)
        {
            if (networkAddresses == null) { return false; }

            bool ret = false;

            foreach (string networkAddress in networkAddresses)
            {
                if (string.IsNullOrEmpty(networkAddress)) { continue; }
                foreach (string address in networkAddress.Split(',').Select(x => x.Trim()).ToArray())
                {
                    if (_reg_nwaddr.IsMatch(address))
                    {
                        ret |= CidrMatch(address);
                    }
                    else if (address.Contains("*"))
                    {
                        ret |= WildcardMatch(address);
                    }
                    else if (address.Contains("~") || address.Contains("-"))
                    {
                        ret |= RangeMatch(address);
                    }
                    else
                    {
                        ret |= FullMatch(address);
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// CIDR表記のチェック
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        private bool CidrMatch(string address)
        {
            //  以下のパターンでのチェック
            //  192.168.1.0/24
            //  192.168.1.0/255.255.255.0
            string networkAddr = address.Substring(0, address.IndexOf("/"));
            string subnetMask = address.Substring(address.IndexOf("/") + 1);

            byte[] networkAddressBytes = IPAddress.Parse(networkAddr).GetAddressBytes();
            byte[] subnetMaskBytes = int.TryParse(subnetMask, out int tempInt) ?
                BitConverter.GetBytes(~(uint.MaxValue >> tempInt)).Reverse().ToArray() :
                IPAddress.Parse(subnetMask).GetAddressBytes();

            foreach (byte[] ipAddressBytes in _ipAddressBytesList)
            {
                if ((networkAddressBytes[0] & subnetMaskBytes[0]) == (ipAddressBytes[0] & subnetMaskBytes[0]) &&
                    (networkAddressBytes[1] & subnetMaskBytes[1]) == (ipAddressBytes[1] & subnetMaskBytes[1]) &&
                    (networkAddressBytes[2] & subnetMaskBytes[2]) == (ipAddressBytes[2] & subnetMaskBytes[2]) &&
                    (networkAddressBytes[3] & subnetMaskBytes[3]) == (ipAddressBytes[3] & subnetMaskBytes[3]))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// ワイルドカードを含むアドレスでチェック
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        private bool WildcardMatch(string address)
        {
            string patternString = Regex.Replace(address, ".",
                x =>
                {
                    string y = x.Value;
                    if (y.Equals("?")) { return "\\."; }
                    else if (y.Equals("*")) { return ".*"; }
                    else { return Regex.Escape(y); }
                });
            if (!patternString.StartsWith("*")) { patternString = "^" + patternString; }
            if (!patternString.EndsWith("*")) { patternString = patternString + "$"; }
            Regex tempReg = new Regex(patternString, RegexOptions.IgnoreCase);

            return _ipAddressStringList.Any(x => tempReg.IsMatch(x));
        }

        /// <summary>
        /// 範囲指定チェック
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        private bool RangeMatch(string address)
        {
            string[] addressNums = address.Split('.');
            if (addressNums.Length == 4)
            {
                Func<string, int[]> splitMinMax = (field) =>
                {
                    if (field.Contains("~") || field.Contains("-"))
                    {
                        int position = field.IndexOf("~");
                        if (position < 0)
                        {
                            position = field.IndexOf("-");
                        }
                        string startText = field.Substring(0, position);
                        string endText = field.Substring(position + 1);
                        if (int.TryParse(startText, out int startNum) && int.TryParse(endText, out int endNum))
                        {
                            return new int[2] { startNum, endNum };
                        }
                    }
                    else
                    {
                        if (int.TryParse(field, out int num))
                        {
                            return new int[2] { num, num };
                        }
                    }
                    return new int[2] { -1, -1 };
                };
                int[][] addressRanges = new int[4][]
                {
                    splitMinMax(addressNums[0]),
                    splitMinMax(addressNums[1]),
                    splitMinMax(addressNums[2]),
                    splitMinMax(addressNums[3])
                };

                foreach (int[] ipAddressInts in _ipAddressIntList)
                {
                    if ((ipAddressInts[0] >= addressRanges[0][0] && ipAddressInts[0] <= addressRanges[0][1]) &&
                        (ipAddressInts[1] >= addressRanges[1][0] && ipAddressInts[1] <= addressRanges[1][1]) &&
                        (ipAddressInts[2] >= addressRanges[2][0] && ipAddressInts[2] <= addressRanges[2][1]) &&
                        (ipAddressInts[3] >= addressRanges[3][0] && ipAddressInts[3] <= addressRanges[3][1]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 完全一致チェック
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        private bool FullMatch(string address)
        {
            return _ipAddressStringList.Any(x => x == address);
        }
    }
}
