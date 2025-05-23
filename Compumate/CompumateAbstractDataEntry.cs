﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compumate
{
    /// <summary>
    /// A Compumate Data Entry is always exactly 5 lines of text
    /// </summary>
    abstract class CompumateAbstractDataEntry
    {
        public List<string> Data { get; } = new List<string>();
        public void Add(string line)
        {
            Data.Add(line);
        }
        public virtual void Fixup()
        {

        }
        public override string ToString()
        {
            var str = Data.Count >= 0 ? Data[0]+"\n" : "(NO DATA)\n";
            for (int i=1; i<Data.Count; i++)
            {
                var item = Data[i];
                str += "    " + item + "\n";
            }
            return str;
        }
    }

    class CompumateDataEntry : CompumateAbstractDataEntry
    {

    }
}
