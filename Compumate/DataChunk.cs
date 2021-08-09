using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compumate
{
    class DataChunk
    {
        public byte[] RawBytes { get; internal set; }
        public void AddBytes(byte[] bytesToAdd)
        {
            int rbl = (RawBytes == null ? 0 : RawBytes.Length);
            int len =  rbl + bytesToAdd.Length;
            byte[] newbytes = new byte[len];
            if (RawBytes != null) RawBytes.CopyTo(newbytes, 0);
            bytesToAdd.CopyTo(newbytes, rbl);
            RawBytes = newbytes;
        }

        public void ClearBytes()
        {
            RawBytes = null;
        }

        /// <summary>
        /// Find str followed by nNullBytes in the RawBytes array. Returns -1 if not found, index to start of str otherwise.
        /// </summary>
        public int Find (byte[] searchFor)
        {
            for (int i = 0; i < RawBytes.Length - searchFor.Length; i++)
            {
                if (IsMatch(RawBytes, i, searchFor))
                {
                    return i;
                }
            }
            return -1;
        }

        private bool IsMatch (byte[] data, int dataIndex, byte[] searchFor)
        {
            for (int i=0; i<searchFor.Length; i++)
            {
                if (data[dataIndex + i] != searchFor[i])
                {
                    return false;
                }
            }
            return true;
        }

    }
}
