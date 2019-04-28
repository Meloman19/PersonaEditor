using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PersonaEditorLib
{
    public class PersonaEncoding : Encoding
    {
        public static PersonaEncoding Empty { get; } = new PersonaEncoding();

        public string Tag { get; set; } = "Empty";

        public string FilePath { get; } = "";

        public Dictionary<int, char> Dictionary { get; } = new Dictionary<int, char>();

        public PersonaEncoding()
        {
        }

        public PersonaEncoding(string fontMap)
        {
            if (File.Exists(fontMap))
                OpenFNTMAP(fontMap);

            Tag = Path.GetFileNameWithoutExtension(fontMap);
            FilePath = Path.GetFullPath(fontMap);
        }

        public void Add(int index, char c)
        {
            if (c != '\0')
            {
                if (Dictionary.ContainsKey(index))
                    Dictionary[index] = c;
                else
                    Dictionary.Add(index, c);
            }
        }

        #region FNTMAP

        public void SaveFNTMAP(string path)
        {
            using (FileStream FS = new FileStream(path, FileMode.Create))
                foreach (var a in Dictionary)
                {
                    FS.Position = a.Key * 2;
                    byte[] temp = new byte[2];
                    Unicode.GetBytes(new char[] { a.Value }, 0, 1, temp, 0);
                    FS.Write(temp, 0, 2);
                }
        }

        private void OpenFNTMAP(string path)
        {
            using (FileStream FS = new FileStream(path, FileMode.Open))
            {
                int count = (int)Math.Floor((double)FS.Length / 2);
                byte[] buffer = new byte[2];

                for (int i = 0; i < count; i++)
                {
                    FS.Read(buffer, 0, 2);
                    int Index = (int)(FS.Position - 2) / 2;
                    Add(Index, Unicode.GetChars(buffer)[0]);
                }
            }
        }

        #endregion FNTMAP

        public char GetChar(int index)
        {
            if (Dictionary.ContainsKey(index))
                return Dictionary[index];
            else
                return '\uFFFD';
        }

        public int GetIndex(char c)
        {
            if (Dictionary.ContainsValue(c))
                return Dictionary.First(x => x.Value.Equals(c)).Key;
            else
                return -1;
        }

        #region Encoding

        public override int GetByteCount(char[] chars, int index, int count)
        {
            int bytenum = 0;

            for (int i = index; i < index + count; i++)
            {
                int ind = GetIndex(chars[i]);
                if (ind >= 0 && ind < 0x80)
                    bytenum += 1;
                else if (ind >= 0x80)
                    bytenum += 2;
            }

            return bytenum;
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            int bytenum = 0;

            for (int i = charIndex; i < charIndex + charCount; i++)
            {
                int ind = GetIndex(chars[i]);
                if (ind >= 0 && ind < 0x80)
                {
                    bytes[byteIndex + bytenum] = System.Convert.ToByte(ind);
                    bytenum += 1;
                }
                else if (ind >= 0x80)
                {
                    byte byte2 = System.Convert.ToByte(((ind - 0x20) % 0x80) + 0x80);
                    byte byte1 = System.Convert.ToByte(((ind - 0x20 - byte2) / 0x80) + 0x81);

                    bytes[byteIndex + bytenum] = byte1;
                    bytes[byteIndex + bytenum + 1] = byte2;
                    bytenum += 2;
                }
            }

            return bytenum;
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            int charnum = 0;

            for (int i = index; i < index + count; i++)
            {
                if ((0x80 <= bytes[i] & bytes[i] < 0xF0) && i + 1 < index + count)
                    i++;

                charnum++;
            }

            return charnum;
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            int charnum = 0;

            for (int i = byteIndex; i < byteIndex + byteCount; i++)
            {
                if (0x20 <= bytes[i] & bytes[i] < 0x80)
                    chars[charIndex + charnum] = GetChar(bytes[i]);
                else if (0x80 <= bytes[i] & bytes[i] < 0xF0)
                {
                    if (i + 1 >= byteIndex + byteCount)
                        chars[charIndex + charnum] = '\uFFFD';
                    else
                    {
                        int link = (bytes[i] - 0x81) * 0x80 + bytes[i + 1] + 0x20;
                        chars[charIndex + charnum] = GetChar(link);
                        i++;
                    }
                }
                charnum++;
            }

            return charnum;
        }

        public override int GetMaxByteCount(int charCount)
        {
            return charCount * 2;
        }

        public override int GetMaxCharCount(int byteCount)
        {
            return byteCount;
        }

        #endregion Encoding
    }
}