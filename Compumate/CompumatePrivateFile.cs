using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;

namespace Compumate
{
    class CompumatePrivateFile
    {
        static byte[] Header = new byte[]
        {
            (byte)'P', (byte)'R', (byte)'I', (byte)'V', (byte)'A', (byte)'T', (byte)'E', (byte)' ', (byte)'F', (byte)'I', (byte)'L', (byte)'E', 0, 0, 0, 0
        };

        public List<CompumateDataEntry> Entries { get; } = new List<CompumateDataEntry>();

        /*
         * 
 PRIVATE FILE x00 x00 x00 x00 x01 x03 x00 x00 x00 x00 x9D 
 Expense x00 
 The categories have the problem of x00 
 requiring the user type in x00 
 the exact category, case  x00 
 sensative. x00 
 Keyboard Impressions x00 
 The pc3 has a bad keyboard. Not only are x00 
 the keys hard to press, but if x00 
 you press too hard,  they x00 
 duplicate the keypress x00 
 Serial port x00 
 Fun fact: if you have no files, the x00 
 system won't respond to the 8/1/1 x00 
 serial send command. ' x00 
 It looks like bad batteries, but it's no x00 


        The 0x03 is the number of entries (3). Each entry is line followed by x00
        After the header the values are:
        +0 x01 unknown
        +1 x03 number of entries
        +2 x00 unknown
        +3 x00 unknown
        +4 x00 unknown
        +5 x00 unknown
        +6 x9D unknown
        +7 <entries>
         * 
         */
        public int Parse(DataChunk dataChunk)
        {
            var data = dataChunk.RawBytes;
            int retval = -1;
            var index = dataChunk.Find(Header);
            if (index < 0) return -1;
            index += Header.Length;
            var nentries = (int)(data[index + 1]);
            index += 7;

            var startIndex = index;
            for (int i=0; i<nentries; i++)
            {
                var entry = new CompumateDataEntry();
                for (int line = 0; line < 5; line++)
                {
                    var endIndex = FindNext(data, startIndex, 0x00);
                    var item = Subbytes(data, startIndex, endIndex); // don't include the null
                    var str = System.Text.Encoding.ASCII.GetString(item);
                    entry.Add(str);
                    startIndex = endIndex + 1;
                }
                Entries.Add(entry);
            }
            return retval;
        }
        public override string ToString()
        {
            var retval = "";
            foreach (var entry in Entries)
            {
                retval += entry.ToString();
            }
            return retval;
        }

        private static int FindNext(byte[] data, int startIndex, byte searchFor)
        {
            for (int i=startIndex; i<data.Length; i++)
            {
                if (data[i] == searchFor) return i;
            }
            return -1;
        }
        private static byte[] Subbytes(byte[] data, int startIndex, int endIndex)
        {
            var len = endIndex - startIndex;
            var retval = new byte[len];
            Array.Copy(data, startIndex, retval, 0, len);
            return retval;
        }
    }
}
