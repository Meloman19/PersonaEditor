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
    class SingleFileEditVM : BindingObject
    {
        private object _SingleFileDataContext = null;
        public object DataContext
        {
            get { return _SingleFileDataContext; }
            private set
            {
                _SingleFileDataContext = value;
                Notify("DataContext");
            }
        }

        private string _Name = "";
        public string Name
        {
            get { return _Name; }
            set
            {
                if (_Name != value)
                {
                    _Name = value;
                    Notify("Name");
                }
            }
        }

        private string _dataContextType = "";
        public string DataContextType => _dataContextType;

        public void Open(ObjectFile data)
        {
            if (data.Object is IPersonaFile pf)
            {
                Name = data.Name;
                if (pf.Type == FileType.SPR)
                {
                    DataContext = new Editors.SPREditorVM(data.Object as PersonaEditorLib.FileStructure.SPR.SPR);
                    _dataContextType = "SPR";
                }
                else if (pf.Type == FileType.PTP)
                {
                    DataContext = new Editors.PTPEditorVM(data.Object as PersonaEditorLib.FileStructure.Text.PTP);
                    _dataContextType = "PTP";
                }
                else if (pf.Type == FileType.BMD)
                {
                    DataContext = new Editors.BMDEditorVM(data);
                    _dataContextType = "BMD";
                }
                else if (pf.Type == FileType.FNT)
                {
                    DataContext = new Editors.FNTEditorVM(data.Object as PersonaEditorLib.FileStructure.FNT.FNT);
                    _dataContextType = "FNT";
                }
                else if (pf.Type == FileType.HEX)
                {
                    DataContext = new Editors.HEXEditorVM(data.Object as PersonaEditorLib.FileStructure.HEX);
                    _dataContextType = "HEX";
                }
                else
                {
                    DataContext = null;
                    _dataContextType = "";
                }

            }
            else
                _dataContextType = "";

            Notify("DataContextType");
        }

        public DragEventHandler Drop => SingleFileEdit_Drop;
        private void SingleFileEdit_Drop(object sender, DragEventArgs e)
        {
            var data = e.Data.GetData(typeof(ObjectFile));
            if (data is ObjectFile objF)
                Open(objF);
        }

        public bool Close()
        {
            if (DataContext is IViewModel mod)
            {
                if (mod.Close())
                {
                    DataContext = null;
                    _dataContextType = "";
                    Notify("DataContextType");
                    return true;
                }
                return false;
            }

            return true;
        }
    }
}