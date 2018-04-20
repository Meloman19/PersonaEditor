using Microsoft.Win32;
using PersonaEditorGUI.Classes;
using PersonaEditorLib;
using PersonaEditorLib.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PersonaEditorGUI.Controls
{
    class MultiFileEditVM : BindingObject
    {
        // private FileStream FileStream;

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

        private string _OpenFileName = "";
        public string OpenFileName => _OpenFileName;

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

                //FileStream = new FileStream(path, FileMode.Open, FileAccess.Read);

                //var file = PersonaEditorLib.Utilities.PersonaFile.OpenFile(Path.GetFileName(path),
                //    PersonaEditorLib.Utilities.PersonaFile.GetFileType(Path.GetFileName(path)),
                //    new StreamFile(FileStream, FileStream.Length, 0));

                var file = PersonaEditorLib.Utilities.PersonaFile.OpenFile(Path.GetFileName(path),
                    File.ReadAllBytes(path),
                    PersonaEditorLib.Utilities.PersonaFile.GetFileType(Path.GetFileName(path)));

                if (file.Object != null)
                {
                    Tree.SetRoot(file);
                    _OpenFileName = Path.GetFullPath(path);
                }
            }
        }

        public void SaveFile(string path)
        {
            if (Tab.CloseAll())
            {
                var root = Tree.GetRoot();
                if (root != null)
                    if (root.Object is IPersonaFile pFile)
                        File.WriteAllBytes(path, pFile.Get());
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

        public DragEventHandler Drop => DropItem;
        private void DropItem(object sender, DragEventArgs e)
        {
            string[] temp = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (temp.Length > 0)
                OpenFile(temp[0]);
        }

        private void Tree_SelectedItemDataOpen(UserTreeViewItem sender)
        {
            MainWindowType = "Single";
            Tab.Open(sender);
        }

        private void Tree_SelectedItemData(UserTreeViewItem sender)
        {
            if (sender.PersonaFile.Object is IImage image)
                Tab.SetPreview(image.GetImage());
            else
                Tab.SetPreview(null);

            statusBar = "";
            if (sender.PersonaFile.Object is IFile file)
            {
                int size = file.Size();
                statusBar = "Size: " + String.Format("0x{0:X8}", size) + " (" + size + ")";
                Notify("StatusBar");
            }
        }

        #endregion Events

        public MultiFileEditVM()
        {
            Tree.SelectedItemChanged += Tree_SelectedItemData;
            Tree.SelectedItemOpen += Tree_SelectedItemDataOpen;
        }
    }
}