using PersonaEditorLib;
using PersonaEditorLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PersonaEditorGUI.Controls.Editors
{
    class FTDSubEntryVM : BindingObject
    {
        byte[] entry;
        Encoding encoding;

        private string _Entry = "";
        public string Entry => _Entry;

        public string EntryString => encoding == null ? "" : encoding.GetString(entry);

        private ContextMenu contextMenu = null;
        public ContextMenu ContextMenu => contextMenu;

        public void SetEncoding(Encoding encoding)
        {
            this.encoding = encoding;
            Notify("EntryString");
        }

        public void Replace(byte[] entry)
        {
            this.entry = entry;
            Create();
            Notify("Entry");
            Notify("EntryString");
        }

        public FTDSubEntryVM(byte[] entry)
        {
            this.entry = entry;
            encoding = Static.EncodingManager.GetPersonaEncoding(Settings.AppSetting.Default.FTDEncoding);
            Create();
            CreateContextMenu();
        }

        private void Create()
        {
            _Entry = "";
            for (int i = 0; i < entry.Length; i++)
            {
                _Entry += String.Format("{0:X2}", entry[i]);
                if (i + 1 == entry.Length)
                {
                    while ((i + 1) % 16 != 0)
                    {
                        _Entry += " 00";
                        i++;
                    }
                    continue;
                }

                if ((i + 1) % 16 == 0)
                    _Entry += Environment.NewLine;
                else
                    _Entry += " ";
            }
        }

        private void CreateContextMenu()
        {
            contextMenu = new ContextMenu();

            MenuItem menuItem = new MenuItem() { Header = "Copy" };
            menuItem.Click += MenuItem_Copy;
            contextMenu.Items.Add(menuItem);

            menuItem = new MenuItem() { Header = "Paste" };
            menuItem.Click += MenuItem_Paste;
            contextMenu.Items.Add(menuItem);
        }

        private void MenuItem_Paste(object sender, RoutedEventArgs e)
        {
            string text = Regex.Replace(Clipboard.GetText(), "\r\n|\r|\n", "");
            try
            {
                byte[] array = PersonaEditorLib.Utilities.String.SplitString(text, ' ');
                for(int i = 0;i< entry.Length; i++)
                {
                    if (i < array.Length)
                        entry[i] = array[i];
                    else
                        entry[i] = 0;
                }

                Create();
                Notify("Entry");
                Notify("EntryString");
            }
            catch { }
        }

        private void MenuItem_Copy(object sender, System.Windows.RoutedEventArgs e)
        {
            Clipboard.SetText(Regex.Replace(Entry, "\r\n|\r|\n", " "));
        }
    }

    class FTDEntryVM : BindingObject
    {
        byte[][] entry;
        Encoding encoding;

        public ObservableCollection<FTDSubEntryVM> SubItems { get; } = new ObservableCollection<FTDSubEntryVM>();

        private string _Entry = "";
        public string Entry => _Entry;

        public string EntryString => _Entry == "SubItems" ? "" : encoding == null ? "" : encoding.GetString(entry[0]);

        private ContextMenu contextMenu = null;
        public ContextMenu ContextMenu => contextMenu;

        public FTDEntryVM(byte[][] entry)
        {
            this.entry = entry;
            encoding = Static.EncodingManager.GetPersonaEncoding(Settings.AppSetting.Default.FTDEncoding);

            if (entry.Length == 1)
                Create();
            else
            {
                _Entry = "SubItems";
                foreach (var a in entry)
                    SubItems.Add(new FTDSubEntryVM(a));
            }

            CreateContextMenu();
        }

        public void SetEncoding(Encoding encoding)
        {
            this.encoding = encoding;
            Notify("EntryString");
            foreach (var a in SubItems)
                a.SetEncoding(encoding);
        }

        private void Create()
        {
            _Entry = "";
            for (int i = 0; i < entry[0].Length; i++)
            {
                _Entry += String.Format("{0:X2}", entry[0][i]);
                if (i + 1 == entry[0].Length)
                {
                    // while ((i + 1) % 16 != 0)
                    // {
                    //     _Entry += " 00";
                    //     i++;
                    // }
                    continue;
                }
                _Entry += " ";
            }
        }

        private void CreateContextMenu()
        {
            if (Entry == "SubItems")
            {
                contextMenu = new ContextMenu();

                MenuItem menuItem = new MenuItem() { Header = "Resize" };
                menuItem.Click += MenuItem_Resize;
                contextMenu.Items.Add(menuItem);
            }
            else
            {
                contextMenu = new ContextMenu();

                MenuItem menuItem = new MenuItem() { Header = "Copy" };
                menuItem.Click += MenuItem_Copy;
                contextMenu.Items.Add(menuItem);

                menuItem = new MenuItem() { Header = "Paste" };
                menuItem.Click += MenuItem_Paste;
                contextMenu.Items.Add(menuItem);
            }
        }

        private void MenuItem_Resize(object sender, RoutedEventArgs e)
        {
            ToolBox.Resize resize = new ToolBox.Resize();
            Visual visual = (Visual)sender;
            Point point = visual.PointToScreen(new Point(0, 0));
            resize.Size = entry[0].Length;
            resize.Left = point.X;
            resize.Top = point.Y;
            if (resize.ShowDialog() == true)
            {
                for (int i = 0; i < entry.Length; i++)
                {
                    Array.Resize(ref entry[i], resize.Size);
                    SubItems[i].Replace(entry[i]);
                }
            }
        }

        private void MenuItem_Paste(object sender, RoutedEventArgs e)
        {
            string text = Regex.Replace(Clipboard.GetText(), "\r\n|\r|\n", "");
            try
            {
                byte[] array = PersonaEditorLib.Utilities.String.SplitString(text, ' ');
                entry[0] = array;
                Create();
                Notify("Entry");
                Notify("EntryString");
            }
            catch { }
        }

        private void MenuItem_Copy(object sender, System.Windows.RoutedEventArgs e)
        {
            Clipboard.SetText(Entry);
        }
    }

    class FTDEditorVM : BindingObject, IViewModel
    {
        PersonaEditorLib.FileStructure.Text.FTD ftd;

        public ObservableCollection<FTDEntryVM> Entries { get; } = new ObservableCollection<FTDEntryVM>();

        public ReadOnlyObservableCollection<string> EncodingList { get; }

        private int selectEncodingIndex = 0;
        public int SelectEncodingIndex
        {
            get { return selectEncodingIndex; }
            set
            {
                selectEncodingIndex = value;
                Settings.AppSetting.Default.FTDEncoding = Static.EncodingManager.GetPersonaEncodingName(value);
                foreach (var a in Entries)
                    a.SetEncoding(Static.EncodingManager.GetPersonaEncoding(value));
            }
        }

        public FontFamily FontFamily { get; } = new FontFamily(System.Drawing.FontFamily.GenericMonospace.Name);

        public FTDEditorVM(PersonaEditorLib.FileStructure.Text.FTD ftd)
        {
            EncodingList = Static.EncodingManager.EncodingList;
            selectEncodingIndex = Static.EncodingManager.GetPersonaEncodingIndex(Settings.AppSetting.Default.FTDEncoding);
            this.ftd = ftd;
            foreach (var a in ftd.Entries)
                Entries.Add(new FTDEntryVM(a));
        }

        public bool Close()
        {
            return true;
        }
    }
}