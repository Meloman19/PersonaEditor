using PersonaEditorLib;
using AuxiliaryLibraries.WPF;
using PersonaEditor.Common;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace PersonaEditor.ViewModels
{
    class MultiFileEditVM : BindingObject
    {
        public MultiFileEditVM()
        {
            DropFileCommand = new RelayCommand(DropFile);
            Tree.ItemAction += Tree_ItemAction;
        }

        public TreeViewPEVM Tree { get; } = new TreeViewPEVM();
        public PreviewEditorTabControlVM Tab { get; } = new PreviewEditorTabControlVM();

        private string _mainWindowType = "";
        public string MainWindowType
        {
            get => _mainWindowType;
            set => SetProperty(ref _mainWindowType, value);
        }

        public string OpenFileName => Static.OpenedFile;

        private string statusBar = "";
        public string StatusBar => statusBar;

        #region Methods

        public void OpenFile(string path)
        {
            if (Tab.CloseAll())
            {
                FileInfo fileInfo = new FileInfo(path);
                if (fileInfo.Length > 1000000000)
                    return;

                var file = GameFormatHelper.OpenFile(Path.GetFileName(path), File.ReadAllBytes(path));

                if (file != null)
                {
                    Tree.SetRoot(file);
                    Static.OpenedFile = Path.GetFullPath(path);
                }
            }
        }

        public void SaveFile(string path)
        {
            if (Tab.CloseAll())
            {
                var root = Tree.GetRoot();
                if (root != null)
                    File.WriteAllBytes(path, root.GameData.GetData());
            }
        }

        public bool CloseFile()
        {
            if (Tab.CloseAll())
                if (OpenFileName != "")
                {
                    MainWindowType = "";
                    return true;
                    var result = MessageBox.Show("Save file?\n" + OpenFileName, Path.GetFileName(OpenFileName), MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);
                    if (result == MessageBoxResult.Yes)
                    {
                        SaveFile(OpenFileName);
                        return true;
                    }
                    else if (result == MessageBoxResult.No)
                        return true;
                }
                else
                    return true;

            return false;
        }

        #endregion Methods

        #region Events

        public ICommand DropFileCommand { get; }

        private void DropFile(object arg)
        {
            if (arg is string filePath)
                OpenFile(filePath);
        }

        private void Tree_ItemSelected(TreeViewItemVM sender)
        {
            Tab.SetPreview(sender.BitmapSource);

            statusBar = "";
            int size = sender.PersonaFile.GameData.GetSize();
            statusBar = "Size: " + String.Format("0x{0:X8}", size) + " (" + size + ")";
            Notify("StatusBar");
        }

        private void Tree_ItemOpen(TreeViewItemVM sender)
        {
            MainWindowType = "Single";
            Tab.Open(sender);
        }

        private void Tree_ItemSaveAs(TreeViewItemVM sender)
        {
        }

        #endregion Events

        private void Tree_ItemAction(TreeViewItemVM sender, UserTreeViewItemEventEnum action)
        {
            if (action == UserTreeViewItemEventEnum.Selected)
                Tree_ItemSelected(sender);
            else if (action == UserTreeViewItemEventEnum.Open)
                Tree_ItemOpen(sender);
        }
    }
}