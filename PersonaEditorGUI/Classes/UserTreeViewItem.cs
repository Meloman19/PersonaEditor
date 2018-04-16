using Microsoft.Win32;
using PersonaEditorGUI.Controls;
using PersonaEditorLib;
using PersonaEditorLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace PersonaEditorGUI.Classes
{
    public class UserTreeViewItem : BindingObject
    {
        public event TreeViewItemEventHandler SelectedItemChanged;
        public event TreeViewItemEventHandler SelectedItemOpen;

        public bool Close()
        {
            return true;
        }

        #region Property

        ObservableCollection<UserTreeViewItem> _SubItems = new ObservableCollection<UserTreeViewItem>();
        public ReadOnlyObservableCollection<UserTreeViewItem> SubItems { get; }

        private string header => _personaFile.Name;
        public string Header
        {
            get { return header; }
            set
            {

            }
        }

        private bool edit;
        public bool Edit
        {
            get { return edit; }
            set
            {
                if (edit != value)
                {
                    edit = value;
                    Notify("Edit");
                }
            }
        }

        private bool isEnable = true;
        public bool IsEnable => isEnable;

        public void UnEnable()
        {
            isEnable = false;
            foreach (var a in SubItems)
                a.UnEnable();
            Notify("IsEnable");
        }

        public void Enable()
        {
            isEnable = true;
            foreach (var a in SubItems)
                a.Enable();
            Notify("IsEnable");
        }

        public bool CanEdit()
        {
            if (!isEnable)
                return false;

            bool returned = true;
            foreach (var a in SubItems)
                returned &= a.CanEdit();

            return returned;
        }

        private ObjectFile _personaFile = null;
        public ObjectFile PersonaFile => _personaFile;

        private bool _AllowDrop = true;
        public bool AllowDrop => _AllowDrop;

        public bool IsSelected
        {
            set
            {
                if (value)
                {
                    SelectedItemChanged?.Invoke(this);
                    Edit = true;
                }
            }
        }

        #endregion Property

        #region Events

        public MouseButtonEventHandler DoubleClick => MouseDoubleClick;
        public MouseEventHandler Leave => MouseLeave;
        public DragEventHandler Drop => DropItem;

        private void MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_SubItems.Count == 0)
                SelectedItemOpen?.Invoke(this);
        }

        private void MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is DependencyObject obj)
                if (e.LeftButton == MouseButtonState.Pressed)
                    DragDrop.DoDragDrop(obj, GetDragObject(), DragDropEffects.Copy);
        }

        private void DropItem(object sender, DragEventArgs e)
        {
            if (PersonaFile is IPersonaFile pFile)
            {
                string[] temp = e.Data.GetData(DataFormats.FileDrop) as string[];

                if (temp.Length > 0)
                    if (MessageBox.Show("Replace " + PersonaFile.Name + "?", "Replace?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                    {
                        var personaFile = PersonaEditorLib.Utilities.PersonaFile.OpenFile(this.PersonaFile.Name, File.ReadAllBytes(temp[0]), pFile.Type);
                        if (personaFile.Object != null)
                            _personaFile.Object = personaFile.Object;
                        else
                            MessageBox.Show("Error. " + Path.GetFileName(temp[0]) + " is not a " + pFile.Type + " type");
                    }
            }
        }

        #endregion Events

        public UserTreeViewItem(ObjectFile personaFile)
        {
            SubItems = new ReadOnlyObservableCollection<UserTreeViewItem>(_SubItems);
            actionSaveAs = new Action(SaveAs_Click);
            actionReplace = new Action(Replace_Click);
            actionSaveAll = new Action(SaveAll_Click);
            actionEdit = new Action(Edit_Click);

            _personaFile = personaFile;

            if (!(personaFile.Object is IPersonaFile))
                throw new Exception("UserTreeViewItem: context not IPersonaFile");

            Update(personaFile.Object as IPersonaFile);
        }

        #region ContextMenu

        public ObservableCollection<object> ContextMenu { get; } = new ObservableCollection<object>();

        private void UpdateContextMenu(List<ContextMenuItems> list)
        {
            ContextMenu.Clear();
            foreach (var a in list)
            {
                if (a == ContextMenuItems.SaveAs)
                {
                    MenuItem menuItem = new MenuItem();
                    menuItem.Header = Application.Current.Resources.MergedDictionaries.GetString("tree_SaveAs");
                    menuItem.Command = new RelayCommandWeak(actionSaveAs);
                    ContextMenu.Add(menuItem);
                }
                else if (a == ContextMenuItems.SaveAll)
                {
                    MenuItem menuItem = new MenuItem();
                    menuItem.Header = Application.Current.Resources.MergedDictionaries.GetString("tree_SaveAll");
                    menuItem.Command = new RelayCommandWeak(actionSaveAll);
                    ContextMenu.Add(menuItem);
                }
                else if (a == ContextMenuItems.Replace)
                {
                    MenuItem menuItem = new MenuItem();
                    menuItem.Header = Application.Current.Resources.MergedDictionaries.GetString("tree_Replace");
                    menuItem.Command = new RelayCommandWeak(actionReplace);
                    ContextMenu.Add(menuItem);
                }
                else if (a == ContextMenuItems.Edit)
                {
                    MenuItem menuItem = new MenuItem();
                    menuItem.Header = Application.Current.Resources.MergedDictionaries.GetString("tree_Edit");
                    menuItem.Command = new RelayCommand(actionEdit);
                    ContextMenu.Add(menuItem);
                }
                else if (a == ContextMenuItems.Separator)
                    ContextMenu.Add(new Separator());
            }
        }

        private Action actionSaveAs;
        private void SaveAs_Click()
        {
            SaveFileDialog SFD = new SaveFileDialog();
            FileType fileType = (PersonaFile.Object as IPersonaFile).Type;
            string filter = PersonaEditorLib.Utilities.PersonaFile.PersonaSaveFileFilter.ContainsKey(fileType) ? PersonaEditorLib.Utilities.PersonaFile.PersonaSaveFileFilter[fileType] : "";
            SFD.Filter = "RAW Data|*.*" + filter;
            SFD.FileName = PersonaFile.Name;
            SFD.DefaultExt = Path.GetExtension(PersonaFile.Name);
            SFD.AddExtension = true;
            SFD.OverwritePrompt = true;
            if (SFD.ShowDialog() == true)
            {
                if (SFD.FilterIndex > 1)
                {
                    if (PersonaFile.Object is IPersonaFile pFile)
                    {
                        if (pFile.Type == FileType.TMX | pFile.Type == FileType.FNT | pFile.Type == FileType.DDS)
                            PersonaEditorLib.Utilities.PersonaFile.SaveImageFile(PersonaFile, SFD.FileName);
                        else if (pFile.Type == FileType.BMD)
                        {
                            var result = PersonaEditorGUI.Controls.ToolBox.ToolBox.Show(PersonaEditorGUI.Controls.ToolBox.ToolBoxType.SaveAsPTP);
                            if (result == PersonaEditorGUI.Controls.ToolBox.ToolBoxResult.Ok)
                            {
                                PersonaEditorLib.PersonaEncoding.PersonaEncoding temp = Settings.AppSetting.Default.SaveAsPTP_CO2N ? Static.EncodingManager.GetPersonaEncoding(Settings.AppSetting.Default.SaveAsPTP_Font) : null;
                                PersonaEditorLib.Utilities.PersonaFile.SavePTPFile(PersonaFile, SFD.FileName, temp);
                            }
                        }
                        else
                            throw new Exception("SavePersonaFileDialog");
                    }
                    else
                        throw new Exception("SavePersonaFileDialog");
                }
                else
                    File.WriteAllBytes(SFD.FileName, (PersonaFile.Object as IFile).Get());
            }
        }

        private Action actionReplace;
        private void Replace_Click()
        {
            FileType fileType = (PersonaFile.Object as IPersonaFile).Type;
            OpenFileDialog OFD = new OpenFileDialog();
            string filter = PersonaEditorLib.Utilities.PersonaFile.PersonaOpenFileFilter.ContainsKey(fileType) ? PersonaEditorLib.Utilities.PersonaFile.PersonaOpenFileFilter[fileType] : "";
            OFD.Filter = "RAW Data|*.*" + filter;
            OFD.CheckFileExists = true;
            OFD.CheckPathExists = true;
            OFD.Multiselect = false;

            if (OFD.ShowDialog() == true)
            {
                if (OFD.FilterIndex > 1)
                {
                    if (fileType == FileType.TMX | fileType == FileType.FNT)
                        PersonaEditorLib.Utilities.PersonaFile.OpenImageFile(PersonaFile, OFD.FileName);
                    else if (fileType == FileType.BMD)
                    {
                        var result = PersonaEditorGUI.Controls.ToolBox.ToolBox.Show(PersonaEditorGUI.Controls.ToolBox.ToolBoxType.OpenPTP);
                        if (result == PersonaEditorGUI.Controls.ToolBox.ToolBoxResult.Ok)
                            PersonaEditorLib.Utilities.PersonaFile.OpenPTPFile(PersonaFile, OFD.FileName, Static.EncodingManager.GetPersonaEncoding(Settings.AppSetting.Default.OpenPTP_Font));
                    }
                    else
                        throw new Exception("OpenPersonaFileDialog");
                }
                else
                {
                    var item = PersonaEditorLib.Utilities.PersonaFile.OpenFile("", File.ReadAllBytes(OFD.FileName), fileType);

                    if (item != null)
                        PersonaFile.Object = item;
                }
            }
            //PersonaEditorLib.Utilities.PersonaFile.OpenPersonaFileDialog(personaFile, Static.EncodingManager);

            //Update(_personaFile.Object as IPersonaFile);
        }

        private Action actionSaveAll;
        private void SaveAll_Click()
        {
            System.Windows.Forms.FolderBrowserDialog FBD = new System.Windows.Forms.FolderBrowserDialog();
            if (FBD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = FBD.SelectedPath;
                if (PersonaFile.Object is IPersonaFile file)
                {
                    var list = file.SubFiles;
                    foreach (var item in list)
                        File.WriteAllBytes(Path.Combine(path, item.Name), (item.Object as IFile).Get());
                }
            }
        }

        private Action actionEdit;
        private void Edit_Click()
        {
            SelectedItemOpen?.Invoke(this);
        }

        #endregion ContextMenu

        private void Update(IPersonaFile item)
        {
            UpdateContextMenu(PersonaEditorLib.Utilities.PersonaFile.GetContextMenuItems(item.Type));

            _SubItems.Clear();

            var list = item.SubFiles;
            foreach (var a in list)
            {
                UserTreeViewItem temp = new UserTreeViewItem(a);
                temp.SelectedItemChanged += SubFile_SelectedItemChanged;
                temp.SelectedItemOpen += SubFile_SelectedItemOpen;
                _SubItems.Add(temp);
            }
        }

        private void SubFile_SelectedItemChanged(UserTreeViewItem sender) => SelectedItemChanged?.Invoke(sender);

        private void SubFile_SelectedItemOpen(UserTreeViewItem sender) => SelectedItemOpen?.Invoke(sender);

        public DataObject GetDragObject()
        {
            DataObject data = new DataObject();

            string filepath = Path.Combine(Path.GetTempPath(), (Header as string).Replace('/', '+'));

            data.SetData(typeof(UserTreeViewItem), this);

            string[] paths = new string[] { filepath };
            File.WriteAllBytes(paths[0], (PersonaFile.Object as IFile).Get());

            data.SetData(DataFormats.FileDrop, paths);

            return data;
        }
    }
}