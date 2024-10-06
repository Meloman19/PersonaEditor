using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using PersonaEditor.Common;
using PersonaEditor.Common.Managers;
using PersonaEditor.Common.Settings;
using PersonaEditorLib;
using PersonaEditorLib.Text;

namespace PersonaEditor.ViewModels.Editors
{
    public sealed class BMDEditorVM : BindingObject, IEditor
    {
        private readonly SettingsProvider _settingsProvider;

        private bool _isEdit = false;
        private int _sourceFont;
        private int _destFont;
        private string _name = string.Empty;


        public BMDEditorVM(GameFile objbmd)
        {
            _settingsProvider = Static.SettingsProvider;

            if (objbmd.GameData is BMD bmd)
            {
                Static.EncodingManager.PropertyChanged += EncodingManager_PropertyChanged;

                int sourceInd = Static.EncodingManager.GetPersonaEncodingIndex(_settingsProvider.AppSettings.BMDFontDefault);
                if (sourceInd >= 0)
                    _sourceFont = sourceInd;
                else
                    _sourceFont = 0;

                sourceInd = Static.EncodingManager.GetPersonaEncodingIndex(_settingsProvider.AppSettings.BMDFontDestDefault);
                if (sourceInd >= 0)
                    _destFont = sourceInd;
                else
                    _destFont = 0;


                foreach (var a in bmd.Name)
                    NameList.Add(new BMDNameVM(a, _sourceFont));
                foreach (var a in bmd.Msg)
                    MsgList.Add(new BMDMsgVM(a, _sourceFont));

                _name = objbmd.Name;
            }
        }

        public bool IsEdit
        {
            get { return _isEdit; }
            set
            {
                if (value)
                {
                    _isEdit = value;
                    IsSelectCharList = false;
                    Notify(nameof(IsSelectCharList));
                }
                else
                {
                    if (Save())
                    {
                        _isEdit = value;
                        IsSelectCharList = true;
                        Notify(nameof(IsSelectCharList));
                    }
                }

                Notify(nameof(IsEdit));
            }
        }

        public int SelectedSourceFont
        {
            get { return _sourceFont; }
            set
            {
                if (SetProperty(ref _sourceFont, value))
                {
                    _settingsProvider.AppSettings.BMDFontDefault = Static.EncodingManager.GetPersonaEncodingName(value);
                    _ = _settingsProvider.SaveAsync();
                    Update();
                }
            }
        }

        public int SelectedDestFont
        {
            get { return _destFont; }
            set
            {
                if (SetProperty(ref _destFont, value))
                {
                    _settingsProvider.AppSettings.BMDFontDestDefault = Static.EncodingManager.GetPersonaEncodingName(value);
                    _ = _settingsProvider.SaveAsync();
                }
            }
        }

        public ReadOnlyObservableCollection<string> FontList => Static.EncodingManager.EncodingList;

        public bool IsSelectCharList { get; set; } = true;

        public ObservableCollection<BMDNameVM> NameList { get; } = new ObservableCollection<BMDNameVM>();

        public ObservableCollection<BMDMsgVM> MsgList { get; } = new ObservableCollection<BMDMsgVM>();

        private bool Save()
        {
            var result = MessageBox.Show("Save changes?", _name, MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);
            if (result == MessageBoxResult.Yes | result == MessageBoxResult.No)
            {
                bool save = result == MessageBoxResult.Yes ? true : false;

                foreach (var a in NameList)
                    a.Changes(save, _destFont);
                foreach (var a in MsgList)
                    a.Changes(save, _destFont);

                return true;
            }
            else return false;
        }

        private void Update()
        {
            foreach (var a in NameList)
                a.Update(_sourceFont);
            foreach (var a in MsgList)
                a.Update(_sourceFont);
        }

        public bool Close()
        {
            if (IsEdit)
                if (!Save())
                    return false;

            NameList.Clear();
            MsgList.Clear();
            return true;
        }

        private void EncodingManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var man = sender as PersonaEncodingManager;
            if (IsSelectCharList && e.PropertyName == man.GetPersonaEncodingName(_sourceFont))
                Update();
        }

        public override void Release()
        {
            base.Release();

            Static.EncodingManager.PropertyChanged -= EncodingManager_PropertyChanged;
        }
    }
}