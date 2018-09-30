using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using AuxiliaryLibraries.WPF;
using AuxiliaryLibraries.GameFormat;
using AuxiliaryLibraries.GameFormat.SpriteContainer;
using AuxiliaryLibraries.GameFormat.Sprite;
using AuxiliaryLibraries.WPF.Wrapper;

namespace PersonaEditor.ViewModels.Editors
{
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
}