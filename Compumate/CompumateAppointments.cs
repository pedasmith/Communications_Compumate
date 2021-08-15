using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Composition.Interactions;

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
            Parse<CompumateAppointmentDataEntry>(dataChunk, Header);
        }
        public override string ToString()
        {
            var retval = Entries.Count > 0 ? "APPOINTMENTS\n" : "";
            foreach (var entry in Entries)
            {
                retval += entry.ToString();
            }
            if (Entries.Count > 0) retval += "\n";
            return retval;
        }
    }

    class CompumateAppointmentDataEntry : CompumateAbstractDataEntry
    {
        public string RawDateTime { get { return Data[0]; } }
        public DateTime At { get { return ParseDateTime(RawDateTime); } }
        public string Name { get { return Data[1]; } }
        public string Memo1 { get { return Data[2]; } }
        public string Memo2 { get { return Data[3]; } }
        public string Memo3 { get { return Data[4]; } }

        public static DateTime ParseDateTime(string raw) // e.g. 2104061200000 ==> 2021 04 06 12:00
        {
            var year = Int32.Parse(raw.Substring(0, 2));
            year = year + ((year > 80) ? 1900 : 2000);
            var month = Int32.Parse(raw.Substring(2, 2));
            var day = Int32.Parse(raw.Substring(4, 2));
            var hour = Int32.Parse(raw.Substring(6, 2));
            var minute = Int32.Parse(raw.Substring(8, 2));
            var dt = new DateTime(year, month, day, hour, minute, 0);
            return dt;
        }

        public override void Fixup()
        {
            for (int i = Data.Count; i < 5; i++)
            {
                Add(""); // Add blank lines until there are five values in all cases. This can't actually happen.
            }
        }

        public override string ToString()
        {
            var at = At.ToString();
            return $"{Name} at {At}\n    {Memo1}\n    {Memo2}\n    {Memo3}\n";
        }
    }
}
