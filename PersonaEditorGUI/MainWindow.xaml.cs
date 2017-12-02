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

namespace PersonaEditorGUI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.BackgroundDefault.Default.Save();
            Settings.WindowSetting.Default.Save();
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog OFD = new OpenFileDialog();
            if (OFD.ShowDialog() == true)
            {
                var file = PersonaEditorLib.Utilities.PersonaFile.OpenFile(Path.GetFileName(OFD.FileName),
                    File.ReadAllBytes(OFD.FileName),
                    PersonaEditorLib.Utilities.PersonaFile.GetFileType(Path.GetFileName(OFD.FileName)), true);
                MainControl.DataContext = null;

                if (file != null)
                {
                    var temp = new UserTreeViewItem(file);
                    MainControl.DataContext = new ObservableCollection<UserTreeViewItem>() { temp };
                }
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            (new About() { Owner = this }).ShowDialog();
        }
    }
}