using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PersonaEditorLib;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using PersonaEditorGUI.Classes;
using PersonaEditorGUI.Controls;
using System.Windows.Input;
using System.ComponentModel;
using PersonaEditorLib.PersonaEncoding;
using System.Threading;

namespace PersonaEditorGUI
{
    class MainWindowVM : BindingObject
    {
        Tools.Visualizer visualizer;
        Tools.SetChar setchar;

        public MultiFileEditVM MultiFile { get; } = new MultiFileEditVM();

        private object _MainControlDC = null;
        public object MainControlDC
        {
            get { return _MainControlDC; }
            set
            {
                _MainControlDC = value;
                Notify("MainControlDC");
            }
        }

        #region Events

        public CancelEventHandler WindowClosing => Window_Closing;
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (MultiFile.CloseFile())
            {
                if (visualizer != null)
                    visualizer.Close();
                if (setchar != null)
                    setchar.Close();

                Settings.AppSetting.Default.Save();
                Settings.BackgroundDefault.Default.Save();
                Settings.SPREditor.Default.Save();
                Settings.WindowSetting.Default.Save();
            }
            else
                e.Cancel = true;
        }

        public ICommand clickOpenFile { get; }
        private void OpenFile()
        {
            OpenFileDialog OFD = new OpenFileDialog();
            if (OFD.ShowDialog() == true)
                MultiFile.OpenFile(OFD.FileName);
        }

        public ICommand clickSaveAsFile { get; }
        private void SaveAsFile()
        {
            if (MultiFile.OpenFileName != "")
            {
                SaveFileDialog SFD = new SaveFileDialog();
                SFD.OverwritePrompt = true;

                string dirpath = Path.GetDirectoryName(MultiFile.OpenFileName);
                string filename = Path.GetFileName(MultiFile.OpenFileName);
                if (Directory.Exists(dirpath))
                    SFD.InitialDirectory = dirpath;
                SFD.FileName = filename;

                string ext = Path.GetExtension(MultiFile.OpenFileName).Remove(0, 1);
                SFD.Filter = ext.ToUpper() + "|*." + ext;

                if (SFD.ShowDialog() == true)
                    MultiFile.SaveFile(SFD.FileName);
            }

        }

        public ICommand clickSettingOpen { get; }
        private void SettingOpen()
        {
            Settings.AppSetting.Default.Save();
            Settings.BackgroundDefault.Default.Save();
            Settings.SPREditor.Default.Save();
            Settings.WindowSetting.Default.Save();
            Controls.SettingsWindow.SetSettings setSettings = new Controls.SettingsWindow.SetSettings() { DataContext = new Controls.SettingsWindow.SetSettingsVM() };
            setSettings.ShowDialog();
        }

        public ICommand clickVisualizerOpen { get; }
        private void ToolVisualizerOpen()
        {
            if (visualizer != null)
                if (visualizer.IsLoaded)
                {
                    visualizer.Activate();
                    return;
                }

            visualizer = new Tools.Visualizer() { DataContext = new Tools.VisualizerVM() };
            visualizer.Show();
        }

        public ICommand clickSetCharOpen { get; }
        private void ToolSetCharOpen()
        {
            if (setchar != null)
                if (setchar.IsLoaded)
                {
                    setchar.Activate();
                    return;
                }

            setchar = new Tools.SetChar() { DataContext = new Tools.SetCharVM() };
            setchar.Show();
        }

        public ICommand clickAboutOpen { get; }
        private void AboutOpen()
        {
            (new About()).ShowDialog();
        }

        #endregion Events

        public ICommand clickTest { get; }
        private void TestClick()
        {
            //Thread thread = new Thread(delegate ()
            //{
            //    Tools.Visualizer visualizer = new Tools.Visualizer() { DataContext = new Tools.VisualizerVM() };
            //    visualizer.ShowDialog();
            //});
            //thread.ApartmentState = ApartmentState.STA;
            //thread.Start();
        }

        public void OpenFile(string path)
        {
            if (File.Exists(path))
                MultiFile.OpenFile(path);
        }

        public MainWindowVM()
        {
            clickOpenFile = new RelayCommand(OpenFile);
            clickSaveAsFile = new RelayCommand(SaveAsFile);
            clickSettingOpen = new RelayCommand(SettingOpen);
            clickVisualizerOpen = new RelayCommand(ToolVisualizerOpen);
            clickSetCharOpen = new RelayCommand(ToolSetCharOpen);
            clickAboutOpen = new RelayCommand(AboutOpen);

            clickTest = new RelayCommand(TestClick);
        }
    }
}