using PersonaEditorLib;
using AuxiliaryLibraries.WPF;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace PersonaEditor.ViewModels.Tools
{
    class SetCharVM : BindingObject
    {
        public class FnMpImg : BindingObject
        {
            public int Index { get; set; } = 0;

            private string _Char = "";
            public string Char
            {
                get { return _Char; }
                set
                {
                    if (_Char != value)
                    {
                        _Char = value;
                        Notify("Char");
                    }
                }
            }

            private BitmapSource _Image;
            public BitmapSource Image
            {
                get { return _Image; }
                set
                {
                    _Image = value;
                    Notify("Image");
                }
            }
        }

        private int _FontSelect = -1;
        public ReadOnlyObservableCollection<string> FontList => Static.FontManager.FontList;

        public int FontSelect
        {
            get { return _FontSelect; }
            set
            {
                if (Save())
                {
                    _FontSelect = value;
                    GlyphListUpdate();
                }
                else { Notify("FontSelect"); }
            }
        }

        public BindingList<FnMpImg> GlyphList { get; } = new BindingList<FnMpImg>();

        public SetCharVM()
        {
            Closing = new RelayCommand(Window_Closing);
            GlyphList.ListChanged += GlyphList_ListChanged;
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
            var font = Static.FontManager.GetPersonaFont(_FontSelect);
            if (font != null)
                foreach (var a in font.DataList)
                {
                    var pallete = new BitmapPalette(font.Palette.Select(x => System.Windows.Media.Color.FromArgb(x.A, x.R, x.G, x.B)).ToArray());
                    var form = AuxiliaryLibraries.WPF.Wrapper.Imaging.AuxToWPF(font.PixelFormat);
                    var temp = new FnMpImg()
                    {
                        Index = a.Key,
                        Image = BitmapSource.Create(font.Width, font.Height, 96, 96, form, pallete, a.Value, (font.PixelFormat.BitsPerPixel * font.Width + 7) / 8)
                    };
                    GlyphList.Add(temp);
                }
            var enc = Static.EncodingManager.GetPersonaEncoding(Static.FontManager.GetPersonaFontName(_FontSelect));

            foreach (var a in GlyphList)
                if (enc.Dictionary.ContainsKey(a.Index))
                    a.Char = enc.Dictionary[a.Index].ToString();

            GlyphList.ListChanged += GlyphList_ListChanged;
        }

        bool Save()
        {
            if (IsChanged)
            {
                var result = MessageBox.Show("Save changed?", "Save", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);

                if (result == MessageBoxResult.Yes)
                {
                    var enc = Static.EncodingManager.GetPersonaEncoding(Static.FontManager.GetPersonaFontName(_FontSelect));
                    if (enc.Tag == "Empty")
                    {
                        PersonaEncoding personaEncoding = new PersonaEncoding();
                        foreach (var a in GlyphList)
                            if (a.Char.Length > 0)
                                personaEncoding.Add(a.Index, a.Char[0]);

                        personaEncoding.SaveFNTMAP(Path.Combine(Static.FontManager.sourcedir, Static.FontManager.GetPersonaFontName(_FontSelect) + ".FNTMAP"));
                    }
                    else
                    {
                        foreach (var a in GlyphList)
                            if (a.Char.Length > 0)
                                enc.Add(a.Index, a.Char[0]);
                        enc.SaveFNTMAP(enc.FilePath);
                        Static.EncodingManager.Update(Static.FontManager.GetPersonaFontName(_FontSelect));
                    }
                }
                else if (result == MessageBoxResult.Cancel)
                    return false;

                IsChanged = false;
                return true;
            }
            else return true;
        }

        public ICommand Closing { get; }

        void Window_Closing(object arg)
        {
            if (!Save())
                (arg as CancelEventArgs).Cancel = true;
        }
    }
}