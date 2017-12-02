using PersonaEditorLib;
using PersonaEditorLib.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonaEditorGUI.Controls
{
    class SingleFileEditVM : BindingObject
    {
        Editors.SPREditor SPREditor { get; } = new Editors.SPREditor();
        Editors.PTPEditor PTPEditor { get; } = new Editors.PTPEditor();
        Editors.HEXEditor HEXEditor { get; } = new Editors.HEXEditor();

        WpfHexaEditor.HexEditor hexEditor { get; } = new WpfHexaEditor.HexEditor();

        private object _SingleFileEditContent = null;
        public object SingleFileEditContent
        {
            get { return _SingleFileEditContent; }
            private set
            {
                _SingleFileEditContent = value;
                Notify("SingleFileEditContent");
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

        public void Open(object data)
        {
            if (data is IPersonaFile pf)
            {
                Name = pf.Name;
                if (pf.Type == FileType.SPR)
                {
                    SPREditor.DataContext = new Editors.SPREditorVM(data as PersonaEditorLib.FileStructure.SPR.SPR);
                    SingleFileEditContent = SPREditor;
                }
                else if (pf.Type == FileType.PTP)
                {
                    PTPEditor.DataContext = new Editors.PTPEditorVM(data as PersonaEditorLib.FileStructure.PTP.PTP);
                    SingleFileEditContent = PTPEditor;
                }
                else
                {
                    HEXEditor.DataContext = (data as IFile).Get(true);
                    SingleFileEditContent = HEXEditor;

                }
            }
        }

        public void Close()
        {
            SingleFileEditContent = null;
            SPREditor.DataContext = null;
            HEXEditor.DataContext = null;
            //  hexEditor.Stream = null;
        }
    }
}