using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace AuxiliaryLibraries.Collections
{
    public class BitArrayCollection
    {      
        private List<byte> finale = new List<byte>();

        private BitArray currentByte = new BitArray(8);

        private int _index = 0;
        private int index
        {
            get { return _index; }
            set
            {
                _index = value;
                if (value == 8)
                {
                    _index = 0;
                    finale.Add(BitArray2Byte());
                    
                }
            }
        }

        public void Write(bool bit)
        {
            currentByte[_index] = bit;
            index++;
        }

        public byte[] GetArray()
        {
            if (index == 0)
            {
                finale.Add(0);
            }
            else
            {
                finale.Add(BitArray2Byte());
                finale.Add(0);
            }
            return finale.ToArray();
        }

        private byte BitArray2Byte()
        {
            byte result = 0;
            for (byte index = 0, m = 1; index < 8; index++, m *= 2)
                result += currentByte.Get(index) ? m : (byte)0;
            return result;
        }
    }
}