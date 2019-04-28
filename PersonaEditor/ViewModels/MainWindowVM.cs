﻿using Microsoft.Win32;
using System.IO;
using System.Windows.Input;
using System.ComponentModel;
using AuxiliaryLibraries.WPF;
using PersonaEditor.ViewModels;
using PersonaEditor.Views;
using PersonaEditor.View.Settings;
using PersonaEditor.ViewModels.Settings;
using PersonaEditorLib.Text;

namespace PersonaEditor.Views
{
    class MainWindowVM : BindingObject
    {
        Views.Tools.Visualizer visualizer;
        Views.Tools.SetChar setchar;
        Views.Tools.FileBrowser fileBrowser;

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

        public ICommand WindowClosing { get; }
        private void Window_Closing(object arg)
        {
            if (MultiFile.CloseFile())
            {
                if (visualizer != null)
                    visualizer.Close();
                if (setchar != null)
                    setchar.Close();
                if (fileBrowser != null)
                    fileBrowser.Close();

                ApplicationSettings.AppSetting.Default.Save();
                ApplicationSettings.BackgroundDefault.Default.Save();
                ApplicationSettings.SPREditor.Default.Save();
                ApplicationSettings.WindowSetting.Default.Save();
            }
            else
                (arg as CancelEventArgs).Cancel = true;
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
            ApplicationSettings.AppSetting.Default.Save();
            ApplicationSettings.BackgroundDefault.Default.Save();
            ApplicationSettings.SPREditor.Default.Save();
            ApplicationSettings.WindowSetting.Default.Save();
            SetSettings setSettings = new SetSettings() { DataContext = new SetSettingsVM() };
            setSettings.ShowDialog();
            Static.BackManager.EmptyUpdate();
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

            visualizer = new Views.Tools.Visualizer() { DataContext = new ViewModels.Tools.VisualizerVM() };
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

            setchar = new Views.Tools.SetChar() { DataContext = new ViewModels.Tools.SetCharVM() };
            setchar.Show();
        }

        public ICommand clickFileBrowserOpen { get; }
        private void FileBrowserOpen()
        {
            if (fileBrowser != null)
                if (fileBrowser.IsLoaded)
                {
                    fileBrowser.Activate();
                    return;
                }

            ViewModels.Tools.FileBrowserVM fileBrowserVM = new ViewModels.Tools.FileBrowserVM();
            fileBrowserVM.OpenFile += FileBrowserVM_OpenFile;
            fileBrowser = new Views.Tools.FileBrowser() { DataContext = fileBrowserVM };

            fileBrowser.Show();
        }

        private void FileBrowserVM_OpenFile(string path)
        {
            MultiFile.OpenFile(path);
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
            WindowClosing = new RelayCommand(Window_Closing);
            clickOpenFile = new RelayCommand(OpenFile);
            clickSaveAsFile = new RelayCommand(SaveAsFile);
            clickSettingOpen = new RelayCommand(SettingOpen);
            clickVisualizerOpen = new RelayCommand(ToolVisualizerOpen);
            clickSetCharOpen = new RelayCommand(ToolSetCharOpen);
            clickFileBrowserOpen = new RelayCommand(FileBrowserOpen);

            clickAboutOpen = new RelayCommand(AboutOpen);

            clickTest = new RelayCommand(TestClick);
        }
    }
}