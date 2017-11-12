using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PersonaText.SettingsWindow
{
    public partial class DefaultBackground : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void Notify(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion INotifyPropertyChanged implementation

        #region Propertys

        string _TextXpos = "";
        string _TextYpos = "";
        string _TextColor = "";
        public string TextXpos
        {
            get { return _TextXpos; }
            set
            {
                if (value != _TextXpos)
                    if (ValidInteger(value))
                        _TextXpos = value;

                Notify("TextXpos");
            }
        }
        public string TextYpos
        {
            get { return _TextYpos; }
            set
            {
                if (value != _TextYpos)
                    if (ValidInteger(value))
                        _TextYpos = value;

                Notify("TextYpos");
            }
        }
        public string TextColor
        {
            get { return _TextColor; }
            set
            {
                if (value != _TextColor)
                    if (ValidColor(value))
                        _TextColor = value;

                Notify("TextColor");
            }
        }

        string _NameXpos = "";
        string _NameYpos = "";
        string _NameColor = "";
        public string NameXpos
        {
            get { return _NameXpos; }
            set
            {
                if (value != _NameXpos)
                    if (ValidInteger(value))
                        _NameXpos = value;

                Notify("NameXpos");
            }
        }
        public string NameYpos
        {
            get { return _NameYpos; }
            set
            {
                if (value != _NameYpos)
                    if (ValidInteger(value))
                        _NameYpos = value;

                Notify("NameYpos");
            }
        }
        public string NameColor
        {
            get { return _NameColor; }
            set
            {
                if (value != _NameColor)
                    if (ValidColor(value))
                        _NameColor = value;

                Notify("NameColor");
            }
        }

        string _BackgroundWidth = "";
        string _BackgroundHeight = "";
        string _BackgroundColor = "";
        public string BackgroundWidth
        {
            get { return _BackgroundWidth; }
            set
            {
                if (value != _BackgroundWidth)
                    if (ValidInteger(value))
                        _BackgroundWidth = value;

                Notify("BackgroundWidth");
            }
        }
        public string BackgroundHeight
        {
            get { return _BackgroundHeight; }
            set
            {
                if (value != _BackgroundHeight)
                    if (ValidInteger(value))
                        _BackgroundHeight = value;

                Notify("BackgroundHeight");
            }
        }
        public string BackgroundColor
        {
            get { return _BackgroundColor; }
            set
            {
                if (value != _BackgroundColor)
                    if (ValidColor(value))
                        _BackgroundColor = value;

                Notify("BackgroundColor");
            }
        }

        string _GlyphScale = "";
        string _LineSpacing = "";
        public string GlyphScale
        {
            get { return _GlyphScale; }
            set
            {
                if (value != _GlyphScale)
                    if (ValidDouble(value))
                        _GlyphScale = value;

                Notify("GlyphScale");
            }
        }
        public string LineSpacing
        {
            get { return _LineSpacing; }
            set
            {
                if (value != _LineSpacing)
                    if (ValidInteger(value))
                        _LineSpacing = value;

                Notify("LineSpacing");
            }
        }

        #endregion Propertys

        public DefaultBackground()
        {
            InitializeComponent();
            Init();
        }

        public void Init()
        {
            TextXpos = Convert.ToString(Current.Default.EmptyTextX);
            TextYpos = Convert.ToString(Current.Default.EmptyTextY);
            TextColor = Current.Default.EmptyTextColor.ToString();

            NameXpos = Convert.ToString(Current.Default.EmptyNameX);
            NameYpos = Convert.ToString(Current.Default.EmptyNameY);
            NameColor = Current.Default.EmptyNameColor.ToString();

            BackgroundWidth = Convert.ToString(Current.Default.EmptyWidth);
            BackgroundHeight = Convert.ToString(Current.Default.EmptyHeight);
            BackgroundColor = Current.Default.EmptyBackgroundColor.ToString();

            GlyphScale = Convert.ToString(Current.Default.EmptyGlyphScale);
            LineSpacing = Convert.ToString(Current.Default.EmptyLineSpacing);
        }

        private void TextColor_Pick(object sender, RoutedEventArgs e)
        {
            var a = ColorConverter.ConvertFromString(TextColor);
            PersonaEditorLib.ColorPicker.ColorPickerTool colorPicker = new PersonaEditorLib.ColorPicker.ColorPickerTool((Color)a);
            if (colorPicker.ShowDialog() == true)
                TextColor = colorPicker.Color.ToString();
        }

        private void NameColor_Pick(object sender, RoutedEventArgs e)
        {
            var a = ColorConverter.ConvertFromString(NameColor);
            PersonaEditorLib.ColorPicker.ColorPickerTool colorPicker = new PersonaEditorLib.ColorPicker.ColorPickerTool((Color)a);
            if (colorPicker.ShowDialog() == true)
                NameColor = colorPicker.Color.ToString();
        }

        private void BackgroundColor_Pick(object sender, RoutedEventArgs e)
        {
            var a = ColorConverter.ConvertFromString(BackgroundColor);
            PersonaEditorLib.ColorPicker.ColorPickerTool colorPicker = new PersonaEditorLib.ColorPicker.ColorPickerTool((Color)a);
            if (colorPicker.ShowDialog() == true)
                BackgroundColor = colorPicker.Color.ToString();
        }

        private bool ValidInteger(string num)
        {
            int temp;
            if (int.TryParse(num, out temp))
                return true;

            return false;
        }

        private bool ValidDouble(string num)
        {
            double temp;
            if (double.TryParse(num, out temp))
                return true;

            return false;
        }

        private bool ValidColor(string color)
        {
            try
            {
                Color temp = (Color)ColorConverter.ConvertFromString(color);
                return true;
            }
            catch { return false; }
        }

        public void Save()
        {
            Current.Default.EmptyTextX = Convert.ToInt32(TextXpos);
            Current.Default.EmptyTextY = Convert.ToInt32(TextYpos);
            Current.Default.EmptyTextColor = (Color)ColorConverter.ConvertFromString(TextColor);

            Current.Default.EmptyNameX = Convert.ToInt32(NameXpos);
            Current.Default.EmptyNameY = Convert.ToInt32(NameYpos);
            Current.Default.EmptyNameColor = (Color)ColorConverter.ConvertFromString(NameColor);

            Current.Default.EmptyWidth = Convert.ToInt32(BackgroundWidth);
            Current.Default.EmptyHeight = Convert.ToInt32(BackgroundHeight);
            Current.Default.EmptyBackgroundColor = (Color)ColorConverter.ConvertFromString(BackgroundColor);

            Current.Default.EmptyGlyphScale = Convert.ToDouble(GlyphScale);
            Current.Default.EmptyLineSpacing = Convert.ToInt32(LineSpacing);
        }
    }
}