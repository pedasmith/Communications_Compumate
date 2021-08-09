using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;

namespace Compumate
{
    class CompumatePrivateFiles : CompumateSection
    {
        static byte[] Header = new byte[]
        {
            (byte)'P', (byte)'R', (byte)'I', (byte)'V', (byte)'A', (byte)'T', (byte)'E', (byte)' ', (byte)'F', (byte)'I', (byte)'L', (byte)'E', 0, 0, 0, 0
        };


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

        public CompumatePrivateFiles(DataChunk dataChunk)
        {
            Parse(dataChunk, Header);
        }
        public override string ToString()
        {
            var retval = Entries.Count > 0 ? "PRIVATE FILES\n" : "";
            foreach (var entry in Entries)
            {
                retval += entry.ToString();
            }
            return retval;
        }
    }
}
