using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PersonaEditorLib;
using Microsoft.Win32;
using PersonaEditorGUI.Files;
using System.Collections.ObjectModel;
using System.IO;

namespace PersonaEditorGUI
{
    class MainWindowVM : BindingObject
    {


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

        public void OpenFile()
        {
            OpenFileDialog OFD = new OpenFileDialog();
            if (OFD.ShowDialog() == true)
            {
                var file = PersonaEditorLib.Utilities.PersonaFile.OpenFile(Path.GetFileName(OFD.FileName),
                    File.ReadAllBytes(OFD.FileName),
                    PersonaEditorLib.Utilities.PersonaFile.GetFileType(Path.GetFileName(OFD.FileName)));
                MainControlDC = null;

                if (file != null)
                {
                    var temp = new UserTreeViewItem(file);
                    MainControlDC = new ObservableCollection<UserTreeViewItem>() { temp };
                }
            }
        }
    }
}