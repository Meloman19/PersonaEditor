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
        public TreeViewPEVM Tree { get; } = new TreeViewPEVM();
        public SingleFileEditVM Single { get; } = new SingleFileEditVM();

        public void OpenFile(string path)
        {
            if (Single.Close())
            {
                var file = PersonaEditorLib.Utilities.PersonaFile.OpenFile(Path.GetFileName(path),
                    File.ReadAllBytes(path),
                    PersonaEditorLib.Utilities.PersonaFile.GetFileType(Path.GetFileName(path)));

                if (file.Object != null)
                    Tree.SetRoot(file);
            }
        }

        private object _Preview = null;
        public object Preview => _Preview;

        private Dictionary<string, object> _Properties = null;
        public Dictionary<string, object> Properties => _Properties;

        public MultiFileEditVM()
        {
            Tree.SelectedItemData += Tree_SelectedItemData;
            Tree.SelectedItemDataOpen += Tree_SelectedItemDataOpen;
        }

        public DragEventHandler Drop => DropItem;

        private void DropItem(object sender, DragEventArgs e)
        {
            string[] temp = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (temp.Length > 0)
                OpenFile(temp[0]);
        }

        private void Tree_SelectedItemDataOpen(ObjectFile sender)
        {
            Single.Open(sender);
        }

        private void Tree_SelectedItemData(ObjectFile sender)
        {
            if (sender.Object is IPersonaFile file)
                _Properties = file.GetProperties;
            else
                _Properties = null;

            if (sender.Object is IPreview preview)
                _Preview = preview.Control;
            else
                _Preview = null;

            Notify("Properties");
            Notify("Preview");
        }
    }
}