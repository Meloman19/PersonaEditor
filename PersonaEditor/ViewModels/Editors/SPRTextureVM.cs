using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Windows;
using AuxiliaryLibraries.WPF;
using PersonaEditorLib;
using PersonaEditorLib.SpriteContainer;
using PersonaEditorLib.Sprite;
using AuxiliaryLibraries.WPF.Wrapper;

namespace PersonaEditor.ViewModels.Editors
{
    class SPRTextureVM : BindingObject
    {
        GameFile texture;

        private BitmapSource _TextureImage = null;
        private Rect _Rect;
        private object _SelectedItem = null;

        public SPRTextureVM(GameFile tmx, IList<SPRKey> keylist, int textureindex)
        {
            if (tmx == null)
                throw new ArgumentNullException(nameof(tmx));
            if (keylist == null)
                throw new ArgumentNullException(nameof(keylist));

            texture = tmx;
            TextureImage = (tmx.GameData as IImage).GetBitmap().GetBitmapSource();

            foreach (var a in keylist.Where(x => x.mTextureIndex == textureindex))
                KeyList.Add(new SPRKeyVM(a));
        }

        #region PublicProperties

        public ObservableCollection<SPRKeyVM> KeyList { get; } = new ObservableCollection<SPRKeyVM>();

        public string Name => texture.GameData is TMX tmx ? tmx.Comment : texture.Name;

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