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
using System.Windows.Media.Imaging;

namespace PersonaEditorGUI.Files
{
    static class Static
    {
        public static class Paths
        {
            public static string CurrentFolderEXE = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            public static string DirBackgrounds = Path.Combine(CurrentFolderEXE, "background");
            public static string DirFont = Path.Combine(CurrentFolderEXE, "font");
            public static string OpenFileName = "";
            public static string FontOld = Path.Combine(DirFont, "FONT_OLD.FNT");
            public static string FontOldMap = Path.Combine(DirFont, "FONT_OLD.TXT");
            public static string FontNew = Path.Combine(DirFont, "FONT_NEW.FNT");
            public static string FontNewMap = Path.Combine(DirFont, "FONT_NEW.TXT");
        }
    }

    class UserTreeViewItem : BindingObject
    {
        public ObservableCollection<UserTreeViewItem> SubItems { get; } = new ObservableCollection<UserTreeViewItem>();

        private string header = "";
        public string Header
        {
            get { return header; }
            set
            {
                if (header != value)
                {
                    header = value;
                    Notify("Header");
                }
            }
        }

        private object _personaFile = null;
        public object personaFile
        {
            get { return _personaFile; }
        }

        private bool _AllowDrop = true;
        public bool AllowDrop
        {
            get { return _AllowDrop; }
        }

        public bool IsMouseOver { get; set; } = false;

        public List<ContextMenuItems> ContextMenuItems
        {
            get { return (personaFile as IPersonaFile).ContextMenuList; }
        }

        public UserTreeViewItem(object context)
        {
            _personaFile = context;

            if (context is IPersonaFile item)
            {
                Header = item.Name;

                Update(item);
            }
            else
                throw new Exception("UserTreeViewItem: context not IPersonaFile");
        }

        private void Update(IPersonaFile item)
        {
            SubItems.Clear();
            var list = item.GetSubFiles();
            foreach (var a in list)
                SubItems.Add(new UserTreeViewItem(a));
        }

        public void Replace(object context)
        {
            if (personaFile is IPersonaFile item)
            {
                item.Replace(context);
                Update(item);
            }
        }

        //public UserTreeViewItem(byte[] data, FileType type, string name, bool IsLittleEndian)
        //{
        //    Header = name;
        //    Update(data, type, IsLittleEndian);
        //}

        //public void Update(byte[] data, FileType type, bool IsLittleEndian)
        //{
        //    SubItems.Clear();
        //    if (type == FileType.PTP)
        //    {
        //        CharList OldCharList = new CharList();
        //        CharList NewCharList = new CharList();

        //        OldCharList.Tag = "old";
        //        OldCharList.OpenFont(Static.Paths.FontOld);
        //        OldCharList.OpenFontMap(Static.Paths.FontOldMap);

        //        NewCharList.Tag = "new";
        //        NewCharList.OpenFont(Static.Paths.FontNew);
        //        NewCharList.OpenFontMap(Static.Paths.FontNewMap);
        //        _personaFile = PersonaEditorLib.Utilities.PersonaFile.OpenFile(data, type, IsLittleEndian, new object[] { OldCharList, NewCharList });
        //    }
        //    else
        //        _personaFile = PersonaEditorLib.Utilities.PersonaFile.OpenFile(data, type, IsLittleEndian);

        //    foreach (var a in (personaFile as IPersonaFile).GetSubFiles())
        //        SubItems.Add(new UserTreeViewItem(a.Data, a.Type, a.Name, IsLittleEndian));

        //    if (Header == "")
        //        if (personaFile is IName tmp)
        //            Header = tmp.Name;
        //}

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog OFD = new OpenFileDialog();
            OFD.Filter = Info.FileInfo.Find(x => x.Item1 == (personaFile as IPersonaFile).Type).Item2;
            OFD.CheckFileExists = true;
            OFD.CheckPathExists = true;
            OFD.Multiselect = false;

            if (OFD.ShowDialog() == true)
            {
                // Update(File.ReadAllBytes(OFD.FileName), (personaFile as IPersonaFile).Type, true);
            }
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog SFD = new SaveFileDialog();
            SFD.Filter = Info.FileInfo.Find(x => x.Item1 == (personaFile as IPersonaFile).Type).Item2;
            SFD.FileName = Header as String;
            SFD.OverwritePrompt = true;
            if (SFD.ShowDialog() == true)
            {
                File.WriteAllBytes(SFD.FileName, (personaFile as IFile).Get());
            }
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

            data.SetData(typeof(UserTreeViewItem), this);

            if (CtrlDown && personaFile is IImage img)
            {
                string[] paths = new string[] { Path.Combine(Path.GetDirectoryName(filepath), Path.GetFileNameWithoutExtension(filepath) + ".png") };
                PersonaEditorLib.Extension.Imaging.SaveBMP(img.Image, paths[0]);
                data.SetData(DataFormats.FileDrop, paths);
            }
            else
            {
                string[] paths = new string[] { filepath };
                File.WriteAllBytes(paths[0], (personaFile as IFile).Get());
                data.SetData(DataFormats.FileDrop, paths);
            }

            CtrlDown = false;

            return data;
        }

        #endregion Drag
    }
}