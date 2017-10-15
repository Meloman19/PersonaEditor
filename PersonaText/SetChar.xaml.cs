using PersonaEditorLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PersonaText
{
    public partial class SetChar : Window
    {
        BindingList<CharList.FnMpImg> CurrentCL = new BindingList<CharList.FnMpImg>();
        CharList CL;
        bool ListChanged = false;

        public SetChar()
        {
            InitializeComponent();
            wrap.DataContext = CurrentCL;
        }

        private void CurrentCL_ListChanged(object sender, ListChangedEventArgs e)
        {
            ListChanged = true;
            CurrentCL.ListChanged -= CurrentCL_ListChanged;
        }

        public void Open(CharList CharList)
        {
            CL = CharList;

            foreach (var a in CharList.List)
            {
                CurrentCL.Add(new CharList.FnMpImg()
                {
                    Index = a.Index,
                    Char = a.Char,
                    Image = BitmapSource.Create(CharList.Width, CharList.Height, 96, 96, CharList.PixelFormat, CharList.Palette,
                    a.Image_data, (CharList.PixelFormat.BitsPerPixel * CharList.Width + 7) / 8)
                });
            }

            CurrentCL.ListChanged += CurrentCL_ListChanged;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (ListChanged)
            {
                if (MessageBox.Show("Font's map will be changed.\nSave?", "Font's map changed", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                {
                    foreach (var a in CurrentCL)
                    {
                        var elem = CL.List.Find(x => x.Index == a.Index);
                        if (elem != null) elem.Char = a.Char;
                    }
                    CL.SaveFontMap(Path.Combine(Static.Paths.CurrentFolderEXE, "FONT_" + CL.Tag.ToUpper() + ".TXT"));
                }
            }
            CL = null;
            CurrentCL.Clear();
        }
    }
}