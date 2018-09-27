using AuxiliaryLibraries.GameFormat;
using AuxiliaryLibraries.GameFormat.Sprite;
using AuxiliaryLibraries.GameFormat.SpriteContainer;
using AuxiliaryLibraries.WPF;
using AuxiliaryLibraries.WPF.Wrapper;
using PersonaEditor;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PersonaEditorGUI.Controls.Editors
{
    class SPDKeyVM : BindingObject
    {
        SPDKey Key;

        private bool _IsSelected = false;

        public string Name => Encoding.GetEncoding("shift-jis").GetString(Key.Comment.Where(x => x != 0x00).ToArray());

        public int X1
        {
            get { return Key.X0; }
            set
            {
                if (value != Key.X0)
                {
                    Key.X0 = value;
                    Notify("X1"); Notify("X2");
                    Notify("Rect");
                }
            }
        }
        public int X2
        {
            get { return Key.X0 + Key.Xdel; }
            set
            {
                if (value != Key.X0 + Key.Xdel)
                {
                    Key.Xdel = value - Key.X0;
                    Notify("X1"); Notify("X2");
                    Notify("Rect");
                }
            }
        }
        public int Y1
        {
            get { return Key.Y0; }
            set
            {
                if (value != Key.Y0)
                {
                    Key.Y0 = value;
                    Notify("Y1"); Notify("Y2");
                    Notify("Rect");
                }
            }
        }
        public int Y2
        {
            get { return Key.Y0 + Key.Ydel; }
            set
            {
                if (value != Key.Y0 + Key.Ydel)
                {
                    Key.Ydel = value - Key.Y0;
                    Notify("Y1"); Notify("Y2");
                    Notify("Rect");
                }
            }
        }

        public Rect Rect
        {
            get { return new Rect(new Point(X1, Y1), new Point(X2, Y2)); }
        }

        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                if (_IsSelected != value)
                {
                    _IsSelected = value;
                    Notify("IsSelected");
                }
            }
        }

        public SPDKeyVM(SPDKey key)
        {
            Key = key ?? throw new ArgumentNullException("key");
        }
    }

    class SPDTextureVM : BindingObject
    {
        ObjectContainer texture;

        private BitmapSource _TextureImage = null;
        private Rect _Rect;
        private object _SelectedItem = null;

        public override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Image")
            {
                if (sender is TMX tmx)
                {
                    TextureImage = tmx.GetBitmap().GetBitmapSource();
                }
            }
        }

        public SPDTextureVM(ObjectContainer dds, IList<SPDKey> keylist, int index)
        {
            texture = dds ?? throw new ArgumentNullException("dds");
            if (texture.Object == null) throw new ArgumentNullException("dds.Object");
            var list = (keylist ?? throw new Exception("keylist")).Where(x => x.TextureIndex == index);

            TextureImage = (dds.Object as DDS).GetBitmap().GetBitmapSource();

            foreach (var a in list)
                KeyList.Add(new SPDKeyVM(a));
        }

        #region PublicProperties

        public ObservableCollection<object> KeyList { get; } = new ObservableCollection<object>();

        public DrawingCollection Drawings { get; } = new DrawingCollection();

        public string Name => texture.Name;

        public BitmapSource TextureImage
        {
            get { return _TextureImage; }
            private set
            {
                _TextureImage = value;
                _Rect = new Rect(0, 0, _TextureImage.Width, _TextureImage.Height);
                Notify("Rect");
                Notify("TextureImage");
            }
        }

        public Rect Rect => _Rect;

        public object SelectedItem
        {
            set
            {
                if (_SelectedItem is SPDKeyVM itt)
                    itt.IsSelected = false;
                _SelectedItem = value;
                if (_SelectedItem is SPDKeyVM itt2)
                    itt2.IsSelected = true;
            }
        }

        #endregion PublicProperties
    }

    class SPDEditorVM : BindingObject, IViewModel
    {
        public ObservableCollection<object> TextureList { get; set; } = new ObservableCollection<object>();

        public SPDEditorVM(SPD spd)
        {
            for (int i = 0; i < spd.SubFiles.Count; i++)
                TextureList.Add(new SPDTextureVM(spd.SubFiles[i], spd.KeyList, i));
        }

        public bool Close()
        {
            return true;
        }
    }
}