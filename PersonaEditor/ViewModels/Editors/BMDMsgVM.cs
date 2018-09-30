using System.Collections.Generic;
using System.Collections.ObjectModel;
using AuxiliaryLibraries.WPF;
using AuxiliaryLibraries.GameFormat.Text;

namespace PersonaEditor.ViewModels.Editors
{
    class BMDMsgVM : BindingObject
    {
        BMD.MSGs msg;

        public string Name => msg.Name;

        public ObservableCollection<BMDMsgStrVM> StringList { get; } = new ObservableCollection<BMDMsgStrVM>();

        public void Changes(bool save, int destFont)
        {
            foreach (var a in StringList)
                a.Changes(save, destFont);

            if (save)
            {
                List<byte> temp = new List<byte>();
                foreach (var a in StringList)
                    temp.AddRange(a.data);

                msg.MsgBytes = temp.ToArray();
            }
        }

        public void Update(int sourceFont)
        {
            foreach (var a in StringList)
                a.Update(sourceFont);
        }

        public BMDMsgVM(BMD.MSGs msg, int sourceFont)
        {
            this.msg = msg;

            var list = msg.MsgBytes.SplitSourceBytes();
            foreach (var a in list)
                StringList.Add(new BMDMsgStrVM(a, sourceFont));
        }
    }
}