using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compumate
{
    class CompumateWordProcessor : CompumateSection
    {
        static byte[] Header = new byte[]
        {
            (byte)'W', (byte)'O', (byte)'R', (byte)'D', (byte)' ', (byte)'P', (byte)'R', (byte)'O', (byte)'C', (byte)'E', (byte)'S', (byte)'S', (byte)'O', (byte)'R', 0, 0x20, 
        };


        /*
         * 
WORD PROCESSOR \00 \20 \00 \00 \00 \00 \02 \03 v 
        WORD PROCESSOR \00 \20
        +0 \00 
        +1 \00 
        +2 \00 
        +3 \00 
        +4 \02 Number of entries 
        +5 \03 Original number of entries after a file is delete
        +6 \76
         * 
         * 
         */

        public CompumateWordProcessor(DataChunk dataChunk)
        {
            Parse<CompumateWordProcessorDataEntry>(dataChunk, Header, 4, 2); // nentries is byte #4, and there are two lines per entry.
        }
        public override string ToString()
        {
            var retval = Entries.Count > 0 ? "WORD PROCESSOR\n" : "";
            foreach (var entry in Entries)
            {
                retval += entry.ToString();
            }
            if (Entries.Count > 0) retval += "\n";
            return retval;
        }
    }

    class CompumateWordProcessorDataEntry : CompumateAbstractDataEntry
    {
        public string Name { get { return Data[0]; } }
        public string Text{ get { return Data[1]; } }

        public override void Fixup()
        {
            // A 0x7F is put at the start of the file name; it must be stripped off.
            if(Data[0][0] == 0x7F)
            {
                Data[0] = Data[0].Substring(1);
            }
        }
        public override string ToString()
        {
            return $"File:{Name}\n{Text}\n--------------------------\n\n";
        }
    }
}
