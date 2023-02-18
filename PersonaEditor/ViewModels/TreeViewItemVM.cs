using PersonaEditorLib;
using AuxiliaryLibraries.WPF;
using AuxiliaryLibraries.WPF.Wrapper;
using PersonaEditor.Common;
using PersonaEditor.Common.Delegates;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace PersonaEditor.ViewModels
{
    public partial class TreeViewItemVM : BindingObject
    {
        public event TreeViewItemEventHandler ItemAction;

        #region Private

        private ObservableCollection<object> _contextMenu = new ObservableCollection<object>();
        private ObservableCollection<TreeViewItemVM> _subItems = new ObservableCollection<TreeViewItemVM>();
        private bool _edit;
        private bool _isEnable = true;
        private bool _isSelected = false;
        private GameFile _personaFile = null;

        #endregion Private

        public TreeViewItemVM(GameFile personaFile)
        {
            SubItems = new ReadOnlyObservableCollection<TreeViewItemVM>(_subItems);
            ContextMenu = new ReadOnlyObservableCollection<object>(_contextMenu);

            _personaFile = personaFile ?? throw new ArgumentNullException(nameof(personaFile));
            Update(personaFile);
        }

        #region Property

        public ReadOnlyObservableCollection<TreeViewItemVM> SubItems { get; }

        public ReadOnlyObservableCollection<object> ContextMenu { get; }

        public string Header
        {
            get { return _personaFile.Name; }
            set
            {

            }
        }

        public bool Edit
        {
            get => _edit;
            set => SetProperty(ref _edit, value);
        }

        public bool IsEnable => _isEnable;

        public GameFile PersonaFile => _personaFile;

        public bool IsSelected
        {
            set
            {
                _isSelected = value;
                if (value)
                {
                    ItemAction?.Invoke(this, UserTreeViewItemEventEnum.Selected);
                    Edit = true;
                }
            }
        }

        public BitmapSource BitmapSource { get; private set; } = null;

        #endregion Property

        public void UnEnable()
        {
            _isEnable = false;
            foreach (var a in SubItems)
                a.UnEnable();
            Notify("IsEnable");
        }

        public void Enable()
        {
            _isEnable = true;
            foreach (var a in SubItems)
                a.Enable();
            Notify("IsEnable");
        }

        public bool CanEdit()
        {
            if (!_isEnable)
                return false;

            bool returned = true;
            foreach (var a in SubItems)
                returned &= a.CanEdit();

            return returned;
        }

        public bool Close()
        {
            return true;
        }

        private void Update(GameFile newObject)
        {
            if (newObject.GameData is IGameData item)
            {
                UpdateContextMenu();

                _subItems.Clear();

                var list = item.SubFiles;
                foreach (var a in list)
                {
                    TreeViewItemVM temp = new TreeViewItemVM(a);
                    temp.ItemAction += SubFile_ItemAction;
                    _subItems.Add(temp);
                }
            }
            if (newObject.GameData is IImage image)
            {
                BitmapSource = image.GetBitmap().GetBitmapSource();
            }
        }

        private void SubFile_ItemAction(TreeViewItemVM sender, UserTreeViewItemEventEnum action) => ItemAction?.Invoke(sender, action);
    }
}