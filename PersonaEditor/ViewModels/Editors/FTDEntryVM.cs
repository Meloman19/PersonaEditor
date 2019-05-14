using PersonaEditorLib.Other;
using AuxiliaryLibraries.WPF;
using System.Text;

namespace PersonaEditor.ViewModels.Editors
{
    abstract class FTDEntryVM : BindingObject
    {
        protected FTD ftd;
        protected int index;
        protected Encoding encoding;

        public FTDEntryVM(FTD ftd, int index, Encoding encoding)
        {
            this.ftd = ftd;
            this.index = index;
            this.encoding = encoding;
        }

        public void SetEncoding(Encoding encoding)
        {
            this.encoding = encoding;
            OnSetEncoding();
        }

        public abstract void OnSetEncoding();
    }
}