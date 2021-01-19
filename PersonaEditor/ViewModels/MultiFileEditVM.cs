using PersonaEditorLib;
using AuxiliaryLibraries.WPF;
using PersonaEditor.Classes;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PersonaEditor.ViewModels
{
    class MultiFileEditVM : BindingObject
    {
        public TreeViewPEVM Tree { get; } = new TreeViewPEVM();
        public PreviewEditorTabControlVM Tab { get; } = new PreviewEditorTabControlVM();

        private string mainWindowType = "";
        public string MainWindowType
        {
            get { return mainWindowType; }
            set
            {
                if (mainWindowType != value)
                {
                    mainWindowType = value;
                    Notify("MainWindowType");
                }
            }
        }

        public int ProgressValue
        {
            get
            {
                return BackgroundWorker.ProgressValue;
            }
            set
            {
                BackgroundWorker.ProgressValue = value;
                Notify("ProgressValue");
            }
            
        }

        public int ProgressMaximum
        {
            get
            {
                return BackgroundWorker.ProgressMaximum;
            }
            set
            {
                BackgroundWorker.ProgressMaximum = value;
                Notify("ProgressMaximum");
            }


        }

        public string OpenFileName => Static.OpenedFile;


        public string StatusBar
        {
            get
            {

                return BackgroundWorker.Status;
            }
            set
            {
                BackgroundWorker.Status = value;
                Notify("StatusBar");
            }
        }

        #region Methods

        public async void OpFile(string path)
        {
            await Task.Run(() =>
            {
                BackgroundWorker.Control.Dispatcher.Invoke(() =>
                {
                    if (Tab.CloseAll())
                    {

                        FileInfo fileInfo = new FileInfo(path);
                        if (fileInfo.Length > 1000000000)
                            return;

                        //FileStream = new FileStream(path, FileMode.Open, FileAccess.Read);

                        //var file = PersonaEditorLib.Utilities.PersonaFile.OpenFile(Path.GetFileName(path),
                        //    PersonaEditorLib.Utilities.PersonaFile.GetFileType(Path.GetFileName(path)),
                        //    new StreamFile(FileStream, FileStream.Length, 0));

                        var file = GameFormatHelper.OpenFile(Path.GetFileName(path), File.ReadAllBytes(path));

                        if (file != null)
                        {
                            Tree.SetRoot(file);
                            Static.OpenedFile = Path.GetFullPath(path);
                        }
                    }
                });
            });
        }

        public void OpenFile(string path)
        {
           OpFile(path);
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

        public ICommand Drop { get; }
        private void DropItem(object arg)
        {
            string[] temp = (arg as DragEventArgs).Data.GetData(DataFormats.FileDrop) as string[];
            if (temp.Length > 0)
                OpenFile(temp[0]);
        }

        private void Tree_ItemSelected(TreeViewItemVM sender)
        {
            Tab.SetPreview(sender.BitmapSource);

            BackgroundWorker.Status = "";
            int size = sender.PersonaFile.GameData.GetSize();
            BackgroundWorker.Status = "Size: " + String.Format("0x{0:X8}", size) + " (" + size + ")";
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

        public MultiFileEditVM(UserControl control)
        {
            BackgroundWorker.Control = control;
            Drop = new RelayCommand(DropItem);
            Tree.ItemAction += Tree_ItemAction;
        }

        private void Tree_ItemAction(TreeViewItemVM sender, UserTreeViewItemEventEnum action)
        {
            if (action == UserTreeViewItemEventEnum.Selected)
                Tree_ItemSelected(sender);
            else if (action == UserTreeViewItemEventEnum.Open)
                Tree_ItemOpen(sender);
        }
    }
}