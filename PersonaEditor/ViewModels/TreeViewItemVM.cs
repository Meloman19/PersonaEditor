using PersonaEditorLib;
using AuxiliaryLibraries.WPF;
using AuxiliaryLibraries.WPF.Wrapper;
using PersonaEditor.Classes;
using PersonaEditor.Classes.Delegates;
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

        #region Private Properties

        private ObservableCollection<object> _contextMenu = new ObservableCollection<object>();
        private ObservableCollection<TreeViewItemVM> _subItems = new ObservableCollection<TreeViewItemVM>();
        private bool _edit;
        private bool _isEnable = true;
        private bool _isSelected = false;
        private bool _AllowDrop = true;
        private GameFile _personaFile = null;

        #endregion Private Properties

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
            get { return _edit; }
            set
            {
                if (_edit != value)
                {
                    _edit = value;
                    Notify("Edit");
                }
            }
        }

        public bool IsEnable => _isEnable;

        public GameFile PersonaFile => _personaFile;

        public bool AllowDrop => _AllowDrop;

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

        #region Events

        public ICommand DoubleClick { get; }
        public ICommand Leave { get; }
        public ICommand Drop { get; }

        private void MouseDoubleClick(object arg)
        {
            if (_subItems.Count == 0)
                ItemAction?.Invoke(this, UserTreeViewItemEventEnum.Open);
        }

        private void MouseLeave(object arg)
        {
            MouseEventArgs mouseEventArgs = arg as MouseEventArgs;
            if (mouseEventArgs.Source is DependencyObject obj)
                if (mouseEventArgs.LeftButton == MouseButtonState.Pressed)
                    DragDrop.DoDragDrop(obj, GetDragObject(), DragDropEffects.Copy);
        }

        private void DropItem(object sender, DragEventArgs e)
        {
            if (PersonaFile is IGameData pFile)
            {
                string[] temp = e.Data.GetData(DataFormats.FileDrop) as string[];

                if (temp.Length > 0)
                    if (MessageBox.Show("Replace " + PersonaFile.Name + "?", "Replace?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                    {
                        var personaFile = GameFormatHelper.OpenFile(PersonaFile.Name, File.ReadAllBytes(temp[0]), pFile.Type);
                        if (personaFile != null)
                            _personaFile.GameData = personaFile.GameData;
                        else
                            MessageBox.Show("Error. " + Path.GetFileName(temp[0]) + " is not a " + pFile.Type + " type");
                    }
            }
        }

        private DataObject GetDragObject()
        {
            DataObject data = new DataObject();

            string filepath = Path.Combine(Path.GetTempPath(), (Header as string).Replace('/', '+'));

            data.SetData(typeof(TreeViewItemVM), this);

            string[] paths = new string[] { filepath };
            File.WriteAllBytes(paths[0], (PersonaFile.GameData as IGameData).GetData());

            data.SetData(DataFormats.FileDrop, paths);

            return data;
        }

        #endregion Events

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

        public TreeViewItemVM(GameFile personaFile)
        {
            DoubleClick = new RelayCommand(MouseDoubleClick);
            Leave = new RelayCommand(MouseLeave);
            // Drop = new RelayCommand(DropItem);

            SubItems = new ReadOnlyObservableCollection<TreeViewItemVM>(_subItems);
            ContextMenu = new ReadOnlyObservableCollection<object>(_contextMenu);

            _personaFile = personaFile ?? throw new ArgumentNullException(nameof(personaFile));
            Update(personaFile);
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