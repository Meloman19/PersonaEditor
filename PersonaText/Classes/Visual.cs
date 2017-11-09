using PersonaEditorLib;
using PersonaEditorLib.FileStructure.PTP;
using PersonaEditorLib.FileTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PersonaText
{
    class VisualNEW
    {
        public void Image_Unloaded(object sender, RoutedEventArgs e)
        {
            (((sender as Image).Source as DrawingImage).Drawing as DrawingGroup).Children.Clear();
            (sender as Image).Unloaded -= Image_Unloaded;

            DrawingText = null;
            DrawingName = null;
            BackImage = null;
            CharLST = null;
        }

        public VisualNEW(CharList CharLST, BackgroundImage BackImage)
        {
            this.CharLST = CharLST;
            this.BackImage = BackImage;
        }

        ImageData CreateImageData(IList<TextBaseElement> array)
        {
            return ImageData.DrawText(array, CharLST, Static.FontMap.Shift, BackImage.LineSpacing);
        }

        ImageData CreateImageData(string text)
        {
            return ImageData.DrawText(text.GetTextBaseList(CharLST), CharLST, Static.FontMap.Shift, BackImage.LineSpacing);
        }

        ImageData CreateImageData(byte[] Name)
        {
            return ImageData.DrawText(Name.GetTextBaseList(), CharLST, Static.FontMap.Shift, BackImage.LineSpacing);
        }

        List<TextBaseElement> TextTemp = new List<TextBaseElement>();
        List<TextBaseElement> NameTemp = new List<TextBaseElement>();

        public void Text_Update(IList<TextBaseElement> array)
        {
            TextTemp.Clear();
            TextTemp.AddRange(array);
            DataText = CreateImageData(TextTemp);
        }

        public void Name_Update(IList<TextBaseElement> array)
        {
            NameTemp.Clear();
            NameTemp.AddRange(array);
            DataName = CreateImageData(NameTemp);
        }

        Point TextStart;
        Point NameStart;
        double GlyphScale;

        public void Background_Update(BackgroundImage background)
        {
            TextStart = background.TextStart;
            NameStart = background.NameStart;
            GlyphScale = background.GlyphScale;

            DataText = CreateImageData(TextTemp);
            DataName = CreateImageData(NameTemp);
        }

        BackgroundImage BackImage;

        ImageData _ImageData;
        ImageData _DataName;
        CharList CharLST;

        ImageData DataText
        {
            get { return _ImageData; }
            set
            {
                _ImageData = value;
                DrawingText.ImageSource = _ImageData.GetImageSource(PersonaEditorLib.Utilities.Utilities.CreatePallete(BackImage.ColorText, _ImageData.PixelFormat));
                DrawingText.Rect = GetSize(TextStart, _ImageData.PixelWidth, _ImageData.PixelHeight);
            }
        }
        ImageData DataName
        {
            get { return _DataName; }
            set
            {
                _DataName = value;
                DrawingName.ImageSource = _DataName.GetImageSource(PersonaEditorLib.Utilities.Utilities.CreatePallete(BackImage.ColorName, _DataName.PixelFormat));
                DrawingName.Rect = GetSize(NameStart, _DataName.PixelWidth, _DataName.PixelHeight);
            }
        }

        public ImageDrawing DrawingText { get; private set; } = new ImageDrawing();
        public ImageDrawing DrawingName { get; private set; } = new ImageDrawing();

        Rect GetSize(Point start, double pixelWidth, double pixelHeight)
        {
            double Height = pixelHeight * GlyphScale;
            double Width = pixelWidth * GlyphScale * 0.9375;
            return new Rect(start, new Size(Width, Height));
        }
    }

    class Visual
    {
        public void Image_Unloaded(object sender, RoutedEventArgs e)
        {
            (((sender as Image).Source as DrawingImage).Drawing as DrawingGroup).Children.Clear();
            (sender as Image).Unloaded -= Image_Unloaded;
            BackImage.PropertyChanged -= BackgroundImageChanged;
            Current.Default.PropertyChanged -= SettingChanged;

            if (Text != null)
            {
                Text.PropertyChanged -= Text_PropertyChanged;
                Text = null;
            }
            if (Name != null)
            {
                Name.Old -= Name_Changed;
                Name.New -= Name_Changed;
                Name = null;
            }

            DrawingText = null;
            DrawingName = null;
            BackImage = null;
            CharLST = null;
        }

        //    public class AsyncTask : INotifyPropertyChanged
        //    {
        //        public AsyncTask(Func<object> valueFunc)
        //        {
        //            AsyncValue = "loading async value"; //temp value for demo
        //            LoadValue(valueFunc);
        //        }

        //        private async Task LoadValue(Func<object> valueFunc)
        //        {
        //            AsyncValue = await Task<object>.Run(() =>
        //            {
        //                return valueFunc();
        //            });
        //            if (PropertyChanged != null)
        //                PropertyChanged(this, new PropertyChangedEventArgs("AsyncValue"));
        //        }

        //        public event PropertyChangedEventHandler PropertyChanged;

        //        public object AsyncValue { get; set; }
        //    }

        //    private void At_PropertyChanged(object sender, PropertyChangedEventArgs e)
        //    {
        //        ImageData = (ImageData)((AsyncTask)sender).AsyncValue;
        //    }

        //public static ImageData DrawText(IList<PTP.MSG.MSGstr.MSGstrElement> text, CharList CharList)
        //{
        //    if (text != null)
        //    {
        //        ImageData returned = null;
        //        ImageData line = null;
        //        foreach (var a in text)
        //        {
        //            if (a.Type == "System")
        //            {
        //                if (Utilities.ByteArrayCompareWithSimplest(a.Array.ToArray(), new byte[] { 0x0A }))
        //                {
        //                    if (returned == null)
        //                    {
        //                        if (line == null)
        //                        {
        //                            returned = new ImageData(PixelFormats.Indexed4, 1, 32);
        //                        }
        //                        else
        //                        {
        //                            returned = line;
        //                            line = null;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (line == null)
        //                        {
        //                            returned = ImageData.MergeUpDown(returned, new ImageData(PixelFormats.Indexed4, 1, 32), 7);
        //                        }
        //                        else
        //                        {
        //                            returned = ImageData.MergeUpDown(returned, line, 7);
        //                            line = null;
        //                        }
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                for (int i = 0; i < a.Array.Length; i++)
        //                {
        //                    CharList.FnMpData fnmp;
        //                    if (0x20 <= a.Array[i] & a.Array[i] < 0x80)
        //                    {
        //                        fnmp = CharList.List.FirstOrDefault(x => x.Index == a.Array[i]);
        //                    }
        //                    else if (0x80 <= a.Array[i] & a.Array[i] < 0xF0)
        //                    {
        //                        int newindex = (a.Array[i] - 0x81) * 0x80 + a.Array[i + 1] + 0x20;

        //                        i++;
        //                        fnmp = CharList.List.FirstOrDefault(x => x.Index == newindex);
        //                    }
        //                    else
        //                    {
        //                        Console.WriteLine("ASD");
        //                        fnmp = null;
        //                    }

        //                    if (fnmp != null)
        //                    {
        //                        byte shift;
        //                        bool shiftb = Static.FontMap.Shift.TryGetValue(fnmp.Index, out shift);
        //                        ImageData glyph = new ImageData(fnmp.Image_data, CharList.PixelFormat, CharList.Width, CharList.Height);
        //                        glyph = shiftb == false ? ImageData.Crop(glyph, new ImageData.Rect(fnmp.Cut.Left, 0, fnmp.Cut.Right - fnmp.Cut.Left - 1, glyph.PixelHeight))
        //                            : ImageData.Shift(ImageData.Crop(glyph, new ImageData.Rect(fnmp.Cut.Left, 0, fnmp.Cut.Right - fnmp.Cut.Left - 1, glyph.PixelHeight)), shift);
        //                        line = ImageData.MergeLeftRight(line, glyph);
        //                    }
        //                }
        //            }
        //        }
        //        returned = ImageData.MergeUpDown(returned, line, 7);
        //        return returned;
        //    }
        //    return null;
        //}

        public enum Type
        {
            Text,
            Name
        }

        public enum Old
        {
            Old,
            New
        }

        public Visual(CharList CharLST, BackgroundImage BackImage, Type type, Old old)
        {
            this.old = old;
            this.CharLST = CharLST;
            this.BackImage = BackImage;
            this.type = type;
            BackImage.PropertyChanged += BackgroundImageChanged;
            Current.Default.PropertyChanged += SettingChanged;
        }

        private void SettingChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ViewVisualizer")
                if (Current.Default.ViewVisualizer == Visibility.Visible)
                    CreateImageData();
        }

        private void BackgroundImageChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ColorText")
                DrawingText.ImageSource = _ImageData.GetImageSource(PersonaEditorLib.Utilities.Utilities.CreatePallete(BackImage.ColorText, _ImageData.PixelFormat));
            else if (e.PropertyName == "ColorName")
                DrawingName.ImageSource = _DataName.GetImageSource(PersonaEditorLib.Utilities.Utilities.CreatePallete(BackImage.ColorName, _DataName.PixelFormat));
            else if (e.PropertyName == "GlyphScale")
            {
                UpdateTextSize();
                UpdateNameSize();
            }
            else if (e.PropertyName == "TextStart")
                UpdateTextSize();
            else if (e.PropertyName == "NameStart")
                UpdateNameSize();
            else if (e.PropertyName == "LineSpacing")
                CreateImageData();
        }

        ImageData CreateImageData(IList<TextBaseElement> array)
        {
            if (Current.Default.ViewVisualizer == Visibility.Visible)
            {
                return ImageData.DrawText(array, CharLST, Static.FontMap.Shift, BackImage.LineSpacing);
                //  AsyncTask at = new AsyncTask(() => DrawText(array, CharLST));
                //  at.PropertyChanged += At_PropertyChanged;
            }
            else return new ImageData();
        }

        ImageData CreateImageData(string text)
        {
            if (Current.Default.ViewVisualizer == Visibility.Visible)
                return ImageData.DrawText(text.GetTextBaseList(CharLST), CharLST, Static.FontMap.Shift, BackImage.LineSpacing);
            else return new ImageData();
        }

        ImageData CreateImageData(byte[] Name)
        {
            if (Current.Default.ViewVisualizer == Visibility.Visible)
                return ImageData.DrawText(Name.GetTextBaseList(), CharLST, Static.FontMap.Shift, BackImage.LineSpacing);
            else return new ImageData();
        }

        void CreateImageData()
        {
            if (type == Type.Text & old == Old.Old)
                DataText = CreateImageData(Text.OldString);
            else if (type == Type.Text & old == Old.New)
                DataText = CreateImageData(Text.NewString);
        }

        public void SetText(PTP.MSG.MSGstr Text)
        {
            this.Text = Text;
            Text.PropertyChanged += Text_PropertyChanged;
            CreateImageData();
        }

        public void SetName(PTP.Names Name)
        {
            if (Name != null)
            {
                this.Name = Name;
                if (old == Old.Old)
                {
                    Name.Old += Name_Changed;
                    Name_Changed(Name.OldName.GetTextBaseList());
                }
                else
                {
                    Name.New += Name_Changed;
                    Name_Changed(Name.NewName.GetTextBaseList(CharLST));
                }
            }
        }

        private void Name_Changed(IList<TextBaseElement> array)
        {
            DataName = CreateImageData(array);
        }

        private void Text_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            CreateImageData();
        }

        Type type;
        Old old;

        BackgroundImage BackImage;
        PTP.MSG.MSGstr Text;
        PTP.Names Name;

        ImageData _ImageData;
        ImageData _DataName;
        CharList CharLST;

        ImageData DataText
        {
            get { return _ImageData; }
            set
            {
                _ImageData = value;
                DrawingText.ImageSource = _ImageData.GetImageSource(PersonaEditorLib.Utilities.Utilities.CreatePallete(BackImage.ColorText, _ImageData.PixelFormat));
                UpdateTextSize();
            }
        }
        ImageData DataName
        {
            get { return _DataName; }
            set
            {
                _DataName = value;
                DrawingName.ImageSource = _DataName.GetImageSource(PersonaEditorLib.Utilities.Utilities.CreatePallete(BackImage.ColorName, _DataName.PixelFormat));
                UpdateNameSize();
            }
        }

        public ImageDrawing DrawingText { get; private set; } = new ImageDrawing();
        public ImageDrawing DrawingName { get; private set; } = new ImageDrawing();

        Rect GetSize(Point start, double pixelWidth, double pixelHeight)
        {
            double Height = pixelHeight * BackImage.GlyphScale;
            double Width = pixelWidth * BackImage.GlyphScale * 0.9375;
            return new Rect(start, new Size(Width, Height));
        }

        void UpdateTextSize()
        {
            double Height = DataText.PixelHeight * BackImage.GlyphScale;
            double Width = DataText.PixelWidth * BackImage.GlyphScale * 0.9375;
            DrawingText.Rect = new Rect(BackImage.TextStart, new Size(Width, Height));
        }

        void UpdateNameSize()
        {
            double Height = DataName.PixelHeight * BackImage.GlyphScale;
            double Width = DataName.PixelWidth * BackImage.GlyphScale * 0.9375;
            DrawingName.Rect = new Rect(BackImage.NameStart, new Size(Width, Height));
        }
    }
}