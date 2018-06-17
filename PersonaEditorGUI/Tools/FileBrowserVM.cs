using PersonaEditorLib;
using PersonaEditorGUI.Classes.Delegates;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Input;
using PersonaEditorLib.Interfaces;

namespace PersonaEditorGUI.Tools
{
    class FileBrowserGridLine : BindingObject
    {
        public bool IsFile { get; }
        public string FullPath { get; }
        public string Name { get; }

        public string Type { get; } = "";

        public FileBrowserGridLine(DirectoryInfo directoryInfo, bool IsBack = false)
        {
            if (IsBack)
            {
                Name = "...";
                if (directoryInfo == null)
                    FullPath = "";
                else
                    FullPath = directoryInfo.FullName;
            }
            else
            {
                Name = directoryInfo.Name;
                FullPath = directoryInfo.FullName;
            }

            IsFile = false;
        }

        public FileBrowserGridLine(FileInfo fileInfo)
        {
            Name = fileInfo.Name;
            FullPath = fileInfo.FullName;
            IsFile = true;

            if (PersonaEditorLib.Utilities.PersonaFile.PersonaFileType.ContainsKey(fileInfo.Extension.ToLower()))
            {
                FileType fileType = PersonaEditorLib.Utilities.PersonaFile.PersonaFileType[fileInfo.Extension.ToLower()];

                if (fileType == FileType.DDS | fileType == FileType.TMX | fileType == FileType.FNT)
                    Type = "Graphic";
                else if (fileType == FileType.BMD | fileType == FileType.PTP)
                    Type = "Text";
            }
        }
    }

    class FileBrowserDrive : BindingObject
    {
        public event OpenFilePathEventHandler OpenDrive;

        public string RootDirectory { get; }
        public string Name { get; }

        public ICommand SelectDrive { get; }
        private void selectDrive()
        {
            OpenDrive?.Invoke(RootDirectory);
        }

        public FileBrowserDrive(DriveInfo drive)
        {
            SelectDrive = new RelayCommand(selectDrive);

            RootDirectory = drive.RootDirectory.FullName;
            Name = drive.Name;
        }
    }

    class FileBrowserVM : BindingObject
    {
        public event OpenFilePathEventHandler OpenFile;

        public KeyEventHandler PressEnter => pressEnter;
        private void pressEnter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                mouseDoubleClick(FileBrowserGridLines[_SelectedIndex]);
        }

        private int _SelectedIndex = 0;
        public int SelectedIndex
        {
            get { return _SelectedIndex; }
            set
            {
                _SelectedIndex = value;
                Notify("SelectedIndex");
            }
        }

        ObservableCollection<FileBrowserGridLine> fileBrowserGridLines = new ObservableCollection<FileBrowserGridLine>();
        public ReadOnlyObservableCollection<FileBrowserGridLine> FileBrowserGridLines { get; }

        ObservableCollection<FileBrowserDrive> driveBrowser = new ObservableCollection<FileBrowserDrive>();
        public ReadOnlyObservableCollection<FileBrowserDrive> DriveBrowser { get; }

        public void mouseDoubleClick(FileBrowserGridLine selected)
        {
            if (selected.IsFile)
                OpenFile?.Invoke(selected.FullPath);
            else
                OpenDirectory(selected.FullPath);
        }


        public FileBrowserVM()
        {
            FileBrowserGridLines = new ReadOnlyObservableCollection<FileBrowserGridLine>(fileBrowserGridLines);
            DriveBrowser = new ReadOnlyObservableCollection<FileBrowserDrive>(driveBrowser);

            Init();
        }

        private void OpenDirectory(string path)
        {
            if (path == "")
                return;

            fileBrowserGridLines.Clear();

            DirectoryInfo directoryInfo = new DirectoryInfo(path);

            fileBrowserGridLines.Add(new FileBrowserGridLine(directoryInfo.Parent, true));

            foreach (var a in directoryInfo.EnumerateDirectories())
            {
                fileBrowserGridLines.Add(new FileBrowserGridLine(a));
            }

            foreach (var a in directoryInfo.EnumerateFiles())
            {
                fileBrowserGridLines.Add(new FileBrowserGridLine(a));
            }

            //SelectedIndex = 0;            
        }

        private void Init()
        {
            var Drives = DriveInfo.GetDrives();

            foreach (var dr in Drives)
            {
                var drive = new FileBrowserDrive(dr);
                drive.OpenDrive += OpenDirectory;
                driveBrowser.Add(drive);
            }
        }
    }
}