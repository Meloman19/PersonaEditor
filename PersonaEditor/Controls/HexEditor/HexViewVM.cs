using AuxiliaryLibraries.Extensions;
using AuxiliaryLibraries.WPF;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace PersonaEditor.Controls.HexEditor
{
    class HexViewByteVM : BindingObject
    {
        public string Byte { get; set; } = "..";

        public void SetByte(byte Byte)
        {
            this.Byte = String.Format("{0:X2}", Byte);
            Notify("Byte");
        }

        public HexViewByteVM()
        {

        }

        internal void Reset()
        {
            Byte = "..";
            Notify("Byte");
        }
    }

    class HexViewUIntVM : BindingObject
    {
        public ObservableCollection<HexViewByteVM> Bytes { get; } = new ObservableCollection<HexViewByteVM>();

        public void SetBytes(byte[] bytes)
        {
            if (bytes.Length <= 4)
                for (int i = 0; i < bytes.Length; i++)
                    Bytes[i].SetByte(bytes[i]);
        }

        public HexViewUIntVM()
        {
            for (int i = 0; i < 4; i++)
                Bytes.Add(new HexViewByteVM());
        }

        internal void Reset()
        {
            foreach (var a in Bytes)
                a.Reset();
        }
    }

    class HexViewLineVM : BindingObject
    {
        private string offset = "0";
        public string Offset => offset;

        private string asText = "................";
        public string AsText => asText;

        public void SetOffset(long offset)
        {
            this.offset = String.Format("0x{0:X8}", offset);
            Notify("Offset");
        }

        public void SetBytes(byte[] bytes)
        {
            if (bytes.Length <= 16)
            {
                int[] pos = new int[] { 0 };

                if (bytes.Length > 12)
                    pos = new int[] { 0, 4, 8, 12 };
                else if (bytes.Length > 8)
                    pos = new int[] { 0, 4, 8 };
                else if (bytes.Length > 4)
                    pos = new int[] { 0, 4 };

                var a = bytes.Split(pos).ToArray();
                for (int i = 0; i < a.Length; i++)
                    UInts[i].SetBytes(a[i]);

                Encode(bytes);
            }
        }

        public ObservableCollection<HexViewUIntVM> UInts { get; } = new ObservableCollection<HexViewUIntVM>();

        public HexViewLineVM()
        {
            for (int i = 0; i < 4; i++)
                UInts.Add(new HexViewUIntVM());
        }

        private void Encode(byte[] bytes)
        {
            asText = new string(Encoding.ASCII.GetChars(bytes).Select(x =>
            {
                if (x < 0x20)
                    return '.';
                else
                    return x;
            }).ToArray());


            while (asText.Length < 16)
                asText += ".";
            Notify("AsText");
        }

        internal void Reset()
        {
            asText = "................";
            Notify("AsText");
            foreach (var a in UInts)
                a.Reset();
        }
    }

    class HexViewVM : BindingObject
    {
        #region Private

        private Stream stream;

        private double ViewHeight = 0;
        private double TableHeight = 0;

        private long startOffset = 0;

        private double sizeColumnWidth = 80;

        #endregion Private

        #region Public

        public ObservableCollection<HexViewLineVM> Lines { get; } = new ObservableCollection<HexViewLineVM>();

        public double SizeColumnWidth => sizeColumnWidth;

        public double ActualHeight
        {
            set
            {
                TableHeight = value;
                CompareHeight();
            }
        }

        public FontFamily FontFamily { get; } = new FontFamily(System.Drawing.FontFamily.GenericMonospace.Name);

        #endregion Public

        private void UpdateLines()
        {
            for (int i = 0; i < Lines.Count; i++)
                SetLine(Lines[i], i);
        }

        private void SetLine(HexViewLineVM line, int index)
        {
            long newoffset = startOffset + index * 16;
            line.SetOffset(newoffset);
            line.Reset();
            if (this.stream is Stream stream && stream.Length > newoffset)
            {
                stream.Position = newoffset;
                long available = stream.Length - stream.Position;
                if (available >= 16)
                {
                    byte[] temp = new byte[16];
                    stream.Read(temp, 0, 16);
                    line.SetBytes(temp);
                }
                else
                {
                    byte[] temp = new byte[available];
                    stream.Read(temp, 0, (int)available);
                    line.SetBytes(temp);
                }
            }
        }

        private void AddLine()
        {
            var line = new HexViewLineVM();
            SetLine(line, Lines.Count);
            Lines.Add(line);
        }

        private void RemoveLine()
        {
            Lines.RemoveAt(Lines.Count - 1);
        }

        private void CompareHeight()
        {
            // double h = Lines.Count == 0 ? 0 : TableHeight / Lines.Count;

            if (TableHeight < ViewHeight)
                AddLine();
            //else
            //{
            //    if (TableHeight - 2 * h > ViewHeight)
            //        RemoveLine();
            //}
        }

        public void SetHeight(double height)
        {
            ViewHeight = height;
            CompareHeight();
        }

        public void SetStartOffset(long offset)
        {
            if (startOffset != offset)
            {
                startOffset = offset;
                UpdateLines();
            }
        }

        public void SetStream(Stream stream)
        {
            this.stream = stream;
            UpdateLines();
        }

        public HexViewVM()
        {
        }
    }
}