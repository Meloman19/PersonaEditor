using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace AuxiliaryLibraries.Collections
{
    public sealed class BitArrayData
    {
        byte[] bitArray;
        bool[] currentByte = new bool[8];

        int lastBitArrayIndex = 0;
        int lastBitIndex = 0;
        int offset = 0;
        
        public BitArrayData(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException("capacity", "capacity must be more than 0");

            bitArray = new byte[capacity];
        }

        public void Write(bool bit)
        {
            if (bit)
            {
              //  bitArray[lastBitIndex] = bitArray[lastBitIndex] << 1;
            }
        }
    }
}