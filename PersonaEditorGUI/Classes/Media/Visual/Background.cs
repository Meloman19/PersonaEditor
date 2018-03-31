using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace PersonaEditorGUI.Classes.Media.Visual
{
    public class BackgroundImage : PersonaEditorLib.BindingObject
    {
        double _glyphScale;
        BitmapSource _Image;
        Color _ColorText;
        Color _ColorName;
        Point _TextStart;
        Point _NameStart;
        int _LineSpacing;

        public Point TextStart
        {
            get { return _TextStart; }
            set
            {
                if (value != _TextStart)
                {
                    _TextStart = value;
                    Notify("TextStart");
                }
            }
        }
        public Point NameStart
        {
            get { return _NameStart; }
            set
            {
                if (value != _NameStart)
                {
                    _NameStart = value;
                    Notify("NameStart");
                }
            }
        }

        public double GlyphScale
        {
            get { return _glyphScale; }
            set
            {
                if (value != _glyphScale)
                {
                    _glyphScale = value;
                    Notify("GlyphScale");
                }
            }
        }
        public BitmapSource Image
        {
            get { return _Image; }
            set
            {
                if (value != _Image)
                {
                    _Image = value;
                    Rect = new Rect(0, 0, _Image.Width, _Image.Height);
                    Notify("Image");
                    Notify("Rect");
                    Notify("Drawing");

                }
            }
        }
        public Color ColorText
        {
            get { return _ColorText; }
            set
            {
                if (_ColorText != value)
                {
                    _ColorText = value;
                    Notify("ColorText");
                }
            }
        }
        public Color ColorName
        {
            get { return _ColorName; }
            set
            {
                if (_ColorName != value)
                {
                    _ColorName = value;
                    Notify("ColorName");
                }
            }
        }
        public int LineSpacing
        {
            get { return _LineSpacing; }
            set
            {
                if (_LineSpacing != value)
                {
                    _LineSpacing = value;
                    Notify("LineSpacing");
                }
            }
        }

        public Rect Rect { get; private set; } = new Rect();
    }

    public class TextBackground : PersonaEditorLib.BindingObject
    {

    }

    public class Backgrounds : PersonaEditorLib.BindingObject
    {
        PersonaEditorLib.EventWrapper SettingEW;

        public string BackgroundDirPath { get; set; } = "";

        public delegate void BackgroundImageChanged(BackgroundImage background);

        public event BackgroundImageChanged BackgroundChanged;

        public BackgroundImage CurrentBackground { get; } = new BackgroundImage();

        ObservableCollection<string> backgroundList { get; } = new ObservableCollection<string>() { "Default" };

        private ReadOnlyObservableCollection<string> _BackgroundList;
        public ReadOnlyObservableCollection<string> BackgroundList => _BackgroundList;

        public Backgrounds(string dir = "")
        {
            _BackgroundList = new ReadOnlyObservableCollection<string>(backgroundList);
            SettingEW = new PersonaEditorLib.EventWrapper(Settings.BackgroundDefault.Default, this);
            SetDefault();
            BackgroundDirPath = dir;
            GetBackgroundList();
        }

        public override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_SelectedIndex == 0)
            {
                if (e.PropertyName == "EmptyTextPos")
                    CurrentBackground.TextStart = Settings.BackgroundDefault.Default.EmptyTextPos;
                else if (e.PropertyName == "EmptyNamePos")
                    CurrentBackground.NameStart = Settings.BackgroundDefault.Default.EmptyNamePos;
                else if (e.PropertyName == "EmptyGlyphScale")
                    CurrentBackground.GlyphScale = Settings.BackgroundDefault.Default.EmptyGlyphScale;
                else if (e.PropertyName == "EmptyLineSpacing")
                    CurrentBackground.LineSpacing = Settings.BackgroundDefault.Default.EmptyLineSpacing;
                else if (e.PropertyName == "EmptyNameColor")
                    CurrentBackground.ColorName = Settings.BackgroundDefault.Default.EmptyNameColor;
                else if (e.PropertyName == "EmptyTextColor")
                    CurrentBackground.ColorText = Settings.BackgroundDefault.Default.EmptyTextColor;
                else if (e.PropertyName == "EmptyBackgroundColor" | e.PropertyName == "EmptyWidth" | e.PropertyName == "EmptyHeight")
                {
                    int Width = Settings.BackgroundDefault.Default.EmptyWidth;
                    int Height = Settings.BackgroundDefault.Default.EmptyHeight;
                    CurrentBackground.Image = BitmapSource.Create(Width, Height, 96, 96, PixelFormats.Indexed1,
                        new BitmapPalette(new List<Color> { Settings.BackgroundDefault.Default.EmptyBackgroundColor }), new byte[Width * Height], Width);
                }
            }
        }

        private void GetBackgroundList()
        {
            if (Directory.Exists(BackgroundDirPath))
            {
                DirectoryInfo DI = new DirectoryInfo(BackgroundDirPath);
                foreach (var file in DI.GetFiles(@"*.png"))
                    backgroundList.Add(file.Name);
            }
        }

        private int _SelectedIndex = 0;
        public int SelectedIndex
        {
            get { return _SelectedIndex; }
            set { SetBackground(value); }
        }

        private string _SelectedItem = "Default";
        public string SelectedItem
        {
            get { return _SelectedItem; }
            set { SetBackground(value); }
        }

        public bool SetBackground(int index)
        {
            if (index >= 0 && index < BackgroundList.Count)
                if (Update(BackgroundList[index]))
                {
                    _SelectedIndex = index;
                    _SelectedItem = BackgroundList[index];
                    Notify("SelectedIndex");
                    Notify("SelectedItem");
                    return true;
                }

            Notify("SelectedIndex");
            return false;
        }

        public bool SetBackground(string name)
        {
            if (BackgroundList.Contains(name))
                if (Update(name))
                {
                    _SelectedItem = name;
                    _SelectedIndex = BackgroundList.IndexOf(name);
                    Notify("SelectedItem");
                    Notify("SelectedIndex");
                    return true;
                }

            Notify("SelectedItem");
            return false;
        }

        bool Update(string FileName)
        {
            if (Equals(FileName, "Default"))
            {
                SetDefault();
                BackgroundChanged?.Invoke(CurrentBackground);
                return true;
            }
            else
            {
                try
                {
                    CurrentBackground.Image = new BitmapImage(new Uri(Path.Combine(Path.GetFullPath(BackgroundDirPath), FileName)));
                    string xml = Path.Combine(Path.GetFullPath(BackgroundDirPath), Path.GetFileNameWithoutExtension(FileName) + ".xml");
                    ParseDescription(xml);
                    BackgroundChanged?.Invoke(CurrentBackground);
                    return true;
                }
                catch (FormatException)
                {
                    MessageBox.Show("Background load error:\nAn error occurred while reading data from the description file.\nCheck that the numeric values(except for GlyphScale) are Integer.");
                }
                catch (FileNotFoundException)
                {
                    MessageBox.Show("Background load error:\nThere is no description file.");
                }
                catch (NullReferenceException)
                {
                    MessageBox.Show("Background load error:\nAn error occurred while reading data from the description file.\nCheck that all the required values are present.");
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.GetType().ToString());
                    MessageBox.Show(e.ToString());
                }
                SetDefault();
                return false;
            }
        }

        void SetDefault()
        {
            int Width = Settings.BackgroundDefault.Default.EmptyWidth;
            int Height = Settings.BackgroundDefault.Default.EmptyHeight;
            CurrentBackground.TextStart = Settings.BackgroundDefault.Default.EmptyTextPos;
            CurrentBackground.NameStart = Settings.BackgroundDefault.Default.EmptyNamePos;

            CurrentBackground.GlyphScale = Settings.BackgroundDefault.Default.EmptyGlyphScale;
            CurrentBackground.ColorName = Settings.BackgroundDefault.Default.EmptyNameColor;
            CurrentBackground.ColorText = Settings.BackgroundDefault.Default.EmptyTextColor;
            CurrentBackground.LineSpacing = Settings.BackgroundDefault.Default.EmptyLineSpacing;

            CurrentBackground.Image = BitmapSource.Create(Width, Height, 96, 96, PixelFormats.Indexed1,
                new BitmapPalette(new List<Color> { Settings.BackgroundDefault.Default.EmptyBackgroundColor }), new byte[Width * Height], Width);
        }

        void ParseDescription(string FileName)
        {
            var culture = CultureInfo.CurrentCulture.Clone() as CultureInfo;
            culture.NumberFormat.NumberDecimalSeparator = ".";
            XDocument XDoc = XDocument.Load(FileName);
            XElement Background = XDoc.Element("Background");

            CurrentBackground.LineSpacing = Convert.ToInt32(Background.Element("LineSpacing").Value, culture);
            CurrentBackground.GlyphScale = Convert.ToDouble(Background.Element("glyphScale").Value, culture);

            CurrentBackground.TextStart = new Point(Convert.ToInt32(Background.Element("textStartX").Value, culture), Convert.ToInt32(Background.Element("textStartY").Value, culture));
            CurrentBackground.NameStart = new Point(Convert.ToInt32(Background.Element("nameStartX").Value, culture), Convert.ToInt32(Background.Element("nameStartY").Value, culture));

            CurrentBackground.ColorText = (Color)ColorConverter.ConvertFromString(Background.Element("ColorText").Value);
            CurrentBackground.ColorName = (Color)ColorConverter.ConvertFromString(Background.Element("ColorName").Value);
        }
    }
}