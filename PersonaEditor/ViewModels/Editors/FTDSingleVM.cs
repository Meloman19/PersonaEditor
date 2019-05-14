using PersonaEditorLib.Other;
using AuxiliaryLibraries.WPF;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace PersonaEditor.ViewModels.Editors
{
    class FTDSingleVM : FTDEntryVM
    {
        protected int subIndex = 0;

        public byte[] Data => ftd.Entries[index][subIndex];
        public string DataDecode => encoding?.GetString(ftd.Entries[index][subIndex]);
        public ICommand CopyData { get; }
        public ICommand PasteData { get; }

        private void copyData()
        {
            Clipboard.Clear();
            Clipboard.SetText(System.BitConverter.ToString(Data));
        }

        private void pasteData()
        {
            var data = Clipboard.GetText();

            if (!string.IsNullOrEmpty(data))
            {
                List<byte> result = new List<byte>();

                var dataArray = Regex.Split(data, $"\r\n|\r|\n");

                foreach (var line in dataArray)
                {
                    var split = line.Split(' ', '-');

                    foreach (var b in split)
                    {
                        try
                        {
                            byte temp = Convert.ToByte(b, 16);
                            result.Add(temp);
                        }
                        catch { }
                    }
                }

                if (result.Count != 0)
                {
                    var newData = result.ToArray();

                    if (ftd.Entries[index].Length != 1)
                        Array.Resize(ref newData, ftd.Entries[index][subIndex].Length);

                    ftd.Entries[index][subIndex] = newData;

                    Notify("Data");
                    Notify("DataDecode");
                }
            }
        }

        public FTDSingleVM(FTD ftd, int index, int subIndex, Encoding encoding) : base(ftd, index, encoding)
        {
            CopyData = new RelayCommand(copyData);
            PasteData = new RelayCommand(pasteData);
            this.subIndex = subIndex;
        }

        public override void OnSetEncoding()
        {
            Notify("DataDecode");
        }

        public void Resize(int size)
        {
            var temp = ftd.Entries[index][subIndex];
            Array.Resize(ref temp, size);
            ftd.Entries[index][subIndex] = temp;

            Notify("Data");
            Notify("DataDecode");
        }
    }
}