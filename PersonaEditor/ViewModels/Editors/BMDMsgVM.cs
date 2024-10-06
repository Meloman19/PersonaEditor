using System.Collections.ObjectModel;
using System.Linq;
using PersonaEditor.Common;
using PersonaEditorLib.Text;

namespace PersonaEditor.ViewModels.Editors
{
    public sealed class BMDMsgVM : BindingObject
    {
        private readonly BMDMSG _msg;

        public BMDMsgVM(BMDMSG msg, int sourceFont)
        {
            _msg = msg;

            foreach (var a in msg.MsgStrings)
                StringList.Add(new BMDMsgStrVM(a, sourceFont));
        }

        public string Name => _msg.Name;

        public ObservableCollection<BMDMsgStrVM> StringList { get; } = new ObservableCollection<BMDMsgStrVM>();

        public void Changes(bool save, int destFont)
        {
            foreach (var a in StringList)
                a.Changes(save, destFont);

            if (save)
                _msg.MsgStrings = StringList.Select(x => x.Data).ToArray();
        }

        public void Update(int sourceFont)
        {
            foreach (var a in StringList)
                a.Update(sourceFont);
        }
    }
}