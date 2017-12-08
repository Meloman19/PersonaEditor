using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using PersonaEditorLib.Extension;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Diagnostics;
using PersonaEditorLib.FileStructure.PTP;
using System.Collections.ObjectModel;

namespace PersonaEditorLib
{
    public class CharList : BindingObject
    {
        public class FnMpImg : BindingObject
        {
            private string _Char = "";

            public int Index { get; set; } = 0;

            public string Char
            {
                get { return _Char; }
                set
                {
                    if (_Char != value)
                    {
                        _Char = value;
                        Notify("Char");
                    }
                }
            }

            private BitmapSource _Image;
            public BitmapSource Image
            {
                get { return _Image; }
                set
                {
                    _Image = value;
                    Notify("Image");
                }
            }
        }

        public class FnMpData
        {
            public int Index { get; set; } = 0;
            public string Char { get; set; } = "";
            public byte[] Image_data { get; set; } = new byte[0];
            public VerticalCut Cut { get; set; }
        }

        public CharList()
        {
            _FontList = new ReadOnlyObservableCollection<string>(fontList);
        }

        public CharList(string FontMap, string Font) : this(FontMap, new FileStructure.FNT.FNT(Font)) { }

        public CharList(string FontMap, FileStructure.FNT.FNT FNT) : this()
        {
            OpenFont(FNT);
            OpenFontMap(FontMap);
        }

        public byte[] GetByte(string Char)
        {
            if (Char != "")
            {
                FnMpData fnmp = List.FirstOrDefault(x => x.Char == Char);

                if (fnmp != null)
                {
                    if (fnmp.Index < 0x80)
                    {
                        return new byte[] { (byte)fnmp.Index };
                    }
                    else
                    {
                        byte byte2 = Convert.ToByte((fnmp.Index - 0x20) % 0x80);
                        byte byte1 = Convert.ToByte(((fnmp.Index - 0x20 - byte2) / 0x80) + 0x81);

                        return new byte[] { byte1, byte2 };
                    }
                }
            }
            return new byte[0];
        }

        public byte[] GetByte(char Char)
        {
            return GetByte(Char.ToString());
        }

        private void OpenFont(FileStructure.FNT.FNT FNT)
        {
            try
            {
                ReadFONT(FNT);
                var a = List.Find(x => x.Index == 32);
                if (a != null) a.Cut = new VerticalCut(10, 20);
            }
            catch (Exception e)
            {
                Logging.Write("PersonaEditorLib", e);
            }
        }

        private void OpenFont(string FontName)
        {
            using (FileStream FS = File.OpenRead(FontName))
                OpenFont(new FileStructure.FNT.FNT(FS, 0));
        }

        private void OpenFontMap(string FontMap)
        {
            try
            {
                ReadFNMP(FontMap);
            }
            catch (Exception e)
            {
                Logging.Write("PersonaEditorLib", e);
            }
        }

        public void Open(string FontName, string FontMap)
        {
            List.Clear();
            OpenFont(FontName);
            OpenFontMap(FontMap);
            Update();
        }

        public void Update()
        {
            Notify("Update");
        }

        public bool SaveFontMap(string filename)
        {
            try
            {
                WriteFNMP(filename);
            }
            catch (Exception e)
            {
                Logging.Write("PersonaEditorLib", e);
                return false;
            }
            return true;
        }

        private void ReadFONT(FileStructure.FNT.FNT FNT)
        {
            Height = FNT.Header.Glyphs.Size1;
            Width = FNT.Header.Glyphs.Size2;
            Palette = FNT.Palette.Pallete;
            if (FNT.Header.Glyphs.BitsPerPixel == 4)
                PixelFormat = PixelFormats.Indexed4;
            else if (FNT.Header.Glyphs.BitsPerPixel == 8)
                PixelFormat = PixelFormats.Indexed8;
            else throw new Exception("ReadFONT Error: unknown PixelFormat");

            var DecList = FNT.Compressed.GetDecompressedData();
            if (FNT.Header.Glyphs.BitsPerPixel == 4)
                Util.ReverseByteInList(DecList);

            for (int i = 0; i < DecList.Count; i++)
            {
                FnMpData fnmp = List.FirstOrDefault(x => x.Index == i + 32);
                if (fnmp == null)
                    List.Add(new FnMpData { Index = i + 32, Char = "", Cut = FNT.WidthTable[i] == null ? new VerticalCut(0, (byte)Width) : FNT.WidthTable[i].Value, Image_data = DecList[i] });
                else
                {
                    fnmp.Cut = FNT.WidthTable[i] == null ? new VerticalCut(0, (byte)Width) : FNT.WidthTable[i].Value;
                    fnmp.Image_data = DecList[i];
                }
            }
        }

        private void ReadFNMP(string filename)
        {
            using (StreamReader sr = new StreamReader(new FileStream(filename, FileMode.Open)))
            {
                while (sr.EndOfStream == false)
                {
                    string str = sr.ReadLine();

                    int Index = Convert.ToInt32(str.Substring(0, str.IndexOf('=')));
                    string Char = str.Substring(str.IndexOf('=') + 1);
                    if (Char.Length > 3)
                        Char = Char.Substring(0, 3);

                    FnMpData fnmp = List.FirstOrDefault(x => x.Index == Index);
                    if (fnmp == null)
                        List.Add(new FnMpData { Index = Index, Char = Char });
                    else
                        fnmp.Char = Char;
                }
            }
        }

        private void WriteFNMP(string filename)
        {
            using (StreamWriter sw = new StreamWriter(new FileStream(filename, FileMode.Create)))
            {
                foreach (var CL in List)
                {
                    if (CL.Char != "")
                    {
                        string str = Convert.ToString(CL.Index) + "=" + Convert.ToString(CL.Char);
                        sw.WriteLine(str);
                        sw.Flush();
                    }
                }
            }
        }

        public string GetChar(int index)
        {
            FnMpData fnmp = List.FirstOrDefault(x => x.Index == index);
            if (fnmp == null)
                return "<NCHAR>";
            else
            {
                if (fnmp.Char.Length == 0)
                    return "<CHAR>";
                else if (fnmp.Char.Length == 1)
                    return fnmp.Char;
                else
                    return "<" + fnmp.Char + ">";
            }
        }

        public enum EncodeOptions
        {
            OneChar,
            Bracket
        }

        public byte[] Encode(string String, EncodeOptions options)
        {
            List<byte> returned = new List<byte>();

            if (options == EncodeOptions.OneChar)
            {
                foreach (var C in String)
                    returned.AddRange(GetByte(C));
            }
            else if (options == EncodeOptions.Bracket)
            {
                foreach (var C in Regex.Split(String, @"(<[^>]+>)"))
                    if (Regex.IsMatch(C, @"<.+>"))
                        returned.AddRange(GetByte(C.Substring(1, C.Length - 2)));
                    else
                        returned.AddRange(Encode(C, EncodeOptions.OneChar));
            }

            return returned.ToArray();
        }

        public string Decode(byte[] bytes)
        {
            string returned = "";

            for (int i = 0; i < bytes.Length; i++)
                if (0x20 <= bytes[i] & bytes[i] < 0x80)
                    returned += GetChar(bytes[i]);
                else if (0x80 <= bytes[i] & bytes[i] < 0xF0)
                {
                    int link = (bytes[i] - 0x81) * 0x80 + bytes[i + 1] + 0x20;
                    i++;
                    returned += GetChar(link);
                }

            return returned;
        }

        public List<FnMpData> List { get; set; } = new List<FnMpData>();
        public BitmapPalette Palette { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public PixelFormat PixelFormat { get; set; }

        public string Tag { get; set; } = "";

        public void CopyTo(CharList charList)
        {
            charList.List.Clear();
            foreach (var a in List)
                charList.List.Add(a);

            charList.Palette = Palette;
            charList.Height = Height;
            charList.Width = Width;
            charList.PixelFormat = PixelFormat;
            charList.Tag = Tag;
            charList.Update();
        }

        private ObservableCollection<string> fontList { get; } = new ObservableCollection<string>() { "Empty" };

        private ReadOnlyObservableCollection<string> _FontList;
        public ReadOnlyObservableCollection<string> FontList => _FontList;

        private string FontDirPath = "";

        private int _SelectedIndex = 0;
        public int SelectedIndex
        {
            get { return _SelectedIndex; }
            set
            {
                if (_SelectedIndex != value)
                    SetFont(value);
            }
        }

        private string _SelectedItem = "Empty";
        public string SelectedItem
        {
            get { return _SelectedItem; }
            set
            {
                if (_SelectedItem != value)
                    SetFont(value);
            }
        }

        public void GetFontList(string dir = "")
        {
            FontDirPath = dir;
            fontList.Clear();
            fontList.Add("Empty");
            if (Directory.Exists(dir))
            {
                DirectoryInfo DI = new DirectoryInfo(dir);
                foreach (var file in DI.GetFiles(@"*.fnt"))
                    fontList.Add(file.Name);
            }
        }

        private void SetFont(string name)
        {
            if (FontList.Contains(name))
            {
                string Font = Path.Combine(FontDirPath, name);
                string FontMap = Path.Combine(FontDirPath, Path.GetFileNameWithoutExtension(name) + ".txt");

                Open(Font, FontMap);

                _SelectedIndex = FontList.IndexOf(name);
                _SelectedItem = name;
            }
        }

        private void SetFont(int index)
        {
            if (index >= 0 && index < FontList.Count)
            {
                string Font = Path.Combine(FontDirPath, FontList[index]);
                string FontMap = Path.Combine(FontDirPath, Path.GetFileNameWithoutExtension(FontList[index]) + ".txt");

                Open(Font, FontMap);

                _SelectedIndex = index;
                _SelectedItem = FontList[index];
            }
        }
    }
}