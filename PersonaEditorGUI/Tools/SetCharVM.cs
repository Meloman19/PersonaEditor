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

        private int _FontSelect = 0;
        public ReadOnlyObservableCollection<string> FontList => CharList.FontList;

        public int FontSelect
        {
            get { return _FontSelect; }
            set
            {
                if (Save())
                {
                    _FontSelect = value;
                    string font = Path.Combine(Static.Paths.DirFont, FontList[_FontSelect]);
                    string fontmp = Path.Combine(Static.Paths.DirFont, Path.GetFileNameWithoutExtension(FontList[_FontSelect]) + ".txt");
                    CharList.Open(font, fontmp);
                }
                else { Notify("FontSelect"); }
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
            CharList.GetFontList(Static.Paths.DirFont);
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
                    CharList.SaveFontMap(Path.Combine(Static.Paths.DirFont, Path.GetFileNameWithoutExtension(FontList[_FontSelect]) + ".txt"));
                }
                else if (result == MessageBoxResult.Cancel)
                    return false;

                IsChanged = false;
                return true;
            }
            else return true;
        }

        public CancelEventHandler Closing => Window_Closing;

        void Window_Closing(object sender, CancelEventArgs e)
        {
            Save();
        }
    }
}