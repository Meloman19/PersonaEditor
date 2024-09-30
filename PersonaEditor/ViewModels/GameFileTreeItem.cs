using System;
using System.Windows.Media.Imaging;
using AuxiliaryLibraries.WPF.Wrapper;
using PersonaEditor.Common;
using PersonaEditor.Common.Delegates;
using PersonaEditorLib;

namespace PersonaEditor.ViewModels
{
    public partial class GameFileTreeItem : TreeViewItemViewModel
    {
        public event TreeViewItemEventHandler ItemAction;

        #region Private

        private GameFile _personaFile = null;
        private BitmapSource _bitmapSource = null;

        #endregion Private

        public GameFileTreeItem(GameFile personaFile)
        {
            _personaFile = personaFile ?? throw new ArgumentNullException(nameof(personaFile));
            Update(personaFile);
        }

        #region Property

        public GameFile PersonaFile => _personaFile;

        public BitmapSource BitmapSource
        {
            get
            {
                if (_bitmapSource == null &&
                    _personaFile != null &&
                    _personaFile.GameData is IImage image)
                {
                    _bitmapSource = image.GetBitmap().GetBitmapSource();
                }

                return _bitmapSource;
            }
        }

        #endregion Property

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);

            switch (propertyName)
            {
                case nameof(TreeViewItemViewModel.IsSelected):
                    if (IsSelected)
                        ItemAction?.Invoke(this, UserTreeViewItemEventEnum.Selected);
                    break;
            }
        }

        public void UnEnable()
        {
            IsEnabled = false;
            foreach (GameFileTreeItem a in SubItems)
                a.UnEnable();
        }

        public void Enable()
        {
            IsEnabled = true;
            foreach (GameFileTreeItem a in SubItems)
                a.Enable();
        }

        public bool CanEdit()
        {
            if (!IsEnabled)
                return false;

            bool returned = true;
            foreach (GameFileTreeItem a in SubItems)
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
                Header = newObject.Name;

                UpdateContextMenu();

                SubItems.Clear();

                var list = item.SubFiles;
                foreach (var a in list)
                {
                    GameFileTreeItem temp = new GameFileTreeItem(a);
                    temp.ItemAction += SubFile_ItemAction;
                    SubItems.Add(temp);
                }
            }
            _bitmapSource = null;
            // temporarily disabled lazy load
            _ = BitmapSource;
        }

        private void SubFile_ItemAction(GameFileTreeItem sender, UserTreeViewItemEventEnum action) => ItemAction?.Invoke(sender, action);
    }
}