using AuxiliaryLibraries.GameFormat;
using AuxiliaryLibraries.GameFormat.Text;
using AuxiliaryLibraries.WPF;
using Microsoft.Win32;
using PersonaEditor;
using PersonaEditorGUI.Classes.Delegates;
using PersonaEditorGUI.Controls.ToolBox;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace PersonaEditorGUI.Classes
{
    public class TreeViewItemVM : BindingObject
    {
        public event TreeViewItemEventHandler ItemAction;

        #region Private Properties

        private ObservableCollection<object> _contextMenu = new ObservableCollection<object>();
        private ObservableCollection<TreeViewItemVM> _subItems = new ObservableCollection<TreeViewItemVM>();
        private bool _edit;
        private bool _isEnable = true;
        private bool _isSelected = false;
        private bool _AllowDrop = true;
        private ObjectContainer _personaFile = null;

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

        public ObjectContainer PersonaFile => _personaFile;

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
            if (PersonaFile is IGameFile pFile)
            {
                string[] temp = e.Data.GetData(DataFormats.FileDrop) as string[];

                if (temp.Length > 0)
                    if (MessageBox.Show("Replace " + PersonaFile.Name + "?", "Replace?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                    {
                        var personaFile = GameFormatHelper.OpenFile(this.PersonaFile.Name, File.ReadAllBytes(temp[0]), pFile.Type);
                        if (personaFile.Object != null)
                            _personaFile.Object = personaFile.Object;
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
            File.WriteAllBytes(paths[0], (PersonaFile.Object as IGameFile).GetData());

            data.SetData(DataFormats.FileDrop, paths);

            return data;
        }

        #endregion Events

        #region ContextMenu

        private void UpdateContextMenu()
        {
            _contextMenu.Clear();

            MenuItem menuItem = null;

            if (PersonaFileHelper.IsEdited(PersonaFile))
            {
                menuItem = new MenuItem();
                menuItem.Header = Application.Current.Resources.MergedDictionaries.GetString("tree_Edit");
                menuItem.Command = new RelayCommand(ContextMenu_Edit);
                _contextMenu.Add(menuItem);
            }

            menuItem = new MenuItem();
            menuItem.Header = Application.Current.Resources.MergedDictionaries.GetString("tree_Replace");
            menuItem.Command = new RelayCommand(ContextMenu_Replace);
            _contextMenu.Add(menuItem);

            _contextMenu.Add(new Separator());

            menuItem = new MenuItem();
            menuItem.Header = Application.Current.Resources.MergedDictionaries.GetString("tree_SaveAs");
            menuItem.Command = new RelayCommand(ContextMenu_SaveAs);
            _contextMenu.Add(menuItem);

            if (PersonaFileHelper.HaveSubFiles(PersonaFile))
            {
                menuItem = new MenuItem();
                menuItem.Header = Application.Current.Resources.MergedDictionaries.GetString("tree_SaveAll");
                menuItem.Command = new RelayCommand(ContextMenu_SaveAll);
                _contextMenu.Add(menuItem);
            }
        }

        private void ContextMenu_Edit()
        {
            ItemAction?.Invoke(this, UserTreeViewItemEventEnum.Open);
        }

        private void ContextMenu_Replace()
        {
            FormatEnum fileType = (PersonaFile.Object as IGameFile).Type;

            OpenFileDialog OFD = new OpenFileDialog();
            string name = PersonaFile.Name.Replace('/', '+');
            OFD.Filter = $"RAW(*{Path.GetExtension(name)})|*{Path.GetExtension(name)}";
            OFD.CheckFileExists = true;
            OFD.CheckPathExists = true;
            OFD.Multiselect = false;

            if (PersonaFile.Object is IImage)
                OFD.Filter += $"|PNG (*.png)|*.png";
            if (PersonaFile.Object is ITable)
                OFD.Filter += $"|XML data table (*.xml)|*.xml";
            if (PersonaFile.Object is BMD)
                OFD.Filter += $"|Persona Text Project (*.ptp)|*.ptp";

            if (OFD.ShowDialog() == true)
            {
                if (OFD.FilterIndex == 1)
                {
                    var item = GameFormatHelper.OpenFile("", File.ReadAllBytes(OFD.FileName), fileType);

                    if (item.Object != null)
                        PersonaFile.Object = item.Object;
                }
                else
                {
                    string ext = Path.GetExtension(OFD.FileName);
                    if (ext.Equals(".png", StringComparison.CurrentCultureIgnoreCase))
                        PersonaEditorTools.OpenImageFile(PersonaFile, OFD.FileName);
                    else if (ext.Equals(".xml", StringComparison.CurrentCultureIgnoreCase))
                        PersonaEditorTools.OpenTableFile(PersonaFile, OFD.FileName);
                    else if (ext.Equals(".ptp", StringComparison.CurrentCultureIgnoreCase))
                    {
                        var result = ToolBox.Show(ToolBoxType.OpenPTP);
                        if (result == ToolBoxResult.Ok)
                            PersonaEditorTools.OpenPTPFile(PersonaFile, OFD.FileName, Static.EncodingManager.GetPersonaEncoding(Settings.AppSetting.Default.OpenPTP_Font));
                    }
                    else
                        throw new Exception("OpenPersonaFileDialog");
                }

                Update(_personaFile);
                if (_isSelected)
                    ItemAction?.Invoke(this, UserTreeViewItemEventEnum.Selected);
            }
        }

        private void ContextMenu_SaveAs()
        {
            SaveFileDialog SFD = new SaveFileDialog();
            SFD.OverwritePrompt = true;
            SFD.AddExtension = false;
            SFD.FileName = PersonaFile.Name.Replace('/', '+');
            SFD.Filter = $"RAW(*{Path.GetExtension(SFD.FileName)})|*{Path.GetExtension(SFD.FileName)}";

            if (PersonaFile.Object is IImage)
                SFD.Filter += $"|PNG (*.png)|*.png";
            if (PersonaFile.Object is ITable)
                SFD.Filter += $"|XML data table (*.xml)|*.xml";
            if (PersonaFile.Object is BMD)
                SFD.Filter += $"|Persona Text Project (*.ptp)|*.ptp";
            if (PersonaFile.Object is PTP)
                SFD.Filter += $"|BMD Text File (*.bmd)|*.bmd";

            SFD.InitialDirectory = Path.GetDirectoryName(Static.OpenedFile);
            if (SFD.ShowDialog() == true)
                if (SFD.FilterIndex == 1)
                    File.WriteAllBytes(SFD.FileName, (PersonaFile.Object as IGameFile).GetData());
                else
                {
                    string ext = Path.GetExtension(SFD.FileName);
                    if (ext.Equals(".png", StringComparison.CurrentCultureIgnoreCase))
                        PersonaEditor.PersonaEditorTools.SaveImageFile(PersonaFile, SFD.FileName);
                    else if (ext.Equals(".ptp", StringComparison.CurrentCultureIgnoreCase))
                    {
                        var result = ToolBox.Show(ToolBoxType.SaveAsPTP);
                        if (result == ToolBoxResult.Ok)
                        {
                            PersonaEncoding temp = Settings.AppSetting.Default.SaveAsPTP_CO2N ? Static.EncodingManager.GetPersonaEncoding(Settings.AppSetting.Default.SaveAsPTP_Font) : null;
                            PersonaEditor.PersonaEditorTools.SavePTPFile(PersonaFile, SFD.FileName, temp);
                        }
                    }
                    else if (ext.Equals(".bmd", StringComparison.CurrentCultureIgnoreCase))
                    {
                        Encoding encoding = Static.EncodingManager.GetPersonaEncoding(Settings.AppSetting.Default.PTPNewDefault);
                        BMD bmd = new BMD();
                        bmd.Open(PersonaFile.Object as PTP, encoding);
                        File.WriteAllBytes(SFD.FileName, bmd.GetData());
                    }
                    else if (ext.Equals(".xml", StringComparison.CurrentCultureIgnoreCase))
                        PersonaEditor.PersonaEditorTools.SaveTableFile(PersonaFile, SFD.FileName);
                    else throw new Exception("SavePersonaFileDialog");
                }
        }

        private void ContextMenu_SaveAll()
        {
            System.Windows.Forms.FolderBrowserDialog FBD = new System.Windows.Forms.FolderBrowserDialog();
            if (FBD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = FBD.SelectedPath;
                if (PersonaFile.Object is IGameFile file)
                {
                    var list = file.SubFiles;
                    foreach (var item in list)
                        File.WriteAllBytes(Path.Combine(path, item.Name), (item.Object as IGameFile).GetData());
                }
            }
        }

        #endregion ContextMenu

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

        public TreeViewItemVM(ObjectContainer personaFile)
        {
            DoubleClick = new RelayCommand(MouseDoubleClick);
            Leave = new RelayCommand(MouseLeave);
            // Drop = new RelayCommand(DropItem);

            SubItems = new ReadOnlyObservableCollection<TreeViewItemVM>(_subItems);
            ContextMenu = new ReadOnlyObservableCollection<object>(_contextMenu);

            _personaFile = personaFile;

            if (!(personaFile.Object is IGameFile))
                throw new Exception("UserTreeViewItem: context not IPersonaFile");

            Update(personaFile);
        }

        private void Update(ObjectContainer newObject)
        {
            if (newObject.Object is IGameFile item)
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
            if (newObject.Object is IImage image)
            {
                BitmapSource = image.GetBitmap().GetBitmapSource();
            }
        }

        private void SubFile_ItemAction(TreeViewItemVM sender, UserTreeViewItemEventEnum action) => ItemAction?.Invoke(sender, action);
    }
}