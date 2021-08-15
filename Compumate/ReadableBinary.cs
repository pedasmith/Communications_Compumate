using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.UI.Xaml;

namespace Compumate
{
    /// <summary>
    /// The ReadableBinary format converts bytes into an ASCII string. Each char is printed either as 
    /// itself or as \XX (literally: backslash and then always two hex chars). Chars 0..31 and 7f..ff
    /// are printed as hex, as is the \ char and #. When a CR or LF is seen, it printed as hex and then
    /// a [CR] added. Lines are restricted to ___ chars long. Note that you can add as many [CR] chars
    /// as you like. 
    /// </summary>
    static class ReadableBinary
    {
        private static bool ShouldPrintHex(byte b)
        {
            bool retval = b <= 0x1f || b >= 0x7f || b == (byte)'\\' || b == (byte)'#';
            return retval;
        }
        public static string ToString(byte[] buffer, int MaxCharInLine=64)
        {
            var sb = new StringBuilder();
            int nchar = 0;
            foreach (var b in buffer)
            {
                if (ShouldPrintHex(b))
                {
                    sb.Append($"\\{b:X2}");
                    if (b == (byte)'\r' || b == (byte)'\n')
                    {
                        sb.Append("\n");
                        nchar = 0;
                    }
                    else
                    {
                        nchar += 3;
                    }
                }
                else
                {
                    sb.Append((char)b);
                    nchar++;
                }
                if (nchar >= MaxCharInLine)
                {
                    nchar = 0;
                    sb.Append("\n");
                }
            }
            return sb.ToString();
        }

        private static int HexValue(char c)
        {
            if (c >= '0' && c <= '9') return c - '0';
            if (c >= 'a' && c <= 'f') return c - 'a' + 10;
            if (c >= 'A' && c <= 'F') return c - 'A' + 10;

            return 0;
        }
        /// <summary>
        /// Given a string with apple\XXpie and passing in an index of 5 (index of \), return the hex value of XX.
        /// </summary>
        private static byte HexValue (string s, int slashIndex)
        {
            var c1 = s[slashIndex + 1];
            var c2 = s[slashIndex + 2];
            var retval = HexValue(c1) * 16 + HexValue(c2);
            return (byte)retval;
        }

        public static byte[] ToBinary(string s)
        {
            var retval = new List<byte>();
            for (int i=0; i<s.Length; i++)
            {
                switch (s[i])
                {
                    case '\\':
                        var b = (byte)HexValue(s, i);
                        retval.Add(b);
                        i += 2; // for the XX. the slash is skipped with the loop i++.
                        break;
                    case var ch when ch >= ' ' && ch <= '~':
                        retval.Add((byte)ch);
                        break;
                    default:
                        // Everything else is ignored
                        break;
                }
            }
            return retval.ToArray();
        }
        public static void Log(string str)
        {
            System.Diagnostics.Debug.WriteLine(str);
        }
        private static int TestOne(string str, string expected = null)
        {
            if (expected == null) expected = str;
            int nerror = 0;
            var bytes = ToBinary(str);
            var actual = ToString(bytes);
            if (actual != expected)
            {
                Log($"ReableBinary: converting \"{str}\" expected=\"{expected}\" actual=\"{actual}\"");
                nerror += 1;
            }
            return nerror;
        }
        private static int TestOneBytes(string str, int maxCharInLine = 10, string expected = null)
        {
            if (expected == null) expected = str;
            int nerror = 0;
            var bytes = Encoding.UTF8.GetBytes(str);
            var actual = ToString(bytes, maxCharInLine);
            if (actual != expected)
            {
                Log($"ReableBinary: converting \"{str}\" expected=\"{expected}\" actual=\"{actual}\"");
                nerror += 1;
            }
            return nerror;

        }

        public static int Test()
        {
            int nerror = 0;
            nerror += TestOne("apple");
            nerror += TestOne("apple\npie", "applepie");
            nerror += TestOne("apple\\01pie");
            nerror += TestOne("apple\\40pie", "apple@pie");
            nerror += TestOneBytes("apple\npie", 10, "apple\\0A\npie");
            nerror += TestOneBytes("string_with_long_lines", 10, "string_wit\nh_long_lin\nes");
            return nerror;
        }

    }
}
