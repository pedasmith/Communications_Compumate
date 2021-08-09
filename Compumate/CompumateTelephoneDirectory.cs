using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compumate
{
    class CompumateTelephoneDirectory : CompumateSection
    {
        static byte[] Header = new byte[]
        {
            (byte)'T', (byte)'E', (byte)'L', (byte)'E', (byte)'P', (byte)'H', (byte)'O', (byte)'N', (byte)'E', (byte)' ', (byte)'D', (byte)'I', (byte)'R', (byte)'.', 0, 0
        };


        /*
         * 
 TELEPHONE DIR. x00  x00  x00  x03  x00  x00  x00  x00  x9F 
 Ahab, Skipper x00 
 609 555-0199 x00 
 Captain's Cabin x00 
 Pequod, Nantucket Island,  MA x00 
 Really wants that whale x00 
 Doe,  John x00 609 555-0123 x00 
 100 Main Street x00 
 Anytown, NJ,  08540 x00 
 Fictious legal person x00 
 Holmes, Sherlock x00 
 609 555-0101 x00 221b Baker Street' x00 
 London UK x00 
 Fictional Detective x00 
 x13  x00 



        The 0x03 is the number of entries (3). Each entry is line followed by x00
        After the header the values are:
        +0 x00 unknown
        +1 x03 number of entries
        +2 x00 unknown
        +3 x00 unknown
        +4 x00 unknown
        +5 x00 unknown
        +6 x9F unknown
        +7 <entries>
         * 
         */
        public CompumateTelephoneDirectory(DataChunk dataChunk)
        {
            Parse(dataChunk, Header);
        }
        public override string ToString()
        {
            var retval = Entries.Count > 0 ? "TELEPHONE DIR.\n" : "";
            foreach (var entry in Entries)
            {
                retval += entry.ToString();
            }
            return retval;
        }
    }
}

