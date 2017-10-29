using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
//using System.Windows.Shapes;
using System.ComponentModel;
using PersonaEditorLib.FileStructure;
using System.Globalization;
using System.Xml.Linq;
using PersonaEditorLib.Extension;
using Microsoft.Win32;
using PersonaEditorLib;
using PersonaEditorLib.FileTypes;
using PersonaEditorLib.FileStructure.PTP;

namespace PersonaText
{
    static class Static
    {
        public static class Paths
        {
            public static string CurrentFolderEXE = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            public static string DirBackgrounds = Path.Combine(CurrentFolderEXE, "background");
            public static string DirFont = Path.Combine(CurrentFolderEXE, "font");
            public static string OpenFileName = "";
            public static string FontOld = Path.Combine(DirFont, "FONT_OLD.FNT");
            public static string FontOldMap = Path.Combine(DirFont, "FONT_OLD.TXT");
            public static string FontNew = Path.Combine(DirFont, "FONT_NEW.FNT");
            public static string FontNewMap = Path.Combine(DirFont, "FONT_NEW.TXT");
        }

        public static class FontMap
        {
            public static Dictionary<int, byte> Shift = new Dictionary<int, byte>();
        }
    }

    public class BackgroundImage : INotifyPropertyChanged
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

        double _glyphScale = 1;
        BitmapSource _Image;
        Color _ColorText;
        Color _ColorName;
        Point _TextStart;
        Point _NameStart;
        int _LineSpacing;

        public Point TextStart
        {
            get { return _TextStart; }
            private set
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
            private set
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
            private set
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
            private set
            {
                if (value != _Image)
                {
                    _Image = value;
                    Drawing.ImageSource = _Image;
                    Drawing.Rect = new Rect(0, 0, _Image.Width, _Image.Height);
                    Rect.Rect = Drawing.Rect;
                    Notify("Image");
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

        public ImageDrawing Drawing { get; private set; } = new ImageDrawing();

        public BackgroundImage()
        {
            SetEmpty();
        }

        public void Update(string FileName)
        {
            if (Equals(FileName, "Empty"))
                SetEmpty();
            else
            {
                Image = new BitmapImage(new Uri(FileName));
                string xml = Path.GetDirectoryName(FileName) + "\\" + Path.GetFileNameWithoutExtension(FileName) + ".xml";
                ParseDescription(xml);
            }
        }

        void SetEmpty()
        {
            int Width = Current.Default.EmptyWidth;
            int Height = Current.Default.EmptyHeight;
            TextStart = new Point(Current.Default.EmptyTextX, Current.Default.EmptyTextY);
            NameStart = new Point(Current.Default.EmptyNameX, Current.Default.EmptyNameY);

            GlyphScale = Current.Default.EmptyGlyphScale;
            ColorName = Current.Default.EmptyNameColor;
            ColorText = Current.Default.EmptyTextColor;
            LineSpacing = 0;

            Image = BitmapSource.Create(Width, Height, 96, 96, PixelFormats.Indexed1, new BitmapPalette(new List<Color> { Current.Default.EmptyBackgroundColor }), new byte[Width * Height], Width);
        }

        void ParseDescription(string FileName)
        {
            try
            {
                var culture = CultureInfo.CurrentCulture.Clone() as CultureInfo;
                culture.NumberFormat.NumberDecimalSeparator = ".";
                XDocument XDoc = XDocument.Load(FileName);
                XElement Background = XDoc.Element("Background");

                GlyphScale = Convert.ToDouble(Background.Element("glyphScale").Value, culture);
                TextStart = new Point(Convert.ToInt32(Background.Element("textStartX").Value, culture), Convert.ToInt32(Background.Element("textStartY").Value, culture));
                NameStart = new Point(Convert.ToInt32(Background.Element("nameStartX").Value, culture), Convert.ToInt32(Background.Element("nameStartY").Value, culture));

                ColorName = (Color)ColorConverter.ConvertFromString(Background.Element("ColorName").Value);
                ColorText = (Color)ColorConverter.ConvertFromString(Background.Element("ColorText").Value);
                LineSpacing = Convert.ToInt32(Background.Element("LineSpacing").Value, culture);
            }
            catch (FormatException)
            {
                MessageBox.Show("Background load error:\nAn error occurred while reading data from the description file.\nCheck that the numeric values(except for GlyphScale) are Integer.");
                SetEmpty();
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("Background load error:\nThere is no description file.");
                SetEmpty();
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Background load error:\nAn error occurred while reading data from the description file.\nCheck that all the required values are present.");
                SetEmpty();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.GetType().ToString());
                MessageBox.Show(e.ToString());
                SetEmpty();
            }
        }

        public RectangleGeometry Rect { get; private set; } = new RectangleGeometry();
    }

    public partial class MainWindow : Window
    {
        ObservableVariable OV = new ObservableVariable();

        public MainWindow()
        {
            InitBefore();
            InitializeComponent();
            InitAfter();
        }

        private void InitBefore()
        {
            Static.FontMap.Shift.ReadShift();
            Current.Default.Reload();

            OV.OldCharList.Tag = "old";
            OV.OldCharList.OpenFont(Static.Paths.FontOld);
            OV.OldCharList.OpenFontMap(Static.Paths.FontOldMap);

            OV.NewCharList.Tag = "new";
            OV.NewCharList.OpenFont(Static.Paths.FontNew);
            OV.NewCharList.OpenFontMap(Static.Paths.FontNewMap);
        }

        private void InitAfter()
        {
            IC_NAME.DataContext = OV.PTP.names;
            IC_MSG.DataContext = OV.PTP.msg;
            ScrollViewer.DataContext = OV;

            OpenOldFont.DataContext = OV.OldCharList;
            SetOldFont.DataContext = OV.OldCharList;
            OpenNewFont.DataContext = OV.NewCharList;
            SetNewFont.DataContext = OV.NewCharList;

            Height = SystemParameters.WorkArea.Height;


            if (Directory.Exists((Static.Paths.DirBackgrounds)))
            {
                DirectoryInfo DI = new DirectoryInfo(Static.Paths.DirBackgrounds);
                foreach (var file in DI.GetFiles(@"*.png"))
                {
                    ComboBoxItem cbi = new ComboBoxItem { Content = file.Name };
                    ElementSelectBackground.Items.Add(cbi);
                }
            }
            var a = ElementSelectBackground.Items.Cast<object>().FirstOrDefault(x => (x as ComboBoxItem).Content.ToString() == Current.Default.SelectedBackground);
            if (a != null) ElementSelectBackground.SelectedItem = a;

            Open();
        }

        private void Open()
        {
            Open(true);
        }

        private void Open(bool IsLittleEndian)
        {
            if (Path.GetExtension(Static.Paths.OpenFileName).ToUpper() == ".PTP")
            {
                OV.PTP.Open(Static.Paths.OpenFileName);
            }
            else if (Path.GetExtension(Static.Paths.OpenFileName).ToUpper() == ".BMD")
            {
                PersonaEditorLib.FileStructure.BMD.BMD BMD = new PersonaEditorLib.FileStructure.BMD.BMD();
                BMD.Open(File.OpenRead(Static.Paths.OpenFileName), Static.Paths.OpenFileName, IsLittleEndian);

                OV.PTP.Open(BMD, false);
            }
        }

        private void OpenPTP_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Persona Text Project (*.PTP)|*.PTP"
            };

            if (ofd.ShowDialog() == true)
            {
                Static.Paths.OpenFileName = ofd.FileName;
                Open();
            }
        }

        private void SavePTP_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Persona Text Project (*.PTP)|*.PTP",
                OverwritePrompt = true,
                InitialDirectory = Path.GetDirectoryName(Static.Paths.OpenFileName),
                FileName = Path.GetFileNameWithoutExtension(Static.Paths.OpenFileName)
            };

            if (sfd.ShowDialog() == true)
            {
                OV.PTP.SaveProject(sfd.FileName);
            }
        }

        private void ImportBMD_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "BMD files|*.BMD"
            };

            if (ofd.ShowDialog() == true)
            {
                Static.Paths.OpenFileName = ofd.FileName;
                Open(Current.Default.IsLittleEndian);
            }
        }

        private void ViewVisualizer_Checked(object sender, RoutedEventArgs e)
        {
            Current.Default.ViewVisualizer = true;
        }

        private void ViewVisualizer_Unchecked(object sender, RoutedEventArgs e)
        {
            Current.Default.ViewVisualizer = false;
        }

        private void ViewVisualizer_Initialized(object sender, EventArgs e)
        {
            ViewVisualizer.IsChecked = Current.Default.ViewVisualizer;
        }

        private void ViewPrefixPostfix_Checked(object sender, RoutedEventArgs e)
        {
            Current.Default.ViewPrefixPostfix = true;
        }

        private void ViewPrefixPostfix_Unchecked(object sender, RoutedEventArgs e)
        {
            Current.Default.ViewPrefixPostfix = false;
        }

        private void ViewPrefixPostfix_Initialized(object sender, EventArgs e)
        {
            ViewPrefixPostfix.IsChecked = Current.Default.ViewPrefixPostfix;
        }

        private void SelectBack_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Current.Default.SelectedBackground = ((sender as ComboBox).SelectedItem as ComboBoxItem).Content as string;
            if (Current.Default.SelectedBackground == "Empty")
            { OV.BackImage.Update("Empty"); }
            else
            { OV.BackImage.Update(Path.Combine(Static.Paths.DirBackgrounds, Current.Default.SelectedBackground)); }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Current.Default.Save();
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Logging.Write("D", ex);
            }
        }

        void SetBind(DependencyObject depO, DependencyProperty depP, object obj, string propName)
        {
            Binding bind = new Binding(propName)
            {
                Source = obj
            };

            BindingOperations.SetBinding(depO, depP, bind);
        }

        private void OldVisualizer_Loaded(object sender, RoutedEventArgs e)
        {
            Image img = sender as Image;
            PTP.MSG.MSGstr MSG = img.DataContext as PTP.MSG.MSGstr;
            Visual visual = new Visual(OV.OldCharList, OV.BackImage, Visual.Type.Text, Visual.Old.Old);
            visual.SetText(MSG);
            visual.SetName(OV.PTP.names.FirstOrDefault(x => x.Index == MSG.CharacterIndex));
            img.Unloaded += visual.Image_Unloaded;

            ((img.Source as DrawingImage).Drawing as DrawingGroup).Children.Add(visual.DrawingText);
            ((img.Source as DrawingImage).Drawing as DrawingGroup).Children.Add(visual.DrawingName);
        }

        private void NewVisualizer_Loaded(object sender, RoutedEventArgs e)
        {
            Image img = sender as Image;
            PTP.MSG.MSGstr MSG = img.DataContext as PTP.MSG.MSGstr;
            Visual visual = new Visual(OV.NewCharList, OV.BackImage, Visual.Type.Text, Visual.Old.New);
            visual.SetText(MSG);
            visual.SetName(OV.PTP.names.FirstOrDefault(x => x.Index == MSG.CharacterIndex));
            img.Unloaded += visual.Image_Unloaded;

            ((img.Source as DrawingImage).Drawing as DrawingGroup).Children.Add(visual.DrawingText);
            ((img.Source as DrawingImage).Drawing as DrawingGroup).Children.Add(visual.DrawingName);
        }

        private void Image_Initialized(object sender, EventArgs e)
        {
            DrawingImage DI = new DrawingImage();
            DrawingGroup DG = new DrawingGroup();
            DI.Drawing = DG;
            DG.Children.Add(OV.BackImage.Drawing);
            DG.ClipGeometry = OV.BackImage.Rect;

            (sender as Image).Source = DI;
        }

        private void OpenFont_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog OFD = new OpenFileDialog
            {
                Filter = "Persona Font (*.FNT) | *.FNT",
                Multiselect = false
            };

            if (OFD.ShowDialog() == true)
            {
                CharList current = (sender as MenuItem).DataContext as CharList;

                string Font = OFD.FileName;

                if (MessageBox.Show("Replace current " + current.Tag + " font?", "Raplace font?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                {
                    File.Copy(Font, Path.Combine(Static.Paths.CurrentFolderEXE, "FONT_" + current.Tag.ToUpper() + ".FNT"), true);
                    string FontMap = Path.Combine(Path.GetDirectoryName(Font), Path.GetFileNameWithoutExtension(Font) + ".TXT");
                    if (File.Exists(FontMap))
                        if (MessageBox.Show("Detected font's map: " + Path.GetFileName(FontMap) + "\nReplace current " + current.Tag + " font's map?", "Replace font's map?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                            File.Copy(FontMap, Path.Combine(Static.Paths.CurrentFolderEXE, "FONT_" + current.Tag.ToUpper() + ".TXT"), true);
                    Font = Path.Combine(Static.Paths.CurrentFolderEXE, "FONT_" + current.Tag.ToUpper() + ".FNT");
                    FontMap = Path.Combine(Static.Paths.CurrentFolderEXE, "FONT_" + current.Tag.ToUpper() + ".TXT");
                    current.OpenFont(Font);
                    current.OpenFontMap(FontMap);
                }
            }
        }

        private void SetFont_Click(object sender, RoutedEventArgs e)
        {
            SetChar SetChar = new SetChar();
            SetChar.Open((sender as MenuItem).DataContext as CharList);
            SetChar.ShowDialog();
        }
    }

    public class OldMSGtoTextBox
    {
        public OldMSGtoTextBox(PTP.MSG.MSGstr MSG, TextBox TB, CharList charList)
        {
            this.TB = TB;
            this.charList = charList;
            this.MSG = MSG;
            this.MSG.OldString.ListChanged += OldString_ListChanged;
            TB.Unloaded += TB_Unloaded;
            SetNewText();

        }

        public OldMSGtoTextBox(PTP.Names Name, TextBox TB, CharList charList)
        {
            this.TB = TB;
            this.charList = charList;
            this.Name = Name;
            this.Name.OldNameChanged += Name_OldNameChanged;
        }


        TextBox TB;
        PTP.MSG.MSGstr MSG;
        PTP.Names Name;
        CharList charList;

        private void Name_OldNameChanged(ByteArray array)
        {
            TB.Text = array.GetPTPMsgStrEl().GetString(charList, true);
        }

        private void OldString_ListChanged(object sender, ListChangedEventArgs e)
        {
            SetNewText();
        }

        public void TB_Unloaded(object sender, RoutedEventArgs e)
        {
            TB.Unloaded -= TB_Unloaded;
            if (MSG != null) MSG.OldString.ListChanged -= OldString_ListChanged;
            if (Name != null) Name.OldNameChanged -= Name_OldNameChanged;
            TB = null;
            MSG = null;
            Name = null;
            charList = null;
        }

        void SetNewText()
        {
            string returned = "";

            foreach (var Bytes in MSG.OldString)
                returned += Bytes.GetText(charList);

            TB.Text = returned;
        }


    }

    public class ObservableVariable : INotifyPropertyChanged
    {
        public ObservableVariable()
        {
            PTP = new PTP(OldCharList, NewCharList);
        }

        public CharList OldCharList { get; set; } = new CharList();
        public CharList NewCharList { get; set; } = new CharList();

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

        private bool _OpenFile = false;
        public bool OpenFile
        {
            get { return _OpenFile; }
            set
            {
                if (value != _OpenFile)
                {
                    _OpenFile = value;
                    Notify("OpenFile");
                }
            }
        }

        BackgroundImage _BackImage = new BackgroundImage();
        public BackgroundImage BackImage
        {
            get { return _BackImage; }
            set
            {
                if (value != _BackImage)
                {
                    _BackImage = value;
                    Notify("BackImage");
                }
            }
        }

        public PTP PTP { get; set; }

        private Visibility _ViewVisualizer = Visibility.Visible;
        public Visibility ViewVisualizer
        {
            get { return _ViewVisualizer; }
            set
            {
                if (value != _ViewVisualizer)
                {
                    _ViewVisualizer = value;
                    Notify("ViewVisualizer");
                }
            }
        }

        private Visibility _ViewPrefixPostfix = Visibility.Visible;
        public Visibility ViewPrefixPostfix
        {
            get { return _ViewPrefixPostfix; }
            set
            {
                if (value != _ViewPrefixPostfix)
                {
                    _ViewPrefixPostfix = value;
                    Notify("ViewPrefixPostfix");
                }
            }
        }
    }

    public class BytesToString : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            ByteArray array = (ByteArray)values[0];
            CharList charlist = (CharList)values[1];
            return array.GetPTPMsgStrEl().GetString(charlist, true);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MSGListToText : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            IList<PTP.MSG.MSGstr.MSGstrElement> array = values[0] as IList<PTP.MSG.MSGstr.MSGstrElement>;
            CharList charlist = (CharList)values[1];
            return array.GetString(charlist, true);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MSGListToSystem : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string returned = "";

            IList<PTP.MSG.MSGstr.MSGstrElement> list = (IList<PTP.MSG.MSGstr.MSGstrElement>)value;
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

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
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
                Name.OldNameChanged -= Name_OldNameChanged;
                Name.NewNameChanged -= Name_NewNameChanged;
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
            //  if (e.PropertyName == "ViewVisualizer")
            //      UpdateImageSource();
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

        ImageData CreateImageData(IList<PTP.MSG.MSGstr.MSGstrElement> array)
        {
            if (Current.Default.ViewVisualizer == true)
            {
                return ImageData.DrawText(array, CharLST, Static.FontMap.Shift, BackImage.LineSpacing);
                //  AsyncTask at = new AsyncTask(() => DrawText(array, CharLST));
                //  at.PropertyChanged += At_PropertyChanged;
            }
            else return new ImageData();
        }

        ImageData CreateImageData(string text)
        {
            if (Current.Default.ViewVisualizer == true)
                return ImageData.DrawText(text.GetPTPMsgStrEl(CharLST), CharLST, Static.FontMap.Shift, BackImage.LineSpacing);
            else return new ImageData();
        }

        ImageData CreateImageData(ByteArray Name)
        {
            if (Current.Default.ViewVisualizer == true)
                return ImageData.DrawText(Name.GetPTPMsgStrEl(), CharLST, Static.FontMap.Shift, BackImage.LineSpacing);
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
                    Name.OldNameChanged += Name_OldNameChanged;
                    Name_OldNameChanged(Name.OldName);
                }
                else
                {
                    Name.NewNameChanged += Name_NewNameChanged;
                    Name_NewNameChanged(Name.NewName);
                }
            }
        }

        private void Name_NewNameChanged(string text)
        {
            DataName = CreateImageData(text);
        }

        private void Name_OldNameChanged(ByteArray array)
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

        public void UpdateTextSize()
        {
            double Height = DataText.PixelHeight * BackImage.GlyphScale;
            double Width = DataText.PixelWidth * BackImage.GlyphScale * 0.9375;
            DrawingText.Rect = new Rect(BackImage.TextStart, new Size(Width, Height));
        }

        public void UpdateNameSize()
        {
            double Height = DataName.PixelHeight * BackImage.GlyphScale;
            double Width = DataName.PixelWidth * BackImage.GlyphScale * 0.9375;
            DrawingName.Rect = new Rect(BackImage.NameStart, new Size(Width, Height));
        }
    }
}