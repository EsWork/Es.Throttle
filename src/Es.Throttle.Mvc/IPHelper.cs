using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace Es.Throttle.Mvc
{
    /// <summary>
    /// IP相关辅助
    /// </summary>
    internal static class IPHelper
    {
        /// <summary>
        /// IP v4 and v6 range
        /// </summary>
        /// <example>
        /// "192.168.0.0/24"
        /// "fe80::/10"
        /// "192.168.0.0/255.255.255.0"
        /// "192.168.0.0-192.168.0.255"
        /// </example>
        public static void GetRange(string ipRangeString, out IPAddress begin, out IPAddress end)
        {
            // remove all spaces.
            ipRangeString = ipRangeString.Replace(" ", "");

            // Pattern 1. CIDR range: "192.168.0.0/24", "fe80::/10"
            var m1 = Regex.Match(ipRangeString, @"^(?<adr>[\da-f\.:]+)/(?<maskLen>\d+)$", RegexOptions.IgnoreCase);
            if (m1.Success)
            {
                var baseAdrBytes = IPAddress.Parse(m1.Groups["adr"].Value).GetAddressBytes();
                var maskBytes = Bits.GetBitMask(baseAdrBytes.Length, int.Parse(m1.Groups["maskLen"].Value));
                baseAdrBytes = Bits.And(baseAdrBytes, maskBytes);
                begin = new IPAddress(baseAdrBytes);
                end = new IPAddress(Bits.Or(baseAdrBytes, Bits.Not(maskBytes)));
                return;
            }

            // Pattern 2. Uni address: "127.0.0.1", ":;1"
            var m2 = Regex.Match(ipRangeString, @"^(?<adr>[\da-f\.:]+)$", RegexOptions.IgnoreCase);
            if (m2.Success)
            {
                end = begin = IPAddress.Parse(ipRangeString);
                return;
            }

            // Pattern 3. Begin end range: "169.258.0.0-169.258.0.255"
            var m3 = Regex.Match(ipRangeString, @"^(?<begin>[\da-f\.:]+)-(?<end>[\da-f\.:]+)$", RegexOptions.IgnoreCase);
            if (m3.Success)
            {
                begin = IPAddress.Parse(m3.Groups["begin"].Value);
                end = IPAddress.Parse(m3.Groups["end"].Value);
                return;
            }

            // Pattern 4. Bit mask range: "192.168.0.0/255.255.255.0"
            var m4 = Regex.Match(ipRangeString, @"^(?<adr>[\da-f\.:]+)/(?<bitmask>[\da-f\.:]+)$", RegexOptions.IgnoreCase);
            if (m4.Success)
            {
                var baseAdrBytes = IPAddress.Parse(m4.Groups["adr"].Value).GetAddressBytes();
                var maskBytes = IPAddress.Parse(m4.Groups["bitmask"].Value).GetAddressBytes();
                baseAdrBytes = Bits.And(baseAdrBytes, maskBytes);
                begin = new IPAddress(baseAdrBytes);
                end = new IPAddress(Bits.Or(baseAdrBytes, Bits.Not(maskBytes)));
                return;
            }

            throw new FormatException("Unknown IP range string.");
        }

        /// <summary>
        /// 是否私有IP
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public static bool IsPrivateIpAddress(this IPAddress ipAddress)
        {
            // http://en.wikipedia.org/wiki/Private_network
            // Private IP Addresses are:
            //  24-bit block: 10.0.0.0 through 10.255.255.255
            //  20-bit block: 172.16.0.0 through 172.31.255.255
            //  16-bit block: 192.168.0.0 through 192.168.255.255
            //  Link-local addresses: 169.254.0.0 through 169.254.255.255 (http://en.wikipedia.org/wiki/Link-local_address)
            var octets = ipAddress.GetAddressBytes();

            bool isIpv6 = octets.Length == 16;

            if (isIpv6)
            {
                bool isUniqueLocalAddress = octets[0] == 253;
                return isUniqueLocalAddress;
            }
            else
            {
                var is24BitBlock = octets[0] == 10;
                if (is24BitBlock) return true;

                var is20BitBlock = octets[0] == 172 && octets[1] >= 16 && octets[1] <= 31;
                if (is20BitBlock) return true;

                var is16BitBlock = octets[0] == 192 && octets[1] == 168;
                if (is16BitBlock) return true;

                var isLinkLocalAddress = octets[0] == 169 && octets[1] == 254;
                return isLinkLocalAddress;
            }
        }

        /// <summary>
        /// 转换为无符号整型字节
        /// </summary>
        /// <param name="address">无符号整型</param>
        /// <returns>返回IPAddress</returns>
        public static IPAddress ConvertIPAddress(uint address)
        {
            return new IPAddress(IPv4ToBytes(address));
        }

        /// <summary>
        /// 转换为无符号整数
        /// </summary>
        /// <param name="address">IPAddress</param>
        /// <returns>返回无符号整数</returns>
        public static uint IPv4ToInteger(this IPAddress address)
        {
            return IPv4ToInteger(address.GetAddressBytes());
        }

        /// <summary>
        /// 验证IPv4是否在范围中
        /// </summary>
        /// <param name="ip">验证的IP</param>
        /// <param name="begin">起始范围</param>
        /// <param name="end">结束范围</param>
        /// <returns>是否通过</returns>
        public static bool IPv4InRange(IPAddress ip, IPAddress begin, IPAddress end)
        {
            return IPv4InRange(ip.IPv4ToInteger(), begin.IPv4ToInteger(), end.IPv4ToInteger());
        }

        /// <summary>
        /// 是否Ipv4
        /// </summary>
        /// <param name="address">ip地址</param>
        /// <returns>bool</returns>
        public static bool IsIPv4(this IPAddress address)
        {
            return address.GetAddressBytes().Length == 4;
        }

        /// <summary>
        /// 是否Ipv6
        /// </summary>
        /// <param name="address">ip地址</param>
        /// <returns>bool</returns>
        public static bool IsIPv6(this IPAddress address)
        {
            return address.GetAddressBytes().Length == 16;
        }

        /// <summary>
        /// 验证IPv4是否在范围中
        /// </summary>
        /// <param name="ip">验证IP</param>
        /// <param name="range">
        /// <para>IP列表自动三种方式匹配</para>
        /// <para>1.127.0.0.5 -> 127.0.0.5 直接匹配</para>
        /// <para>2.127.0.0.5 -> 127.0.*.* 通配符匹配</para>
        /// <para>3.127.0.0.5 -> 127.0.0.1-127.0.0.255 范围匹配</para>
        /// </param>
        /// <returns></returns>
        public static bool IPv4InRange(string ip, string[] range)
        {
            var segment = ip.Split('.');

            foreach (var test in range)
            {
                if (test.Contains("*"))
                {
                    var s = test.Split('.');
                    bool flag = true;
                    for (int i = 0; i < s.Length; i++)
                    {
                        if (s[i] != "*")
                        {
                            if (segment[i] != s[i])
                                flag = false;
                            break;
                        }
                    }
                    if (flag)
                        return true;
                }
                else if (test.Contains("-"))
                {
                    var s = test.Split('-');

                    if (IPv4InRange(ip, s[0], s[1]))
                        return true;
                }
                else
                {
                    if (ip == test)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 验证IPv4是否在范围中
        /// </summary>
        /// <param name="ip">验证的IP</param>
        /// <param name="begin">起始范围</param>
        /// <param name="end">结束范围</param>
        /// <returns>是否通过</returns>
        public static bool IPv4InRange(string ip, string begin, string end)
        {
            var _ip = IPv4ToBytes(ip);
            var _begin = IPv4ToBytes(begin);
            var _end = IPv4ToBytes(end);
            return IPv4InRange(_ip, _begin, _end);
        }

        /// <summary>
        /// 验证IPv4是否在范围中
        /// </summary>
        /// <param name="ip">验证的IP</param>
        /// <param name="begin">起始范围</param>
        /// <param name="end">结束范围</param>
        /// <returns>是否通过</returns>
        public static bool IPv4InRange(uint ip, uint begin, uint end)
        {
            if (begin > end)
            {
                begin ^= end;
                end ^= begin;
                begin ^= end;
            }
            return ip >= begin && ip <= end;
        }

        /// <summary>
        /// 验证IPv4是否在范围中
        /// </summary>
        /// <param name="ip">验证的IP</param>
        /// <param name="begin">起始范围</param>
        /// <param name="end">结束范围</param>
        /// <returns>是否通过</returns>
        public static bool IPv4InRange(byte[] ip, byte[] begin, byte[] end)
        {
            return IPv4InRange(IPv4ToInteger(ip), IPv4ToInteger(begin), IPv4ToInteger(end));
        }

        /// <summary>
        /// 转换为无符号整型字节
        /// </summary>
        /// <param name="address">IP地址（127.0.0.1）</param>
        /// <returns></returns>
        public static byte[] IPv4ToBytes(string address)
        {
            return address.Split('.').Select(s => byte.Parse(s)).ToArray();
        }

        /// <summary>
        /// 转换为无符号整型字节
        /// </summary>
        /// <param name="address">无符号整型</param>
        /// <returns>返回无符号整型字节</returns>
        public static byte[] IPv4ToBytes(uint address)
        {
            var bytes = new byte[4];
            bytes[0] = (byte)(address >> 24);
            bytes[1] = (byte)(address >> 16);
            bytes[2] = (byte)(address >> 8);
            bytes[3] = (byte)address;
            return bytes;
        }

        /// <summary>
        /// 转换为整数
        /// </summary>
        /// <param name="address">IP地址（127.0.0.1）</param>
        /// <returns></returns>
        public static uint IPv4ToInteger(string address)
        {
            return IPv4ToInteger(IPv4ToBytes(address));
        }

        /// <summary>
        /// 转换为无符号整数
        /// </summary>
        /// <param name="address">4个字节</param>
        /// <returns>返回无符号整数</returns>
        public static uint IPv4ToInteger(byte[] address)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            if (address.Length != 4)
            {
                throw new ArgumentException("IPv4 字节长度必须是4个字节", "address");
            }
            //IP是网络字节，所以用大端转换
            return (uint)((address[0] << 24 | address[1] << 16 | address[2] << 8 | address[3]) & 0x0FFFFFFFF);
        }

        /// <summary>
        /// 验证是否IPv4或Ipv6
        /// </summary>
        /// <param name="address"></param>
        /// <returns>bool</returns>
        public static bool IsValid(
            string address)
        {
            return IsValidIPv4(address) || IsValidIPv6(address);
        }

        /// <summary>
        /// 验证是否IPv4或Ipv6 和 netmask
        /// </summary>
        /// <param name="address"></param>
        /// <returns>bool</returns>
        public static bool IsValidWithNetMask(
            string address)
        {
            return IsValidIPv4WithNetmask(address) || IsValidIPv6WithNetmask(address);
        }

        /// <summary>
        /// 是否Ipv4
        /// </summary>
        /// <param name="address">ip地址</param>
        /// <returns>bool</returns>
        public static bool IsValidIPv4(
            string address)
        {
            try
            {
                return UnsafeIsValidIPv4(address);
            }
            catch (FormatException) { }
            catch (OverflowException) { }
            return false;
        }

        private static bool UnsafeIsValidIPv4(
            string address)
        {
            if (address.Length == 0)
                return false;

            int octets = 0;
            string temp = address + ".";

            if (address.IndexOf('.') == -1)
                return false;

            int pos;
            int start = 0;
            while (start < temp.Length
                && (pos = temp.IndexOf('.', start)) > start)
            {
                if (octets == 4)
                    return false;

                string octetStr = temp.Substring(start, pos - start);

                if (!int.TryParse(octetStr, out int octet))
                    return false;

                if (octet < 0 || octet > 255)
                    return false;

                start = pos + 1;
                octets++;
            }

            return octets == 4;
        }

        /// <summary>
        /// 是否IPv4
        /// </summary>
        /// <param name="address">ip地址</param>
        /// <returns>bool</returns>
        public static bool IsValidIPv4WithNetmask(
            string address)
        {
            int index = address.IndexOf("/");
            string mask = address.Substring(index + 1);

            return (index > 0) && IsValidIPv4(address.Substring(0, index))
                && (IsValidIPv4(mask) || IsMaskValue(mask, 32));
        }

        /// <summary>
        /// 是否IPv6
        /// </summary>
        /// <param name="address">ip地址</param>
        /// <returns>bool</returns>
        public static bool IsValidIPv6WithNetmask(
            string address)
        {
            int index = address.IndexOf("/");
            string mask = address.Substring(index + 1);

            return (index > 0) && (IsValidIPv6(address.Substring(0, index))
                && (IsValidIPv6(mask) || IsMaskValue(mask, 128)));
        }

        private static bool IsMaskValue(
           string component,
           int size)
        {
            if (!int.TryParse(component, out int val))
                return false;
            return val >= 0 && val <= size;
        }

        /// <summary>
        /// 是否IPv6
        /// </summary>
        /// <param name="address">ip地址</param>
        /// <returns></returns>
        public static bool IsValidIPv6(
            string address)
        {
            try
            {
                return UnsafeIsValidIPv6(address);
            }
            catch (FormatException) { }
            catch (OverflowException) { }
            return false;
        }

        private static bool UnsafeIsValidIPv6(
            string address)
        {
            if (address.Length == 0)
            {
                return false;
            }

            int octets = 0;

            string temp = address + ":";
            bool doubleColonFound = false;
            int pos;
            int start = 0;
            while (start < temp.Length
                && (pos = temp.IndexOf(':', start)) >= start)
            {
                if (octets == 8)
                {
                    return false;
                }

                if (start != pos)
                {
                    string value = temp.Substring(start, pos - start);

                    if (pos == (temp.Length - 1) && value.IndexOf('.') > 0)
                    {
                        if (!IsValidIPv4(value))
                        {
                            return false;
                        }

                        octets++; // add an extra one as address covers 2 words.
                    }
                    else
                    {
                        string octetStr = temp.Substring(start, pos - start);
                        if (!int.TryParse(octetStr, NumberStyles.AllowHexSpecifier, NumberFormatInfo.CurrentInfo, out int octet))
                            return false;
                        if (octet < 0 || octet > 0xffff)
                            return false;
                    }
                }
                else
                {
                    if (pos != 1 && pos != temp.Length - 1 && doubleColonFound)
                    {
                        return false;
                    }
                    doubleColonFound = true;
                }
                start = pos + 1;
                octets++;
            }

            return octets == 8 || doubleColonFound;
        }
    }

    internal static class Bits
    {
        internal static byte[] Not(byte[] bytes)
        {
            return bytes.Select(b => (byte)~b).ToArray();
        }

        internal static byte[] And(byte[] A, byte[] B)
        {
            return A.Zip(B, (a, b) => (byte)(a & b)).ToArray();
        }

        internal static byte[] Or(byte[] A, byte[] B)
        {
            return A.Zip(B, (a, b) => (byte)(a | b)).ToArray();
        }

        internal static bool GE(byte[] A, byte[] B)
        {
            return A.Zip(B, (a, b) => a == b ? 0 : a < b ? 1 : -1)
                .SkipWhile(c => c == 0)
                .FirstOrDefault() >= 0;
        }

        internal static bool LE(byte[] A, byte[] B)
        {
            return A.Zip(B, (a, b) => a == b ? 0 : a < b ? 1 : -1)
                .SkipWhile(c => c == 0)
                .FirstOrDefault() <= 0;
        }

        internal static byte[] GetBitMask(int sizeOfBuff, int bitLen)
        {
            var maskBytes = new byte[sizeOfBuff];
            var bytesLen = bitLen / 8;
            var bitsLen = bitLen % 8;
            for (int i = 0; i < bytesLen; i++)
            {
                maskBytes[i] = 0xff;
            }
            if (bitsLen > 0) maskBytes[bytesLen] = (byte)~Enumerable.Range(1, 8 - bitsLen).Select(n => 1 << n - 1).Aggregate((a, b) => a | b);
            return maskBytes;
        }
    }
}