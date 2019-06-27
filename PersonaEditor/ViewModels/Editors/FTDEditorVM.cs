using PersonaEditorLib.Other;
using AuxiliaryLibraries.WPF;
using PersonaEditor.Classes;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace PersonaEditor.ViewModels.Editors
{
    class FTDEditorVM : BindingObject, IEditor
    {
        FTD ftd;

        public ObservableCollection<FTDEntryVM> Entrie { get; } = new ObservableCollection<FTDEntryVM>();

        public ReadOnlyObservableCollection<string> EncodingList { get; }

        private int selectEncodingIndex = 0;
        public int SelectEncodingIndex
        {
            get { return selectEncodingIndex; }
            set
            {
                selectEncodingIndex = value;
                ApplicationSettings.AppSetting.Default.FTDEncoding = Static.EncodingManager.GetPersonaEncodingName(value);
                UpdateEncoding();
            }
        }

        public FontFamily FontFamily { get; } = new FontFamily(System.Drawing.FontFamily.GenericMonospace.Name);

        public FTDEditorVM(FTD ftd)
        {
            EncodingList = Static.EncodingManager.EncodingList;
            selectEncodingIndex = Static.EncodingManager.GetPersonaEncodingIndex(ApplicationSettings.AppSetting.Default.FTDEncoding);
            this.ftd = ftd;
            Init();
        }

        private void Init()
        {
            Entrie.Clear();
            for (int i = 0; i < ftd.Entries.Count; i++)
            {
                if (ftd.Entries[i].Length == 1)
                    Entrie.Add(new FTDSingleVM(ftd, i, 0, Static.EncodingManager.GetPersonaEncoding(selectEncodingIndex)));
                else
                    Entrie.Add(new FTDMultiVM(ftd, i, Static.EncodingManager.GetPersonaEncoding(selectEncodingIndex)));
            }
        }

        private void UpdateEncoding()
        {
            foreach (var a in Entrie)
                a.SetEncoding(Static.EncodingManager.GetPersonaEncoding(SelectEncodingIndex));
        }

        public bool Close()
        {
            return true;
        }


    }
}