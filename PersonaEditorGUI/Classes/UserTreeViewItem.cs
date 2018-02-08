using Microsoft.Win32;
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
    class UserTreeViewItem : BindingObject
    {
        public bool Close()
        {
            return true;
        }

        ObservableCollection<UserTreeViewItem> _SubItems { get; } = new ObservableCollection<UserTreeViewItem>();
        public ReadOnlyObservableCollection<UserTreeViewItem> SubItems { get; }

        List<EventWrapper> SubItemEW = new List<EventWrapper>();

        EventWrapper fileEW;

        public string Header => _personaFile.Name;

        private ObjectFile _personaFile = null;
        public ObjectFile personaFile => _personaFile;

        private bool _AllowDrop = true;
        public bool AllowDrop => _AllowDrop;

        public bool IsSelected
        {
            set
            {
                if (value)
                    Notify("IsSelected");
            }
        }

        #region Events

        public MouseButtonEventHandler DoubleClick => MouseDoubleClick;
        public MouseEventHandler Leave => MouseLeave;
        public KeyEventHandler PreviewUp => PreviewKeyUp;
        public KeyEventHandler PreviewDown => PreviewKeyDown;
        public DragEventHandler Drop => DropItem;
        public RoutedEventHandler Loaded => ItemLoaded;
        public RoutedEventHandler Unloaded => ItemUnloaded;

        private void MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_SubItems.Count == 0)
                Notify("Open");
        }

        private void MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is DependencyObject obj)
                if (e.LeftButton == MouseButtonState.Pressed)
                    DragDrop.DoDragDrop(obj, GetDragObject(), DragDropEffects.Copy);

            CtrlDown = false;
        }

        private void PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.OriginalSource == e.Source)
                KeyUp(e.Key);
        }

        private void PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.OriginalSource == e.Source)
                KeyDown(e.Key);
        }

        private void DropItem(object sender, DragEventArgs e)
        {
            if (personaFile is IPersonaFile pFile)
            {
                string[] temp = e.Data.GetData(DataFormats.FileDrop) as string[];

                if (temp.Length > 0)
                    if (MessageBox.Show("Replace " + personaFile.Name + "?", "Replace?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                    {
                        var personaFile = PersonaEditorLib.Utilities.PersonaFile.OpenFile(this.personaFile.Name, File.ReadAllBytes(temp[0]), pFile.Type);
                        if (personaFile != null)
                            _personaFile.Object = personaFile.Object;
                        else
                            MessageBox.Show("Error. " + Path.GetFileName(temp[0]) + " is not a " + pFile.Type + " type");
                    }
            }
        }

        private void ItemLoaded(object sender, RoutedEventArgs e)
        {
            Update(personaFile.Object as IPersonaFile);
        }

        private void ItemUnloaded(object sender, RoutedEventArgs e)
        { 
        }

        #endregion Events

        public UserTreeViewItem(ObjectFile personaFile)
        {
            SubItems = new ReadOnlyObservableCollection<UserTreeViewItem>(_SubItems);
            actionSaveAs = new Action(SaveAs_Click);
            actionReplace = new Action(Replace_Click);
            actionSaveAll = new Action(SaveAll_Click);

            _personaFile = personaFile;
            fileEW = new EventWrapper(personaFile, this);

            if (!(personaFile.Object is IPersonaFile))
                throw new Exception("UserTreeViewItem: context not IPersonaFile");
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
                    menuItem.Header = "Save As...";
                    menuItem.Command = new RelayCommandWeak(actionSaveAs);
                    ContextMenu.Add(menuItem);
                }
                else if (a == ContextMenuItems.SaveAll)
                {
                    MenuItem menuItem = new MenuItem();
                    menuItem.Header = "Export All";
                    menuItem.Command = new RelayCommandWeak(actionSaveAll);
                    ContextMenu.Add(menuItem);
                }
                else if (a == ContextMenuItems.Replace)
                {
                    MenuItem menuItem = new MenuItem();
                    menuItem.Header = "Replace";
                    menuItem.Command = new RelayCommandWeak(actionReplace);
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
            SFD.Filter = Info.FileInfo.Find(x => x.Item1 == (personaFile.Object as IPersonaFile).Type).Item2;
            SFD.FileName = Header as String;
            SFD.OverwritePrompt = true;
            if (SFD.ShowDialog() == true)
            {

                File.WriteAllBytes(SFD.FileName, (personaFile as IFile).Get());
            }
        }

        private Action actionReplace;
        private void Replace_Click()
        {
            OpenFileDialog OFD = new OpenFileDialog();
            OFD.Filter = Info.FileInfo.Find(x => x.Item1 == (personaFile.Object as IPersonaFile).Type).Item2;
            OFD.CheckFileExists = true;
            OFD.CheckPathExists = true;
            OFD.Multiselect = false;

            if (OFD.ShowDialog() == true)
            {
                var item = PersonaEditorLib.Utilities.PersonaFile.OpenFile(Header, File.ReadAllBytes(OFD.FileName), (personaFile.Object as IPersonaFile).Type);
                if (item != null)
                {
                    _personaFile.Object = item.Object;
                    Update(item as IPersonaFile);
                }
            }
        }

        private Action actionSaveAll;
        private void SaveAll_Click()
        {
            System.Windows.Forms.FolderBrowserDialog FBD = new System.Windows.Forms.FolderBrowserDialog();
            if (FBD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = FBD.SelectedPath;
                if (personaFile is IPersonaFile file)
                {
                    var list = file.GetSubFiles();
                    foreach (var item in list)
                        File.WriteAllBytes(Path.Combine(path, item.Name), (item.Object as IFile).Get());
                }
            }
        }

        #endregion ContextMenu

        private void Update(IPersonaFile item)
        {
            UpdateContextMenu(item.ContextMenuList);

            _SubItems.Clear();
            SubItemEW.Clear();


            var list = item.GetSubFiles();
            foreach (var a in list)
            {
                UserTreeViewItem temp = new UserTreeViewItem(a);
                _SubItems.Add(temp);
                SubItemEW.Add(new EventWrapper(temp, this));
            }
        }

        public override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Object")
            {
            }
            else
                TunnelNotify(sender, e);
        }

        #region Drag

        bool CtrlDown = false;

        public void KeyUp(System.Windows.Input.Key Key)
        {
            if (Key == System.Windows.Input.Key.LeftCtrl)
                CtrlDown = false;
        }

        public void KeyDown(System.Windows.Input.Key Key)
        {
            if (Key == System.Windows.Input.Key.LeftCtrl)
                CtrlDown = true;
        }

        public DataObject GetDragObject()
        {
            DataObject data = new DataObject();

            string filepath = Path.Combine(Path.GetTempPath(), (Header as string).Replace('/', '+'));

            data.SetData(typeof(ObjectFile), personaFile);

            if (CtrlDown && personaFile.Object is IImage img)
            {
                string[] paths = new string[] { Path.Combine(Path.GetDirectoryName(filepath), Path.GetFileNameWithoutExtension(filepath) + ".png") };
                PersonaEditorLib.Extension.Imaging.SaveBMP(img.GetImage(), paths[0]);
                data.SetData(DataFormats.FileDrop, paths);
            }
            else
            {
                string[] paths = new string[] { filepath };
                File.WriteAllBytes(paths[0], (personaFile.Object as IFile).Get());
              
                data.SetData(DataFormats.FileDrop, paths);
            }
            
            return data;
        }

        #endregion Drag
    }
}