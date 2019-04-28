using System.Collections.ObjectModel;
using AuxiliaryLibraries.WPF;
using PersonaEditorLib.Text;
using System.Linq;

namespace PersonaEditor.ViewModels.Editors
{
    class BMDMsgVM : BindingObject
    {
        BMDMSG msg;

        public string Name => msg.Name;

        public ObservableCollection<BMDMsgStrVM> StringList { get; } = new ObservableCollection<BMDMsgStrVM>();

        public void Changes(bool save, int destFont)
        {
            foreach (var a in StringList)
                a.Changes(save, destFont);

            if (save)
                msg.MsgStrings = StringList.Select(x => x.data).ToArray();
        }

        public void Update(int sourceFont)
        {
            foreach (var a in StringList)
                a.Update(sourceFont);
        }

        public BMDMsgVM(BMDMSG msg, int sourceFont)
        {
            this.msg = msg;

            foreach (var a in msg.MsgStrings)
                StringList.Add(new BMDMsgStrVM(a, sourceFont));
        }
    }
}