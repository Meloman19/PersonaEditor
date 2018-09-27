using AuxiliaryLibraries.GameFormat.Other;
using AuxiliaryLibraries.GameFormat.Text;
using AuxiliaryLibraries.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace PersonaEditorGUI.Controls.Editors
{
    class FTDMultiVM : FTDEntryVM
    {
        public string ButtonContent { get; set; } = "+";
        public ICommand Expand { get; }
        public ICommand Resize { get; }
        public Visibility Visibility { get; set; } = Visibility.Collapsed;
        public ObservableCollection<FTDSingleVM> MultiElements { get; } = new ObservableCollection<FTDSingleVM>();

        private void expand()
        {
            if (Visibility == Visibility.Collapsed)
            {
                ButtonContent = "-";
                Visibility = Visibility.Visible;
            }
            else
            {
                ButtonContent = "+";
                Visibility = Visibility.Collapsed;
            }

            Notify("Visibility");
            Notify("ButtonContent");
        }

        private void resize()
        {
            ToolBox.Resize resize = new ToolBox.Resize();
            resize.Size = ftd.Entries[index][0].Length;
            if (resize.ShowDialog() == true)
            {
                foreach (var a in MultiElements)
                    a.Resize(resize.Size);
            }
        }

        public FTDMultiVM(FTD ftd, int index, Encoding encoding) : base(ftd, index, encoding)
        {
            Expand = new RelayCommand(expand);
            Resize = new RelayCommand(resize);

            for (int i = 0; i < ftd.Entries[index].Length; i++)
                MultiElements.Add(new FTDSingleVM(ftd, index, i, encoding));
        }

        public override void OnSetEncoding()
        {
            foreach (var a in MultiElements)
                a.SetEncoding(encoding);
        }
    }

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

    abstract class FTDEntryVM : BindingObject
    {
        protected FTD ftd;
        protected int index;
        protected Encoding encoding;

        public FTDEntryVM(FTD ftd, int index, Encoding encoding)
        {
            this.ftd = ftd;
            this.index = index;
            this.encoding = encoding;
        }

        public void SetEncoding(Encoding encoding)
        {
            this.encoding = encoding;
            OnSetEncoding();
        }

        public abstract void OnSetEncoding();
    }

    class FTDEditorVM : BindingObject, IViewModel
    {
        FTD ftd;

        public ObservableCollection<FTDEntryVM> Entrie { get; } = new ObservableCollection<FTDEntryVM>();

        public ReadOnlyObservableCollection<string> EncodingList { get; }

        private int selectEncodingIndex = 0;
        public int SelectEncodingIndex
        {
            get { return selectEncodingIndex; }
            set
            {
                selectEncodingIndex = value;
                Settings.AppSetting.Default.FTDEncoding = Static.EncodingManager.GetPersonaEncodingName(value);
                UpdateEncoding();
            }
        }

        public FontFamily FontFamily { get; } = new FontFamily(System.Drawing.FontFamily.GenericMonospace.Name);

        public FTDEditorVM(FTD ftd)
        {
            EncodingList = Static.EncodingManager.EncodingList;
            selectEncodingIndex = Static.EncodingManager.GetPersonaEncodingIndex(Settings.AppSetting.Default.FTDEncoding);
            this.ftd = ftd;
            Init();
        }

        private void Init()
        {
            Entrie.Clear();
            for (int i = 0; i < ftd.Entries.Count; i++)
            {
                if (ftd.Entries[i].Length == 1)
                    Entrie.Add(new FTDSingleVM(ftd, i, 0, Static.EncodingManager.GetPersonaEncoding(selectEncodingIndex)));
                else
                    Entrie.Add(new FTDMultiVM(ftd, i, Static.EncodingManager.GetPersonaEncoding(selectEncodingIndex)));
            }
        }

        private void UpdateEncoding()
        {
            foreach (var a in Entrie)
                a.SetEncoding(Static.EncodingManager.GetPersonaEncoding(SelectEncodingIndex));
        }

        public bool Close()
        {
            return true;
        }


    }
}