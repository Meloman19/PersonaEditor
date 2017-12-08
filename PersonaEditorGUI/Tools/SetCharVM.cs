using PersonaEditorLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PersonaEditorGUI.Tools
{
    class SetCharGlyphVM : BindingObject
    {

    }

    class SetCharVM : BindingObject
    {
        readonly CharList CharList = new CharList();
        EventWrapper CharListEW;

        string Dir;

        private int _FontSelect = 0;
        public ObservableCollection<string> FontList { get; } = new ObservableCollection<string>();
        public int FontSelect
        {
            get { return _FontSelect; }
            set
            {
                if (Save())
                {
                    _FontSelect = value;
                    string font = Path.Combine(Dir, FontList[_FontSelect]);
                    string fontmp = Path.Combine(Dir, Path.GetFileNameWithoutExtension(FontList[_FontSelect]) + ".txt");
                    CharList.Open(font, fontmp);
                }
                else { Notify("FontSelect"); }
            }
        }

        private void LoadFontList(string dir = "")
        {
            Dir = Path.GetFullPath(dir);
            if (Directory.Exists(Dir))
            {
                DirectoryInfo DI = new DirectoryInfo(Dir);
                foreach (var file in DI.GetFiles(@"*.fnt"))
                    FontList.Add(file.Name);
            }
        }

        public override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is CharList chr)
            {
                GlyphListUpdate();
            }
        }

        public BindingList<CharList.FnMpImg> GlyphList { get; } = new BindingList<CharList.FnMpImg>();

        public SetCharVM()
        {
            GlyphList.ListChanged += GlyphList_ListChanged;
            CharListEW = new EventWrapper(CharList, this);
            LoadFontList(Static.Paths.DirFont);
            FontSelect = 0;
        }

        private void GlyphList_ListChanged(object sender, ListChangedEventArgs e)
        {
            IsChanged = true;
            GlyphList.ListChanged -= GlyphList_ListChanged;
        }

        bool IsChanged = false;

        private void GlyphListUpdate()
        {
            GlyphList.ListChanged -= GlyphList_ListChanged;
            GlyphList.Clear();
            foreach (var a in CharList.List)
            {
                var temp = new CharList.FnMpImg()
                {
                    Index = a.Index,
                    Char = a.Char,
                    Image = BitmapSource.Create(CharList.Width, CharList.Height, 96, 96, CharList.PixelFormat, CharList.Palette,
                    a.Image_data, (CharList.PixelFormat.BitsPerPixel * CharList.Width + 7) / 8)
                };
                GlyphList.Add(temp);
            }
            GlyphList.ListChanged += GlyphList_ListChanged;
        }

        bool Save()
        {
            if (IsChanged)
            {
                var result = MessageBox.Show("Save changed?", "Save", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);

                if (result == MessageBoxResult.Yes)
                {
                    for (int i = 0; i < GlyphList.Count; i++)
                        CharList.List[i].Char = GlyphList[i].Char;
                    CharList.SaveFontMap(Path.Combine(Dir, Path.GetFileNameWithoutExtension(FontList[_FontSelect]) + ".txt"));
                }
                else if (result == MessageBoxResult.Cancel)
                    return false;

                IsChanged = false;
                return true;
            }
            else return true;
        }
    }
}