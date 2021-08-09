using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compumate
{
    class CompumateAppointments : CompumateSection
    {
        static byte[] Header = new byte[]
        {
            (byte)'A', (byte)'P', (byte)'P', (byte)'O', (byte)'I', (byte)'N', (byte)'T', (byte)'M', (byte)'E', (byte)'N', (byte)'T', (byte)'S', 0, 0, 0, 0
        };


        /*
         * 
 APPOINTMENTS x00 x00 x00 x00 x02 x02 x00 x00 x00 x00 |
 2108070900000 x00 
 Ahab x00 
 He is behind in  capturing enough oil x00 
 mMore whales=more profit x00 
 Pequod needs new sails x00 
 2108071600000 x00 
 Sherlock Holmes x00 
 Is he on  track  to find the beryl?' x00 
 And is is a proper blue?  x00 
 Is the maid involved x00 



        The 0x03 is the number of entries (3). Each entry is line followed by x00
        After the header the values are:
        +0 x02 unknown
        +1 x02 number of entries
        +2 x00 unknown
        +3 x00 unknown
        +4 x00 unknown
        +5 x00 unknown
        +6 | unknown
        +7 <entries>

        The first line of the entry is the time: 210807 is 2021-08-07 and 0900000 is 09:00
        The second line is the person for the appointment
         * 
         */
        public CompumateAppointments(DataChunk dataChunk)
        {
            Parse(dataChunk, Header);
        }
        public override string ToString()
        {
            var retval = Entries.Count > 0 ? "APPOINTMENTS\n" : "";
            foreach (var entry in Entries)
            {
                retval += entry.ToString();
            }
            return retval;
        }
    }
}
