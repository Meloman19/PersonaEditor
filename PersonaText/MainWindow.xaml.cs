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

            ElementSelectBackground.DataContext = OV.BackImage.BackgroundList;

            int index = OV.BackImage.BackgroundList.IndexOf(Current.Default.SelectedBackground);
            ElementSelectBackground.SelectedIndex = index == -1 ? 0 : index;

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

        #region File

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

        private void ExportBMD_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "BMD files|*.BMD",
                OverwritePrompt = true,
                InitialDirectory = Path.GetDirectoryName(Static.Paths.OpenFileName),
                FileName = Path.GetFileNameWithoutExtension(Static.Paths.OpenFileName) + "(NEW).BMD"
            };

            if (sfd.ShowDialog() == true)
            {
                PersonaEditorLib.FileStructure.BMD.BMD BMD = new PersonaEditorLib.FileStructure.BMD.BMD();
                BMD.Open(OV.PTP);
                File.WriteAllBytes(sfd.FileName, BMD.Get(true));
            }
        }

        #endregion File

        #region View

        private void ViewVisualizer_Checked(object sender, RoutedEventArgs e)
        {
            Current.Default.ViewVisualizer = Visibility.Visible;
        }

        private void ViewVisualizer_Unchecked(object sender, RoutedEventArgs e)
        {
            Current.Default.ViewVisualizer = Visibility.Collapsed;
        }

        private void ViewVisualizer_Initialized(object sender, EventArgs e)
        {
            ViewVisualizer.IsChecked = Current.Default.ViewVisualizer == Visibility.Visible ? true : false;
        }

        private void ViewPrefixPostfix_Checked(object sender, RoutedEventArgs e)
        {
            Current.Default.ViewPrefixPostfix = Visibility.Visible;
        }

        private void ViewPrefixPostfix_Unchecked(object sender, RoutedEventArgs e)
        {
            Current.Default.ViewPrefixPostfix = Visibility.Collapsed;
        }

        private void ViewPrefixPostfix_Initialized(object sender, EventArgs e)
        {
            ViewPrefixPostfix.IsChecked = Current.Default.ViewPrefixPostfix == Visibility.Visible ? true : false;
        }

        private void SelectBack_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = (sender as ComboBox).SelectedIndex;
            if (OV.BackImage.SetBackground(index))
                Current.Default.SelectedBackground = OV.BackImage.BackgroundList[index];
        }

        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            SetSettings setSettings = new SetSettings();
            setSettings.Show();
        }

        #endregion View

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
            Visual visual = new Visual(OV.OldCharList, OV.BackImage.CurrentBackground, Visual.Type.Text, Visual.Old.Old);
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
            Visual visual = new Visual(OV.NewCharList, OV.BackImage.CurrentBackground, Visual.Type.Text, Visual.Old.New);
            visual.SetText(MSG);
            visual.SetName(OV.PTP.names.FirstOrDefault(x => x.Index == MSG.CharacterIndex));
            img.Unloaded += visual.Image_Unloaded;

            ((img.Source as DrawingImage).Drawing as DrawingGroup).Children.Add(visual.DrawingText);
            ((img.Source as DrawingImage).Drawing as DrawingGroup).Children.Add(visual.DrawingName);
        }

        private void Image_Initialized(object sender, EventArgs e)
        {
            DrawingGroup DG = new DrawingGroup();
            DG.Children.Add(OV.BackImage.CurrentBackground.Drawing);
            DG.ClipGeometry = OV.BackImage.CurrentBackground.Rect;

            (sender as Image).Source = new DrawingImage(DG);
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

        private void ToolVis_Click(object sender, RoutedEventArgs e)
        {
            Visualizer visualizer = new Visualizer(OV);
            visualizer.ShowDialog();
        }
    }

    public class BytesToString : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            byte[] array = (byte[])values[0];
            CharList charlist = (CharList)values[1];
            return array.GetTextBaseList().GetString(charlist, true);
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
            IList<TextBaseElement> array = values[0] as IList<TextBaseElement>;
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

            IList<TextBaseElement> list = (IList<TextBaseElement>)value;
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
}