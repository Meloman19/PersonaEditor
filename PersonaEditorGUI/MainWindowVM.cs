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

namespace PersonaEditorGUI
{
    class MainWindowVM : BindingObject
    {
        Settings.SetSettings setSettings;
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
            if (visualizer != null)
                visualizer.Close();
            if (setSettings != null)
                setSettings.Close();
            if (setchar != null)
                setchar.Close();

            Settings.App.Default.Save();
            Settings.BackgroundDefault.Default.Save();
            Settings.SPREditor.Default.Save();
            Settings.WindowSetting.Default.Save();
        }

        public ICommand clickOpenFile { get; }
        private void OpenFile()
        {
            OpenFileDialog OFD = new OpenFileDialog();
            if (OFD.ShowDialog() == true)
                MultiFile.OpenFile(OFD.FileName);
        }

        public ICommand clickSettingOpen { get; }
        private void SettingOpen()
        {
            if (setSettings != null)
                if (setSettings.IsLoaded)
                {
                    setSettings.Activate();
                    return;
                }

            setSettings = new Settings.SetSettings() { DataContext = new Settings.SetSettingsVM() };
            setSettings.Show();
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
        }

        public MainWindowVM(string startarg = "")
        {
            clickOpenFile = new RelayCommand(OpenFile);
            clickSettingOpen = new RelayCommand(SettingOpen);
            clickVisualizerOpen = new RelayCommand(ToolVisualizerOpen);
            clickSetCharOpen = new RelayCommand(ToolSetCharOpen);
            clickAboutOpen = new RelayCommand(AboutOpen);

            clickTest = new RelayCommand(TestClick);

            if (File.Exists(startarg))
                MultiFile.OpenFile(startarg);
        }
    }
}