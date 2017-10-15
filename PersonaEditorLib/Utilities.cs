using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonaEditorLib
{
    public static class Utilities
    {
        public static int Alignment(int Size, int Align)
        {
            return Alignment((long)Size, Align);
        }

        public static int Alignment(long Size, int Align)
        {
            int temp = (int)Size % Align;
            temp = Align - temp;
            return temp % Align;
        }

        public static byte[] DataReverse(byte[] Data)
        {
            byte[] returned = new byte[Data.Length];
            for (int i = 0; i < Data.Length; i++)
                returned[i] = ReverseByte(Data[i]);
            return returned;
        }

        public static byte ReverseByte(byte toReverse)
        {
            int temp = (toReverse >> 4) + ((toReverse - (toReverse >> 4 << 4)) << 4);
            return Convert.ToByte(temp);
        }
    }
}
