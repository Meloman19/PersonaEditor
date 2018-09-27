using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using AuxiliaryLibraries.WPF;
using AuxiliaryLibraries.GameFormat;
using AuxiliaryLibraries.GameFormat.SpriteContainer;
using AuxiliaryLibraries.GameFormat.Sprite;
using PersonaEditor;
using AuxiliaryLibraries.WPF.Wrapper;

namespace PersonaEditorGUI.Controls.Editors
{
    class SPRKeyVM : BindingObject
    {
        SPRKey Key;

        private bool _IsSelected = false;

        public string Name
        {
            get { return Key.mComment; }
        }
        public int X1
        {
            get { return Key.X1; }
            set
            {
                if (value != Key.X1)
                {
                    Key.X1 = value;
                    Notify("X1");
                    Notify("Rect");
                }
            }
        }
        public int X2
        {
            get { return Key.X2; }
            set
            {
                if (value != Key.X2)
                {
                    Key.X2 = value;
                    Notify("X2");
                    Notify("Rect");
                }
            }
        }
        public int Y1
        {
            get { return Key.Y1; }
            set
            {
                if (value != Key.Y1)
                {
                    Key.Y1 = value;
                    Notify("Y1");
                    Notify("Rect");
                }
            }
        }
        public int Y2
        {
            get { return Key.Y2; }
            set
            {
                if (value != Key.Y2)
                {
                    Key.Y2 = value;
                    Notify("Y2");
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

        public SPRKeyVM(SPRKey key)
        {
            Key = key ?? throw new ArgumentNullException("key");
        }
    }

    class SPRTextureVM : BindingObject
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

        public SPRTextureVM(ObjectContainer tmx, IList<SPRKey> keylist, int textureindex)
        {
            texture = tmx ?? throw new ArgumentNullException("tmx");
            if (texture.Object == null) throw new ArgumentNullException("tmx.Object");
            var list = (keylist ?? throw new ArgumentNullException("keylist")).Where(x => x.mTextureIndex == textureindex);

            TextureImage = (tmx.Object as TMX).GetBitmap().GetBitmapSource();

            foreach (var a in list)
                KeyList.Add(new SPRKeyVM(a));
        }

        #region PublicProperties

        public ObservableCollection<object> KeyList { get; } = new ObservableCollection<object>();

        public DrawingCollection Drawings { get; } = new DrawingCollection();

        public string Name
        {
            get
            {
                if (texture.Object is TMX tmx)
                    return tmx.Comment;
                else return texture.Name;
            }
        }

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
                if (_SelectedItem is SPRKeyVM it)
                    it.IsSelected = false;
                _SelectedItem = value;
                if (_SelectedItem is SPRKeyVM it2)
                    it2.IsSelected = true;
            }
        }

        #endregion PublicProperties
    }

    class SPREditorVM : BindingObject, IViewModel
    {
        public ObservableCollection<object> TextureList { get; set; } = new ObservableCollection<object>();

        public SPREditorVM(SPR spr)
        {
            for (int i = 0; i < spr.SubFiles.Count; i++)
                TextureList.Add(new SPRTextureVM(spr.SubFiles[i], spr.KeyList.List, i));
        }

        public bool Close()
        {
            return true;
        }
    }
}