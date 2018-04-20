using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PersonaEditorGUI.Classes.Controls
{
    class ImageVisualizer : Image
    {
        #region DependencyProperty

        public static readonly DependencyProperty BackgroundNameProperty =
            DependencyProperty.Register("BackgroundName", typeof(string), typeof(ImageVisualizer), new PropertyMetadata("", BackgroundNamePropertyChanged));

        public static readonly DependencyProperty EncodingNameProperty =
            DependencyProperty.Register("EncodingName", typeof(string), typeof(ImageVisualizer), new PropertyMetadata("", EncodingNamePropertyChanged));

        public static readonly DependencyProperty NameVisualProperty =
            DependencyProperty.Register("NameVisual", typeof(object), typeof(ImageVisualizer), new PropertyMetadata(null, NameVisualPropertyChanged));

        public static readonly DependencyProperty TextVisualProperty =
            DependencyProperty.Register("TextVisual", typeof(object), typeof(ImageVisualizer), new PropertyMetadata(null, TextVisualPropertyChanged));

        private static void BackgroundNamePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImageVisualizer vis = (ImageVisualizer)d;
            vis.BackgroundNameChanged(e.NewValue as string);
        }

        private static void EncodingNamePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImageVisualizer vis = (ImageVisualizer)d;
            vis.EncodingNameChanged(e.NewValue as string);
        }

        private static void NameVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImageVisualizer vis = (ImageVisualizer)d;
            vis.Name_Changed(e.NewValue);
        }

        private static void TextVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImageVisualizer vis = (ImageVisualizer)d;
            vis.Text_Changed(e.NewValue);
        }

        [Bindable(true)]
        public string BackgroundName
        {
            get { return (string)GetValue(BackgroundNameProperty); }
            set { SetValue(BackgroundNameProperty, value); }
        }

        [Bindable(true)]
        public string EncodingName
        {
            get { return (string)GetValue(EncodingNameProperty); }
            set { SetValue(EncodingNameProperty, value); }
        }

        [Bindable(true)]
        public object NameVisual
        {
            get { return GetValue(NameVisualProperty); }
            set { SetValue(NameVisualProperty, value); }
        }

        [Bindable(true)]
        public object TextVisual
        {
            get { return GetValue(TextVisualProperty); }
            set { SetValue(TextVisualProperty, value); }
        }

        #endregion DependencyProperty

        Visual.Background background;
        Visual.TextVisual name = new Visual.TextVisual();
        Visual.TextVisual text = new Visual.TextVisual();

        ImageDrawing textID = new ImageDrawing();
        ImageDrawing nameID = new ImageDrawing();
        ImageDrawing backgroundID = new ImageDrawing();
        RectangleGeometry clipID = new RectangleGeometry();

        public ImageVisualizer()
        {
            name.VisualChanged += Name_VisualChanged;
            text.VisualChanged += Text_VisualChanged;

            DrawingGroup drawingGroup = new DrawingGroup();
            drawingGroup.Children.Add(backgroundID);
            drawingGroup.Children.Add(textID);
            drawingGroup.Children.Add(nameID);
            drawingGroup.ClipGeometry = clipID;
            Source = new DrawingImage(drawingGroup);
        }

        private void Name_Changed(object data)
        {
            if (data is byte[] dataByte)
                name.UpdateText(dataByte);
            else if (data is IList<PersonaEditorLib.FileStructure.Text.TextBaseElement> dataList)
                name.UpdateText(dataList);
        }

        private void Text_Changed(object data)
        {
            if (data is byte[] dataByte)
                text.UpdateText(dataByte);
            else if (data is IList<PersonaEditorLib.FileStructure.Text.TextBaseElement> dataList)
                text.UpdateText(dataList);
        }

        private void Text_VisualChanged(ImageSource imageSource, Rect rect)
        {
            textID.ImageSource = imageSource;
            textID.Rect = rect;
        }

        private void Name_VisualChanged(ImageSource imageSource, Rect rect)
        {
            nameID.ImageSource = imageSource;
            nameID.Rect = rect;
        }

        private void BackgroundNameChanged(string name)
        {
            background = Static.BackManager.GetBackground(name);
            SetBack();
            backgroundID.ImageSource = background.Image;
            backgroundID.Rect = background.Rect;
            clipID.Rect = background.Rect;
        }

        private void EncodingNameChanged(string name)
        {
            this.name.UpdateFont(Static.FontManager.GetPersonaFont(name));
            text.UpdateFont(Static.FontManager.GetPersonaFont(name));
        }

        private void SetBack()
        {
            if (background != null)
            {
                name.Start = background.NameStart;
                name.Color = background.ColorName;
                name.LineSpacing = background.LineSpacing;
                name.GlyphScale = background.GlyphScale;

                text.Start = background.TextStart;
                text.Color = background.ColorText;
                text.LineSpacing = background.LineSpacing;
                text.GlyphScale = background.GlyphScale;
            }
        }
    }
}
