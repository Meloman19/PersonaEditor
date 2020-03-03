using System;
using System.Collections.Generic;
using System.Text;

namespace AuxiliaryLibraries
{
    public class ArraySection<T>
    {
        T[] source = null;
        int offset = 0;
        int length = 0;

        public ArraySection(T[] source, int offset, int length)
        {
            this.source = source;
            this.offset = offset;
            this.length = length;
        }

        public T this[int index]
        {
            get
            {
                return source[offset + index];
            }
            set
            {
                source[offset + index] = value;
            }
        }
    }
}