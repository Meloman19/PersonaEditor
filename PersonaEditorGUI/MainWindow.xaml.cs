using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
using PersonaEditorLib.Interfaces;
using System.IO;
using PersonaEditorGUI.Files;
using System.Collections.ObjectModel;
using PersonaEditorLib;

namespace PersonaEditorGUI
{
    static class Static
    {
        public static class Paths
        {
            public static string CurrentFolderEXE = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            public static string DirBackgrounds = Path.Combine(CurrentFolderEXE, "background");
            public static string DirFont = Path.Combine(CurrentFolderEXE, "font");
            public static string FontOld = Path.Combine(DirFont, "FONT_OLD.FNT");
            public static string FontOldMap = Path.Combine(DirFont, "FONT_OLD.TXT");
            public static string FontNew = Path.Combine(DirFont, "FONT_NEW.FNT");
            public static string FontNewMap = Path.Combine(DirFont, "FONT_NEW.TXT");
        }

        public static class FontMap
        {
            public static Dictionary<int, byte> Shift = new Dictionary<int, byte>()
            {
                { 81, 2 },
                { 103, 2 },
                { 106, 2 },
                { 112, 2 },
                { 113, 2 },
                { 121, 2 }
            };
        }
    }

    public partial class MainWindow : Window
    {
        Tools.Visualizer visualizer;
        Tools.VisualizerVM visualizerVM = new Tools.VisualizerVM();

        Tools.SetChar setchar;
        Tools.SetCharVM setcharVM = new Tools.SetCharVM();

        public MainWindow()
        {
            DataContext = new MainWindowVM();
            InitializeComponent();
        }
        
        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as MainWindowVM).OpenFile();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Settings.BackgroundDefault d = new Settings.BackgroundDefault();
            d.EmptyGlyphScale = 5;
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            (new About() { Owner = this }).ShowDialog();
        }

        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            Settings.SetSettings setSettings = new Settings.SetSettings();
            setSettings.ShowDialog();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (visualizer != null)
                visualizer.Close();
            if (setchar != null)
                setchar.Close();

            Settings.BackgroundDefault.Default.Save();
            Settings.WindowSetting.Default.Save();
        }

        private void Tool_Visualizer_Click(object sender, RoutedEventArgs e)
        {
            if (visualizer != null)
                if (visualizer.IsLoaded)
                {
                    visualizer.Activate();
                    return;
                }

            visualizer = new Tools.Visualizer();
            visualizer.DataContext = visualizerVM;
            visualizer.Show();
        }

        private void Tool_CharSet_Click(object sender, RoutedEventArgs e)
        {
            if (setchar != null)
                if (setchar.IsLoaded)
                {
                    setchar.Activate();
                    return;
                }

            setchar = new Tools.SetChar();
            setchar.DataContext = setcharVM;
            setchar.Show();
        }
    }
}