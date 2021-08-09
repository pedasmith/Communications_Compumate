using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;

namespace Compumate
{
    class CompumateSection
    {
        public List<CompumateAbstractDataEntry> Entries { get; } = new List<CompumateAbstractDataEntry>();

        public int Parse<SpecificDataEntry>(DataChunk dataChunk, byte[] header, int nentryLocation=1, int linesPerEntry=5) where SpecificDataEntry: CompumateAbstractDataEntry, new()
        {
            var data = dataChunk.RawBytes;
            int retval = -1;
            var index = dataChunk.Find(header);
            if (index < 0) return -1;
            index += header.Length;
            var nentries = (int)(data[index + nentryLocation]);
            index += 7;

            var startIndex = index;
            for (int i = 0; i < nentries; i++)
            {
                var entry = new SpecificDataEntry();
                for (int line = 0; line < linesPerEntry; line++)
                {
                    var endIndex = FindNext(data, startIndex, 0x00);
                    var item = Subbytes(data, startIndex, endIndex); // don't include the null
                    var str = System.Text.Encoding.ASCII.GetString(item);
                    entry.Add(str);
                    startIndex = endIndex + 1;
                }
                entry.Fixup();
                Entries.Add(entry);
            }
            return retval;
        }
        protected static int FindNext(byte[] data, int startIndex, byte searchFor)
        {
            for (int i = startIndex; i < data.Length; i++)
            {
                if (data[i] == searchFor) return i;
            }
            return -1;
        }
        protected static byte[] Subbytes(byte[] data, int startIndex, int endIndex)
        {
            var len = endIndex - startIndex;
            var retval = new byte[len];
            Array.Copy(data, startIndex, retval, 0, len);
            return retval;
        }
    }
}
