using PersonaEditorLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PersonaEditorLib.FileStructure.Text;
using System.IO;
using System.Windows.Data;
using System.Globalization;
using PersonaEditorLib.Extension;
using System.Windows.Media.Imaging;
using PersonaEditorGUI.Classes.Media.Visual;
using System.Windows;
using System.Windows.Media;
using PersonaEditorLib.Interfaces;

namespace PersonaEditorGUI.Controls.Editors
{
    class PTPMsgStrEditVM : BindingObject
    {
        TextVisual OldText;
        EventWrapper OldTextEW;
        TextVisual NewText;
        EventWrapper NewTextEW;

        BackgroundImage BackgroundImg;
        EventWrapper BackgroundEW;

        CharList OldChar;
        CharList NewChar;
        EventWrapper OldCharEW;
        EventWrapper NewCharEW;

        MSG.MSGstr str;
        EventWrapper strEW;

        public string Prefix
        {
            get { return MSGListToSystem(str.Prefix); }
        }
        public string Postfix
        {
            get { return MSGListToSystem(str.Postfix); }
        }
        public string OldString
        {
            get { return str.OldString.GetString(OldChar, true); }
        }
        public string NewString
        {
            get { return str.NewString; }
            set { str.NewString = value; }
        }

        public ImageSource OldTextSource => OldText.Image;
        public Rect OldTextRect => OldText.Rect;

        public ImageSource NewTextSource => NewText.Image;
        public Rect NewTextRect => NewText.Rect;

        public override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is TextVisual visual)
            {
                if (visual.Tag == "Old")
                {
                    if (e.PropertyName == "Image")
                        Notify("OldTextSource");
                    else if (e.PropertyName == "Rect")
                        Notify("OldTextRect");
                }
                else if (visual.Tag == "New")
                {
                    if (e.PropertyName == "Image")
                        Notify("NewTextSource");
                    else if (e.PropertyName == "Rect")
                        Notify("NewTextRect");
                }
            }
            else if (sender is MSG.MSGstr Msg)
            {
                if (e.PropertyName == "NewString")
                {
                    NewText.UpdateText(str.NewString.GetTextBaseList(NewChar));
                }
            }
            else if (sender is CharList charlist)
            {
                if (charlist.Tag == "New")
                    Notify("NewString");
                else if (charlist.Tag == "Old")
                    Notify("OldString");
            }
            else if (sender is BackgroundImage image)
            {
                if (e.PropertyName == "TextStart")
                {
                    OldText.Start = image.TextStart;
                    NewText.Start = image.TextStart;
                }
                else if (e.PropertyName == "ColorText")
                {
                    OldText.Color = image.ColorText;
                    NewText.Color = image.ColorText;
                }
                else if (e.PropertyName == "LineSpacing")
                {
                    OldText.LineSpacing = image.LineSpacing;
                    NewText.LineSpacing = image.LineSpacing;
                }
                else if (e.PropertyName == "GlyphScale")
                {
                    OldText.GlyphScale = image.GlyphScale;
                    NewText.GlyphScale = image.GlyphScale;
                }
            }
        }

        public PTPMsgStrEditVM(MSG.MSGstr str, CharList Old, CharList New, BackgroundImage backgroundImage)
        {
            this.str = str;
            strEW = new EventWrapper(str, this);

            NewChar = New;
            OldChar = Old;
            OldCharEW = new EventWrapper(Old, this);
            NewCharEW = new EventWrapper(New, this);

            BackgroundImg = backgroundImage;
            BackgroundEW = new EventWrapper(backgroundImage, this);

            OldText = new TextVisual(Old) { Tag = "Old" };
            OldTextEW = new EventWrapper(OldText, this);
            OldText.UpdateText(str.OldString);
            NewText = new TextVisual(New) { Tag = "New" };
            NewTextEW = new EventWrapper(NewText, this);
            NewText.UpdateText(str.NewString);

            SetBack(backgroundImage);
        }

        string MSGListToSystem(IList<TextBaseElement> list)
        {
            string returned = "";
            foreach (var Bytes in list)
            {
                byte[] temp = Bytes.Array.ToArray();
                if (temp.Length > 0)
                {
                    returned += "{" + System.Convert.ToString(temp[0], 16).PadLeft(2, '0').ToUpper();
                    for (int i = 1; i < temp.Length; i++)
                    {
                        returned += "\u00A0" + System.Convert.ToString(temp[i], 16).PadLeft(2, '0').ToUpper();
                    }
                    returned += "} ";
                }
            }
            return returned;
        }

        private void SetBack(BackgroundImage image)
        {
            OldText.Start = image.TextStart;
            NewText.Start = image.TextStart;

            OldText.Color = image.ColorText;
            NewText.Color = image.ColorText;

            OldText.LineSpacing = image.LineSpacing;
            NewText.LineSpacing = image.LineSpacing;

            OldText.GlyphScale = image.GlyphScale;
            NewText.GlyphScale = image.GlyphScale;
        }
    }

    class PTPNameEditVM : BindingObject
    {
        PTPName name;
        EventWrapper OldCharEW;

        private string _OldName = "";

        public int Index => name.Index;
        public string OldName => _OldName;
        public string NewName
        {
            get { return name.NewName; }
            set
            {
                if (name.NewName != value)
                {
                    name.NewName = value;
                    Notify("NewName");
                }
            }
        }

        public override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is CharList charlist)
            {
                _OldName = name.OldName.GetTextBaseList().GetString(charlist);
                Notify("OldName");
            }
        }

        public PTPNameEditVM(PTPName name, CharList Old)
        {
            this.name = name;
            OldCharEW = new EventWrapper(Old, this);
            _OldName = name.OldName.GetTextBaseList().GetString(Old);
        }
    }

    class PTPMsgVM : BindingObject
    {
        MSG msg;

        EventWrapper BackgroundEW;

        EventWrapper PTPNameEW;

        TextVisual OldName;
        EventWrapper OldNameEW;
        TextVisual NewName;
        EventWrapper NewNameEW;

        public string Name => msg.Name;

        public ImageSource OldNameImage => OldName.Image;
        public Rect OldNameRect => OldName.Rect;

        public ImageSource NewNameImage => NewName.Image;
        public Rect NewNameRect => NewName.Rect;

        public ObservableCollection<PTPMsgStrEditVM> Strings { get; } = new ObservableCollection<PTPMsgStrEditVM>();

        public override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is PTPName name)
            {
                if (e.PropertyName == "NewName")
                    NewName.UpdateText(name.NewName);
                else if (e.PropertyName == "OldName")
                    OldName.UpdateText(name.OldName);
            }
            else if (sender is TextVisual vis)
            {
                if (vis.Tag == "Old")
                {
                    if (e.PropertyName == "Image")
                        Notify("OldNameImage");
                    else if (e.PropertyName == "Rect")
                        Notify("OldNameRect");
                }
                else if (vis.Tag == "New")
                {
                    if (e.PropertyName == "Image")
                        Notify("NewNameImage");
                    else if (e.PropertyName == "Rect")
                        Notify("NewNameRect");
                }
            }
            else if (sender is BackgroundImage image)
            {
                if (e.PropertyName == "NameStart")
                {
                    OldName.Start = image.NameStart;
                    NewName.Start = image.NameStart;
                }
                else if (e.PropertyName == "ColorName")
                {
                    OldName.Color = image.ColorName;
                    NewName.Color = image.ColorName;
                }
                else if (e.PropertyName == "LineSpacing")
                {
                    OldName.LineSpacing = image.LineSpacing;
                    NewName.LineSpacing = image.LineSpacing;
                }
                else if (e.PropertyName == "GlyphScale")
                {
                    OldName.GlyphScale = image.GlyphScale;
                    NewName.GlyphScale = image.GlyphScale;
                }
            }

        }

        public PTPMsgVM(MSG msg, IList<PTPName> names, CharList Old, CharList New, BackgroundImage backgroundImage)
        {
            this.msg = msg;

            foreach (var a in msg.Strings)
                Strings.Add(new PTPMsgStrEditVM(a, Old, New, backgroundImage));

            BackgroundEW = new EventWrapper(backgroundImage, this);



            OldName = new TextVisual(Old) { Tag = "Old" };
            NewName = new TextVisual(New) { Tag = "New" };

            if (names.FirstOrDefault(x => x.Index == msg.CharacterIndex) is PTPName name)
            {
                OldNameEW = new EventWrapper(OldName, this);
                OldName.UpdateText(name.OldName);
                NewNameEW = new EventWrapper(NewName, this);
                NewName.UpdateText(name.NewName);
                PTPNameEW = new EventWrapper(name, this);
            }

            SetBack(backgroundImage);
        }

        private void SetBack(BackgroundImage image)
        {
            OldName.Start = image.NameStart;
            NewName.Start = image.NameStart;

            OldName.Color = image.ColorName;
            NewName.Color = image.ColorName;

            OldName.LineSpacing = image.LineSpacing;
            NewName.LineSpacing = image.LineSpacing;

            OldName.GlyphScale = image.GlyphScale;
            NewName.GlyphScale = image.GlyphScale;
        }
    }

    class PTPEditorVM : BindingObject, IViewModel
    {
        #region Private

        EventWrapper BackgroundEW;
        Backgrounds BackImage { get; } = new Backgrounds(Settings.App.Default.DirBackground);

        CharList OldCharList { get; } = new CharList() { Tag = "Old" };
        CharList NewCharList { get; } = new CharList() { Tag = "New" };

        #endregion Private

        public ReadOnlyObservableCollection<string> BackgroundList => BackImage.BackgroundList;
        public int SelectedIndex
        {
            get { return BackImage.SelectedIndex; }
            set
            {
                BackImage.SelectedIndex = value;
                Settings.App.Default.PTPBackgroundDefault = BackImage.SelectedItem;
            }
        }

        public ReadOnlyObservableCollection<string> OldFontList => OldCharList.FontList;
        public int OldFontIndex
        {
            get { return OldCharList.SelectedIndex; }
            set
            {
                OldCharList.SelectedIndex = value;
                Settings.App.Default.PTPOldDefault = OldCharList.SelectedItem;
                Notify("OldFontIndex");
            }
        }

        public ReadOnlyObservableCollection<string> NewFontList => NewCharList.FontList;
        public int NewFontIndex
        {
            get { return NewCharList.SelectedIndex; }
            set
            {
                NewCharList.SelectedIndex = value;
                Settings.App.Default.PTPNewDefault = NewCharList.SelectedItem;
                Notify("NewFontIndex");
            }
        }

        public BitmapSource BackgroundImage => BackImage.CurrentBackground.Image;
        public Rect BackgroundRect => BackImage.CurrentBackground.Rect;

        public ObservableCollection<PTPNameEditVM> Names { get; } = new ObservableCollection<PTPNameEditVM>();
        public ObservableCollection<PTPMsgVM> MSG { get; } = new ObservableCollection<PTPMsgVM>();

        public override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is BackgroundImage image)
            {
                if (e.PropertyName == "Image")
                    Notify("BackgroundImage");
                else if (e.PropertyName == "Rect")
                    Notify("BackgroundRect");
            }
        }

        public PTPEditorVM(PTP ptp)
        {
            BackImage.SelectedItem = Settings.App.Default.PTPBackgroundDefault;

            OldCharList.GetFontList(Settings.App.Default.DirFont);
            OldCharList.SelectedItem = Settings.App.Default.PTPOldDefault;
            NewCharList.GetFontList(Settings.App.Default.DirFont);
            NewCharList.SelectedItem = Settings.App.Default.PTPNewDefault;

            BackgroundEW = new EventWrapper(BackImage.CurrentBackground, this);

            foreach (var a in ptp.names)
                Names.Add(new PTPNameEditVM(a, OldCharList));

            foreach (var a in ptp.msg)
                MSG.Add(new PTPMsgVM(a, ptp.names, OldCharList, NewCharList, BackImage.CurrentBackground));
        }

        public bool Close()
        {
            return true;
        }
    }
}