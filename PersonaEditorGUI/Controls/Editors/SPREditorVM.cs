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

namespace PersonaEditorGUI.Controls.Editors
{
    class SPRKeyVM : BindingObject
    {
        PersonaEditorLib.FileStructure.SPR.SPRKey Key;
        EventWrapper EW;
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
            EW = new EventWrapper(key, this);
        }
    }

    class TMXVM : BindingObject
    {
        ObjectFile tmx;
        EventWrapper EW;

        private BitmapSource _TextureImage = null;
        private Rect _Rect;
        private Point _Point;

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

        public TMXVM(ObjectFile tmx, IList<PersonaEditorLib.FileStructure.SPR.SPRKey> keylist, int textureindex)
        {
            this.tmx = tmx ?? throw new ArgumentNullException("tmx");
            if (tmx.Object == null) throw new ArgumentNullException("tmx.Object");
            var list = (keylist ?? throw new ArgumentNullException("keylist")).Where(x => x.mTextureIndex == textureindex);


            TextureImage = (tmx.Object as PersonaEditorLib.FileStructure.Graphic.TMX).GetImage();
            EW = new EventWrapper(tmx, this);

            foreach (var a in list)
                KeyList.Add(new SPRKeyVM(a));
        }

        #region PublicProperties

        public ObservableCollection<SPRKeyVM> KeyList { get; } = new ObservableCollection<SPRKeyVM>();

        public string Name => (tmx.Object as PersonaEditorLib.FileStructure.Graphic.TMX).Name;

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

        public Point Point
        {
            get { return _Point; }
            set
            {
                _Point = new Point(Math.Floor(value.X * Rect.Width), (Math.Floor(value.Y * Rect.Height)));
                Notify("Point");
            }
        }

        #endregion PublicProperties
    }

    class SPREditorVM : BindingObject, IViewModel
    {
        public ObservableCollection<TMXVM> TextureList { get; set; } = new ObservableCollection<TMXVM>();

        public SPREditorVM(PersonaEditorLib.FileStructure.SPR.SPR spr)
        {
            for (int i = 0; i < spr.TextureList.Count; i++)
            {
                TextureList.Add(new TMXVM(spr.TextureList[i], spr.KeyList.List, i));
            }
        }

        public bool Close()
        {
            return true;
        }
    }
}