using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PersonaEditorLib;
using System.Windows;
using PersonaEditorLib.Interfaces;
using System.Windows.Input;

namespace PersonaEditorGUI.Controls.Editors
{
    class SPRKeyVM : BindingObject
    {
        PersonaEditorLib.FileStructure.SPR.SPRKey Key;

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

        public SPRKeyVM(PersonaEditorLib.FileStructure.SPR.SPRKey key)
        {
            Key = key ?? throw new ArgumentNullException("key");
        }
    }

    class SPDKeyVM : BindingObject
    {
        PersonaEditorLib.FileStructure.SPR.SPDKey Key;

        private bool _IsSelected = false;

        public string Name
        {
            get { return Encoding.GetEncoding("shift-jis").GetString(Key.Comment.Where(x => x != 0x00).ToArray()); }
        }
        public int X1
        {
            get { return Key.X0; }
            set
            {
                if (value != Key.X0)
                {
                    Key.X0 = value;
                    Notify("X1");
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
                    Notify("X2");
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
                    Notify("Y1");
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

        public SPDKeyVM(PersonaEditorLib.FileStructure.SPR.SPDKey key)
        {
            Key = key ?? throw new ArgumentNullException("key");
        }
    }

    class TextureVM : BindingObject
    {
        ObjectFile texture;

        private BitmapSource _TextureImage = null;
        private Rect _Rect;
        private object _SelectedItem = null;

        public override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Image")
            {
                if (sender is PersonaEditorLib.FileStructure.Graphic.TMX tmx)
                {
                    TextureImage = tmx.GetImage();
                }
            }
        }

        public TextureVM(ObjectFile tmx, IList<PersonaEditorLib.FileStructure.SPR.SPRKey> keylist, int textureindex)
        {
            texture = tmx ?? throw new ArgumentNullException("tmx");
            if (texture.Object == null) throw new ArgumentNullException("tmx.Object");
            var list = (keylist ?? throw new ArgumentNullException("keylist")).Where(x => x.mTextureIndex == textureindex);

            TextureImage = (tmx.Object as PersonaEditorLib.FileStructure.Graphic.TMX).GetImage();

            foreach (var a in list)
                KeyList.Add(new SPRKeyVM(a));
        }

        public TextureVM(ObjectFile dds, IList<PersonaEditorLib.FileStructure.SPR.SPDKey> keylist, int index)
        {
            texture = dds ?? throw new ArgumentNullException("dds");
            if (texture.Object == null) throw new ArgumentNullException("dds.Object");
            var list = (keylist ?? throw new Exception("keylist")).Where(x => x.TextureIndex == index);

            TextureImage = (dds.Object as PersonaEditorLib.FileStructure.Graphic.DDS).GetImage();

            foreach (var a in list)
                KeyList.Add(new SPDKeyVM(a));
        }

        #region PublicProperties

        public ObservableCollection<object> KeyList { get; } = new ObservableCollection<object>();

        public DrawingCollection Drawings { get; } = new DrawingCollection();

        public string Name
        {
            get
            {
                if (texture.Object is PersonaEditorLib.FileStructure.Graphic.TMX tmx)
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
                else if (_SelectedItem is SPDKeyVM itt)
                    itt.IsSelected = false;
                _SelectedItem = value;
                if (_SelectedItem is SPRKeyVM it2)
                    it2.IsSelected = true;
                else if (_SelectedItem is SPDKeyVM itt2)
                    itt2.IsSelected = true;
            }
        }

        #endregion PublicProperties
    }

    class SPREditorVM : BindingObject, IViewModel
    {
        public ObservableCollection<TextureVM> TextureList { get; set; } = new ObservableCollection<TextureVM>();

        public SPREditorVM(PersonaEditorLib.FileStructure.SPR.SPR spr)
        {
            for (int i = 0; i < spr.SubFiles.Count; i++)
                TextureList.Add(new TextureVM(spr.SubFiles[i], spr.KeyList.List, i));
        }

        public SPREditorVM(PersonaEditorLib.FileStructure.SPR.SPD spd)
        {
            for (int i = 0; i < spd.SubFiles.Count; i++)
                TextureList.Add(new TextureVM(spd.SubFiles[i], spd.KeyList, i+1));
        }

        public bool Close()
        {
            return true;
        }
    }
}