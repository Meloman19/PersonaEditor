using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PersonaEditorLib.FileStructure.Text;
using PersonaEditorLib;
using PersonaEditorLib.Extension;
using System.ComponentModel;
using System.Windows;
using PersonaEditorLib.Interfaces;
using System.Windows.Documents;
using System.Windows.Controls;
using PersonaEditorGUI.Classes;

namespace PersonaEditorGUI.Controls.Editors
{
    class BMDMsgStrVM : BindingObject
    {
        int sourceFont;

        public byte[] data { get; private set; }

        public string Text { get; set; }

        // public FlowDocument Document { get; } = new FlowDocument();

        public void Changes(bool save, int destFont)
        {
            if (save)
                data = Text.GetTextBaseList(Static.EncodingManager.GetPersonaEncoding(sourceFont)).GetByteArray();
            else
            {
                Text = data.GetTextBaseList().GetString(Static.EncodingManager.GetPersonaEncoding(sourceFont));
                Notify("Text");
            }
        }

        public void Update(int sourceFont)
        {
            this.sourceFont = sourceFont;
            Text = data.GetTextBaseList().GetString(Static.EncodingManager.GetPersonaEncoding(sourceFont));
            Notify("Text");
        }

        public BMDMsgStrVM(byte[] array, int sourceFont)
        {
            data = array;
            this.sourceFont = sourceFont;

            Text = data.GetTextBaseList().GetString(Static.EncodingManager.GetPersonaEncoding(sourceFont));
            //  Style style = new Style(typeof(Paragraph));
            //  style.Setters.Add(new Setter(Block.MarginProperty, new Thickness(0)));
            //  Document.Resources.Add(typeof(Paragraph), style);
            //  Document.Blocks.Add(data.GetTextBaseList().GetDocument(TestClass.personaEncoding, false));
        }
    }

    class BMDMsgVM : BindingObject
    {
        BMD.MSGs msg;

        public string Name => msg.Name;

        public ObservableCollection<BMDMsgStrVM> StringList { get; } = new ObservableCollection<BMDMsgStrVM>();

        public void Changes(bool save, int destFont)
        {
            foreach (var a in StringList)
                a.Changes(save, destFont);

            if (save)
            {
                List<byte> temp = new List<byte>();
                foreach (var a in StringList)
                    temp.AddRange(a.data);

                msg.MsgBytes = temp.ToArray();
            }
        }

        public void Update(int sourceFont)
        {
            foreach (var a in StringList)
                a.Update(sourceFont);
        }

        public BMDMsgVM(BMD.MSGs msg, int sourceFont)
        {
            this.msg = msg;

            var list = msg.MsgBytes.SplitSourceBytes();
            foreach (var a in list)
                StringList.Add(new BMDMsgStrVM(a, sourceFont));
        }
    }

    class BMDNameVM : BindingObject
    {
        BMD.Names name;
        int sourceFont;

        public int Index => name.Index;

        public string Name { get; set; }

        public void Changes(bool save, int destFont)
        {
            if (save)
                name.NameBytes = Static.EncodingManager.GetPersonaEncoding(destFont).GetBytes(Name);
            else
            {
                Name = name.NameBytes.GetTextBaseList().GetString(Static.EncodingManager.GetPersonaEncoding(sourceFont));
                Notify("Name");
            }
        }

        public void Update(int sourceFont)
        {
            this.sourceFont = sourceFont;
            Name = name.NameBytes.GetTextBaseList().GetString(Static.EncodingManager.GetPersonaEncoding(sourceFont));
            Notify("Name");
        }

        public BMDNameVM(BMD.Names name, int sourceFont)
        {
            this.name = name;
            this.sourceFont = sourceFont;
            Name = name.NameBytes.GetTextBaseList().GetString(Static.EncodingManager.GetPersonaEncoding(sourceFont));
        }
    }

    class BMDEditorVM : BindingObject, IViewModel
    {
        EventWrapper EncodingEW;

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

                Notify("IsEdit");
            }
        }

        private int sourceFont;
        private int destFont;

        public int SelectedSourceFont
        {
            get { return sourceFont; }
            set
            {
                sourceFont = value;
                Settings.AppSetting.Default.BMDFontDefault = Static.EncodingManager.GetPersonaEncodingName(value);
                Update();
                Notify("SelectedSourceFont");
            }
        }

        public int SelectedDestFont
        {
            get { return destFont; }
            set
            {
                destFont = value;
                Settings.AppSetting.Default.BMDFontDestDefault = Static.EncodingManager.GetPersonaEncodingName(value);
                Notify("SelectedDestFont");
            }
        }

        public ReadOnlyObservableCollection<string> FontList => Static.EncodingManager.EncodingList;
        public bool IsSelectCharList { get; set; } = true;

        public ObservableCollection<BMDNameVM> NameList { get; } = new ObservableCollection<BMDNameVM>();

        public ObservableCollection<BMDMsgVM> MsgList { get; } = new ObservableCollection<BMDMsgVM>();

        private string Name = "";

        private bool Save()
        {
            var result = MessageBox.Show("Save changes?", Name, MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);
            if (result == MessageBoxResult.Yes | result == MessageBoxResult.No)
            {
                bool save = result == MessageBoxResult.Yes ? true : false;

                foreach (var a in NameList)
                    a.Changes(save, destFont);
                foreach (var a in MsgList)
                    a.Changes(save, destFont);

                return true;
            }
            else return false;
        }

        private void Update()
        {
            foreach (var a in NameList)
                a.Update(sourceFont);
            foreach (var a in MsgList)
                a.Update(sourceFont);
        }

        public bool Close()
        {
            if (IsEdit)
                if (!Save())
                    return false;

            NameList.Clear();
            MsgList.Clear();
            return true;
        }

        public override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is PersonaEditorLib.PersonaEncoding.PersonaEncodingManager man)
            {
                if (IsSelectCharList && e.PropertyName == man.GetPersonaEncodingName(sourceFont))
                    Update();
            }
        }

        public BMDEditorVM(ObjectFile objbmd)
        {
            if (objbmd.Object is BMD bmd)
            {
                EncodingEW = new EventWrapper(Static.EncodingManager, this);

                int sourceInd = Static.EncodingManager.GetPersonaEncodingIndex(Settings.AppSetting.Default.BMDFontDefault);
                if (sourceInd >= 0)
                    sourceFont = sourceInd;
                else
                    sourceFont = 0;

                sourceInd = Static.EncodingManager.GetPersonaEncodingIndex(Settings.AppSetting.Default.BMDFontDestDefault);
                if (sourceInd >= 0)
                    destFont = sourceInd;
                else
                    destFont = 0;


                foreach (var a in bmd.Name)
                    NameList.Add(new BMDNameVM(a, sourceFont));
                foreach (var a in bmd.Msg)
                    MsgList.Add(new BMDMsgVM(a, sourceFont));

                Name = objbmd.Name;
            }
        }
    }
}