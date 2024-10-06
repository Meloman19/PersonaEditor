using System.Collections.ObjectModel;
using System.Windows.Media;
using PersonaEditor.Common;
using PersonaEditor.Common.Settings;
using PersonaEditorLib.Other;

namespace PersonaEditor.ViewModels.Editors
{
    public sealed class FTDEditorVM : BindingObject, IEditor
    {
        private readonly SettingsProvider _settingsProvider;

        private FTD _ftd;
        private int _selectEncodingIndex = 0;

        public FTDEditorVM(FTD ftd)
        {
            _settingsProvider = Static.SettingsProvider;
            EncodingList = Static.EncodingManager.EncodingList;
            _selectEncodingIndex = Static.EncodingManager.GetPersonaEncodingIndex(_settingsProvider.AppSettings.FTDEncoding);
            _ftd = ftd;
            Init();
        }

        public ObservableCollection<FTDEntryVM> Entries { get; } = new ObservableCollection<FTDEntryVM>();

        public ReadOnlyObservableCollection<string> EncodingList { get; }

        public int SelectEncodingIndex
        {
            get { return _selectEncodingIndex; }
            set
            {
                _selectEncodingIndex = value;
                _settingsProvider.AppSettings.FTDEncoding = Static.EncodingManager.GetPersonaEncodingName(value);
                UpdateEncoding();
            }
        }

        public FontFamily FontFamily { get; } = new FontFamily(System.Drawing.FontFamily.GenericMonospace.Name);

        private void Init()
        {
            Entries.Clear();
            for (int i = 0; i < _ftd.Entries.Count; i++)
            {
                if (_ftd.Entries[i].Length == 1)
                    Entries.Add(new FTDSingleVM(_ftd, i, 0, Static.EncodingManager.GetPersonaEncoding(_selectEncodingIndex)));
                else
                    Entries.Add(new FTDMultiVM(_ftd, i, Static.EncodingManager.GetPersonaEncoding(_selectEncodingIndex)));
            }
        }

        private void UpdateEncoding()
        {
            foreach (var a in Entries)
                a.SetEncoding(Static.EncodingManager.GetPersonaEncoding(SelectEncodingIndex));
        }

        public bool Close()
        {
            return true;
        }
    }
}