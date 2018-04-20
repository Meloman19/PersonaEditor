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
using PersonaEditorGUI.Classes.Visual;
using System.Windows;
using System.Windows.Media;
using PersonaEditorLib.Interfaces;
using System.Windows.Input;

namespace PersonaEditorGUI.Controls.Editors
{
    class PTPNameEditVM : BindingObject
    {
        private PTPName name;

        private Encoding OldEncoding;
        private Encoding NewEncoding;

        public TextVisual OldNameVisual { get; } = new TextVisual();
        public TextVisual NewNameVisual { get; } = new TextVisual();

        public int Index => name.Index;
        public string OldName => name.OldName.GetTextBaseList().GetString(OldEncoding);
        public string NewName
        {
            get { return name.NewName; }
            set
            {
                if (name.NewName != value)
                {
                    name.NewName = value;
                    NewNameVisual.UpdateText(value.GetTextBaseList(NewEncoding));
                    Notify("NewName");
                }
            }
        }

        public void UpdateOldEncoding(string oldEncoding)
        {
            OldEncoding = Static.EncodingManager.GetPersonaEncoding(oldEncoding);
            OldNameVisual.UpdateFont(Static.FontManager.GetPersonaFont(oldEncoding));
            Notify("OldName");
        }

        public void UpdateNewEncoding(string newEncoding)
        {
            NewEncoding = Static.EncodingManager.GetPersonaEncoding(newEncoding);
            NewNameVisual.UpdateFont(Static.FontManager.GetPersonaFont(newEncoding));
        }

        public void UpdateBackground(int background)
        {
            if (Static.BackManager.GetBackground(background) is Background back)
            {
                OldNameVisual.Start = back.NameStart;
                NewNameVisual.Start = back.NameStart;

                OldNameVisual.Color = back.ColorName;
                NewNameVisual.Color = back.ColorName;

                OldNameVisual.LineSpacing = back.LineSpacing;
                NewNameVisual.LineSpacing = back.LineSpacing;

                OldNameVisual.GlyphScale = back.GlyphScale;
                NewNameVisual.GlyphScale = back.GlyphScale;
            }
        }

        public void UpdateView(bool view)
        {
            OldNameVisual.IsEnable = view;
            NewNameVisual.IsEnable = view;
        }

        public PTPNameEditVM(PTPName name, int oldEncoding, int newEncoding, int background)
        {
            this.name = name;
            UpdateOldEncoding(Static.EncodingManager.GetPersonaEncodingName(oldEncoding));
            UpdateNewEncoding(Static.EncodingManager.GetPersonaEncodingName(newEncoding));
            UpdateBackground(background);

            OldNameVisual.UpdateText(name.OldName);
            NewNameVisual.UpdateText(name.NewName.GetTextBaseList(NewEncoding));
        }
    }

    class PTPMsgStrEditVM : BindingObject
    {
        MSGstr str;
        EventWrapper strEW;

        private Encoding OldEncoding;
        private Encoding NewEncoding;

        TextVisual OldText = new TextVisual();
        TextVisual NewText = new TextVisual();

        public string Prefix => str.Prefix.MSGListToSystem();
        public string Postfix => str.Postfix.MSGListToSystem();
        public string OldString => str.OldString.GetString((OldEncoding), true);
        public string NewString
        {
            get { return str.NewString; }
            set
            {
                str.NewString = value;
                NewText.UpdateText(str.NewString.GetTextBaseList(NewEncoding));
            }
        }

        public DrawingImage OldTextImage { get; } = new DrawingImage();
        public DrawingImage NewTextImage { get; } = new DrawingImage();

        public ICommand MovePrefixDown { get; }
        public ICommand MovePrefixUp { get; }
        public ICommand MovePostfixDown { get; }
        public ICommand MovePostfixUp { get; }

        public void UpdateOldEncoding(string oldEncoding)
        {
            OldEncoding = Static.EncodingManager.GetPersonaEncoding(oldEncoding);
            OldText.UpdateFont(Static.FontManager.GetPersonaFont(oldEncoding));
            Notify("OldString");
        }

        public void UpdateNewEncoding(string newEncoding)
        {
            NewEncoding = Static.EncodingManager.GetPersonaEncoding(newEncoding);
            NewText.UpdateText(str.NewString.GetTextBaseList(NewEncoding), Static.FontManager.GetPersonaFont(newEncoding));
        }

        public void UpdateBackground(int backgroundIndex)
        {
            if (Static.BackManager.GetBackground(backgroundIndex) is Background back)
            {
                OldText.Start = back.TextStart;
                NewText.Start = back.TextStart;

                OldText.Color = back.ColorText;
                NewText.Color = back.ColorText;

                OldText.LineSpacing = back.LineSpacing;
                NewText.LineSpacing = back.LineSpacing;

                OldText.GlyphScale = back.GlyphScale;
                NewText.GlyphScale = back.GlyphScale;
            }
        }

        public void UpdateView(bool view)
        {
            OldText.IsEnable = view;
            NewText.IsEnable = view;
        }

        public PTPMsgStrEditVM(MSGstr str, Tuple<ImageDrawing, ImageDrawing, ImageDrawing, RectangleGeometry> tuple, string oldEncoding, string newEncoding, int backgroundIndex)
        {
            this.str = str;
            strEW = new EventWrapper(str, this);

            OldEncoding = Static.EncodingManager.GetPersonaEncoding(oldEncoding);
            NewEncoding = Static.EncodingManager.GetPersonaEncoding(newEncoding);
            OldText = new TextVisual(Static.FontManager.GetPersonaFont(oldEncoding)) { Tag = "Old" };
            NewText = new TextVisual(Static.FontManager.GetPersonaFont(newEncoding)) { Tag = "New" };
            OldText.IsEnable = Settings.AppSetting.Default.PTPImageView;
            NewText.IsEnable = Settings.AppSetting.Default.PTPImageView;

            UpdateBackground(backgroundIndex);

            OldText.UpdateText(str.OldString);
            NewText.UpdateText(str.NewString.GetTextBaseList(NewEncoding));

            DrawingGroup oldDrawingGroup = new DrawingGroup();
            oldDrawingGroup.Children.Add(tuple.Item3);
            if (tuple.Item1 != null)
                oldDrawingGroup.Children.Add(tuple.Item1);
            oldDrawingGroup.Children.Add(OldText.TextDrawing);
            oldDrawingGroup.ClipGeometry = tuple.Item4;
            OldTextImage.Drawing = oldDrawingGroup;

            DrawingGroup newDrawingGroup = new DrawingGroup();
            newDrawingGroup.Children.Add(tuple.Item3);
            if (tuple.Item2 != null)
                newDrawingGroup.Children.Add(tuple.Item2);
            newDrawingGroup.Children.Add(NewText.TextDrawing);
            newDrawingGroup.ClipGeometry = tuple.Item4;
            NewTextImage.Drawing = newDrawingGroup;

            MovePrefixDown = new RelayCommand(str.MovePrefixDown);
            MovePrefixUp = new RelayCommand(str.MovePrefixUp);
            MovePostfixDown = new RelayCommand(str.MovePostfixDown);
            MovePostfixUp = new RelayCommand(str.MovePostfixUp);
        }

        public override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Notify(e.PropertyName);
        }
    }

    class PTPMsgVM : BindingObject
    {
        MSG msg;
        private int BackgroundIndex;

        public string Name => msg.Name;

        public ObservableCollection<PTPMsgStrEditVM> Strings { get; } = new ObservableCollection<PTPMsgStrEditVM>();

        public void UpdateOldEncoding(string OldEncoding)
        {
            foreach (var a in Strings)
                a.UpdateOldEncoding(OldEncoding);
        }

        public void UpdateNewEncoding(string NewEncoding)
        {
            foreach (var a in Strings)
                a.UpdateNewEncoding(NewEncoding);
        }

        public void UpdateBackground()
        {
            //foreach (var a in Strings)
            //    a.UpdateBackground();
        }

        public void UpdateView(bool isEnable)
        {
            foreach (var a in Strings)
                a.UpdateView(isEnable);
        }

        public void UpdateBackground(int BackgroundIndex)
        {
            this.BackgroundIndex = BackgroundIndex;

            foreach (var a in Strings)
                a.UpdateBackground(BackgroundIndex);
        }

        public PTPMsgVM(MSG msg, Tuple<ImageDrawing, ImageDrawing, ImageDrawing, RectangleGeometry> tuple, string OldEncoding, string NewEncoding, int backgroundIndex)
        {
            BackgroundIndex = backgroundIndex;
            this.msg = msg;

            foreach (var a in msg.Strings)
                Strings.Add(new PTPMsgStrEditVM(a, tuple, OldEncoding, NewEncoding, BackgroundIndex));
        }
    }

    class PTPEditorVM : BindingObject, IViewModel
    {
        #region Private

        EventWrapper EncodingManagerEW;

        #endregion Private

        private int Background;

        private Background selectBack = null;
        private Background SelectBack
        {
            get { return selectBack; }
            set
            {
                if (selectBack != value)
                {
                    if (selectBack != null)
                        selectBack.BackgroundChanged -= SelectBack_BackgroundChanged;
                    selectBack = value;
                    if (selectBack != null)
                        selectBack.BackgroundChanged += SelectBack_BackgroundChanged;
                }
            }
        }

        public ImageDrawing BackgroundDrawing { get; } = new ImageDrawing();
        public RectangleGeometry ClipGeometry { get; } = new RectangleGeometry();

        private void SelectBack_BackgroundChanged(Background background)
        {
            UpdateBackground(SelectedBackgroundIndex);
            BackgroundDrawing.ImageSource = SelectBack.Image;
            BackgroundDrawing.Rect = SelectBack.Rect;
            ClipGeometry.Rect = SelectBack.Rect;
        }

        public ReadOnlyObservableCollection<string> BackgroundList => Static.BackManager.BackgroundList;
        public int SelectedBackgroundIndex
        {
            get { return Background; }
            set
            {
                if (SelectBack != Static.BackManager.GetBackground(value) && Static.BackManager.GetBackground(value) != null)
                {
                    SelectBack = Static.BackManager.GetBackground(value);
                    Background = value;
                    Settings.AppSetting.Default.PTPBackgroundDefault = Static.BackManager.GetBackgroundName(value);
                    UpdateBackground(value);
                    BackgroundDrawing.ImageSource = SelectBack.Image;
                    BackgroundDrawing.Rect = SelectBack.Rect;
                    ClipGeometry.Rect = SelectBack.Rect;
                }
                Notify("SelectedBackgroundIndex");
            }
        }

        private int OldEncoding;
        private int NewEncoding;
        private bool View;

        public ReadOnlyObservableCollection<string> FontList => Static.EncodingManager.EncodingList;

        public int SelectedOldFont
        {
            get { return OldEncoding; }
            set
            {
                OldEncoding = value;
                Settings.AppSetting.Default.PTPOldDefault = Static.EncodingManager.GetPersonaEncodingName(value);
                UpdateOldEncoding(Settings.AppSetting.Default.PTPOldDefault);
                Notify("SelectedOldFont");
            }
        }

        public int SelectedNewFont
        {
            get { return NewEncoding; }
            set
            {
                NewEncoding = value;
                Settings.AppSetting.Default.PTPNewDefault = Static.EncodingManager.GetPersonaEncodingName(value);
                UpdateNewEncoding(Settings.AppSetting.Default.PTPNewDefault);
                Notify("SelectedNewFont");
            }
        }

        public bool ViewImage
        {
            get { return View; }
            set
            {
                if (View != value)
                {
                    View = value;
                    Settings.AppSetting.Default.PTPImageView = value;
                    Notify("ViewImage");
                    UpdateView();
                }
            }
        }

        public ObservableCollection<PTPNameEditVM> Names { get; } = new ObservableCollection<PTPNameEditVM>();
        public ObservableCollection<PTPMsgVM> MSG { get; } = new ObservableCollection<PTPMsgVM>();

        public override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is PersonaEditorLib.PersonaEncoding.PersonaEncodingManager man)
            {
                if (e.PropertyName == man.GetPersonaEncodingName(OldEncoding))
                    UpdateOldEncoding(Settings.AppSetting.Default.PTPOldDefault);
                if (e.PropertyName == man.GetPersonaEncodingName(NewEncoding))
                    UpdateNewEncoding(Settings.AppSetting.Default.PTPNewDefault);
            }
        }

        private void UpdateOldEncoding(string oldEncoding)
        {
            foreach (var a in Names)
                a.UpdateOldEncoding(oldEncoding);

            foreach (var a in MSG)
                a.UpdateOldEncoding(oldEncoding);
        }

        private void UpdateNewEncoding(string newEncoding)
        {
            foreach (var a in Names)
                a.UpdateNewEncoding(newEncoding);

            foreach (var a in MSG)
                a.UpdateNewEncoding(newEncoding);
        }

        private void UpdateBackground(int backgroundIndex)
        {
            foreach (var a in Names)
                a.UpdateBackground(backgroundIndex);

            foreach (var a in MSG)
                a.UpdateBackground(backgroundIndex);
        }

        private void UpdateView()
        {
            foreach (var a in Names)
                a.UpdateView(View);

            foreach (var a in MSG)
                a.UpdateView(View);
        }

        public PTPEditorVM(PTP ptp)
        {
            int sourceInd = Static.EncodingManager.GetPersonaEncodingIndex(Settings.AppSetting.Default.PTPOldDefault);
            if (sourceInd >= 0)
                OldEncoding = sourceInd;
            else
                OldEncoding = 0;

            sourceInd = Static.EncodingManager.GetPersonaEncodingIndex(Settings.AppSetting.Default.PTPNewDefault);
            if (sourceInd >= 0)
                NewEncoding = sourceInd;
            else
                NewEncoding = 0;

            sourceInd = Static.BackManager.GetBackgroundIndex(Settings.AppSetting.Default.PTPBackgroundDefault);
            if (sourceInd >= 0)
                SelectedBackgroundIndex = sourceInd;
            else
                SelectedBackgroundIndex = 0;

            View = Settings.AppSetting.Default.PTPImageView;
            EncodingManagerEW = new EventWrapper(Static.EncodingManager, this);

            foreach (var a in ptp.names)
                Names.Add(new PTPNameEditVM(a, OldEncoding, NewEncoding, SelectedBackgroundIndex));



            foreach (var a in ptp.msg)
            {
                var name = Names.FirstOrDefault(x => x.Index == a.CharacterIndex);
                Tuple<ImageDrawing, ImageDrawing, ImageDrawing, RectangleGeometry> tuple;
                if (name == null)
                    tuple = new Tuple<ImageDrawing, ImageDrawing, ImageDrawing, RectangleGeometry>(null, null, BackgroundDrawing, ClipGeometry);
                else
                    tuple = new Tuple<ImageDrawing, ImageDrawing, ImageDrawing, RectangleGeometry>(name.OldNameVisual.TextDrawing, name.NewNameVisual.TextDrawing, BackgroundDrawing, ClipGeometry);

                MSG.Add(new PTPMsgVM(a, tuple, Settings.AppSetting.Default.PTPOldDefault, Settings.AppSetting.Default.PTPNewDefault, SelectedBackgroundIndex));
            }
        }

        public bool Close()
        {
            SelectBack = null;
            return true;
        }
    }
}