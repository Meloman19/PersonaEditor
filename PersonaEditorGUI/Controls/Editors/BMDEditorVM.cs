using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PersonaEditorLib.FileStructure.BMD;
using PersonaEditorLib;
using PersonaEditorLib.Extension;
using PersonaEditorLib.FileStructure.PTP;
using System.ComponentModel;
using System.Windows;

namespace PersonaEditorGUI.Controls.Editors
{
    class BMDMsgStrVM : BindingObject
    {
        CharList charList;
        EventWrapper CharListEW;

        public byte[] data { get; private set; }

        public string Text { get; set; }

        public void Changes(bool save)
        {
            if (save)
                data = Text.GetTextBaseList(charList).GetByteArray();
            else
            {
                Text = data.GetTextBaseList().GetString(charList, false);
                Notify("Text");
            }
        }

        public override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is CharList charlist)
            {
                Text = data.GetTextBaseList().GetString(charList, false);
                Notify("Text");
            }
        }

        public BMDMsgStrVM(byte[] array, CharList charList)
        {
            data = array;

            this.charList = charList;
            CharListEW = new EventWrapper(charList, this);

            Text = data.GetTextBaseList().GetString(charList, false);
        }
    }

    class BMDMsgVM : BindingObject
    {
        BMD.MSGs msg;

        public string Name => msg.Name;

        public ObservableCollection<BMDMsgStrVM> StringList { get; } = new ObservableCollection<BMDMsgStrVM>();

        public void Changes(bool save)
        {
            foreach (var a in StringList)
                a.Changes(save);

            if (save)
            {
                List<byte> temp = new List<byte>();
                foreach (var a in StringList)
                    temp.AddRange(a.data);

                msg.MsgBytes = temp.ToArray();
            }
        }

        public BMDMsgVM(BMD.MSGs msg, CharList charList)
        {
            this.msg = msg;

            var list = msg.MsgBytes.SplitSourceBytes();
            foreach (var a in list)
                StringList.Add(new BMDMsgStrVM(a, charList));
        }
    }

    class BMDNameVM : BindingObject
    {
        CharList CharList;
        EventWrapper CharListEW;

        BMD.Names name;

        public int Index => name.Index;

        public string Name { get; set; }

        public void Changes(bool save)
        {
            if (save)
                name.NameBytes = CharList.Encode(Name, CharList.EncodeOptions.Bracket);
            else
            {
                Name = name.NameBytes.GetTextBaseList().GetString(CharList);
                Notify("Name");
            }
        }

        public override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is CharList charlist)
            {
                Name = name.NameBytes.GetTextBaseList().GetString(CharList);
                Notify("Name");
            }
        }

        public BMDNameVM(BMD.Names name, CharList charList)
        {
            this.name = name;

            CharList = charList;
            CharListEW = new EventWrapper(charList, this);

            Name = name.NameBytes.GetTextBaseList().GetString(CharList);
        }
    }

    class BMDEditorVM : BindingObject
    {
        readonly CharList charList = new CharList();

        private bool _IsEdit = false;
        public bool IsEdit
        {
            get { return _IsEdit; }
            set
            {
                if (value)
                {
                    _IsEdit = value;
                    IsSelectCharList = false;
                    Notify("IsSelectCharList");
                }
                else
                {
                    if (Save())
                    {
                        _IsEdit = value;
                        IsSelectCharList = true;
                        Notify("IsSelectCharList");
                    }
                }
            }
        }


        public int SelectedIndex
        {
            get { return charList.SelectedIndex; }
            set
            {
                charList.SelectedIndex = value;
                Notify("SelectedIndex");
            }
        }

        public ReadOnlyObservableCollection<string> FontList => charList.FontList;
        public bool IsSelectCharList { get; set; } = true;

        public ObservableCollection<BMDNameVM> NameList { get; } = new ObservableCollection<BMDNameVM>();

        public ObservableCollection<BMDMsgVM> MsgList { get; } = new ObservableCollection<BMDMsgVM>();

        private bool Save()
        {
            var result = MessageBox.Show("Save changes?", "Save", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);
            if (result == MessageBoxResult.Yes | result == MessageBoxResult.No)
            {
                bool save = result == MessageBoxResult.Yes ? true : false;

                foreach (var a in NameList)
                    a.Changes(save);
                foreach (var a in MsgList)
                    a.Changes(save);

                return true;
            }
            else return false;
        }

        public BMDEditorVM(BMD bmd)
        {
            charList.GetFontList(Static.Paths.DirFont);
            charList.SelectedItem = Settings.App.Default.BMDFontDefault;

            foreach (var a in bmd.name)
                NameList.Add(new BMDNameVM(a, charList));
            foreach (var a in bmd.msg)
                MsgList.Add(new BMDMsgVM(a, charList));
        }
    }
}