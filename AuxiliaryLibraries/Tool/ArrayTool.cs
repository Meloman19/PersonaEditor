using AuxiliaryLibraries.Media;
using System.Collections.Generic;
using System.Linq;

namespace AuxiliaryLibraries.Tools
{
    public static class ArrayTool
    {
        public static void ReverseByteInList(IList<byte[]> list)
        {
            foreach (var a in list)
            {
                for (int i = 0; i < a.Length; i++)
                {
                    a[i] = PixelFormatHelper.ReverseByte(a[i]);
                }
            }
        }

        public static bool CheckEntrance(this byte[] B, byte[] Bytes, int StartIndex)
        {
            if (Bytes.Length != 0)
            {
                if (StartIndex < B.Length)
                {
                    if (B[StartIndex] == Bytes[0])
                        return B.CheckEntrance(Bytes.Skip(1).ToArray(), StartIndex + 1);
                    else return false;
                }
                else return false;
            }
            else return true;
        }
    }
}