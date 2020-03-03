using System;
using System.Linq;

namespace AuxiliaryLibraries.Tools
{
    public static class StringTool
    {
        public static byte[] SplitString(string str, char del)
        {
            string[] temp = str.Split(del);
            return Enumerable.Range(0, temp.Length).Select(x => Convert.ToByte(temp[x], 16)).ToArray();
        }

        public static bool TryParseArray(string str, out byte[] array)
        {
            var splitted = str.Split(' ');
            array = new byte[splitted.Length];

            for (int i = 0; i < splitted.Length; i++)
            {
                if (byte.TryParse(splitted[i], System.Globalization.NumberStyles.HexNumber, null, out byte result))
                    array[i] = result;
                else
                    return false;
            }
            return true;
        }
    }
}