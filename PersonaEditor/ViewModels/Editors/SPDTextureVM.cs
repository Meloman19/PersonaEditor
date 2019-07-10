using PersonaEditorLib;
using PersonaEditorLib.Sprite;
using PersonaEditorLib.SpriteContainer;
using AuxiliaryLibraries.WPF;
using AuxiliaryLibraries.WPF.Wrapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PersonaEditor.ViewModels.Editors
{
    class SPDTextureVM : BindingObject
    {
        GameFile texture;

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

        public SPDTextureVM(GameFile dds, IList<SPDKey> keylist, int index)
        {
            if (dds == null)
                throw new ArgumentNullException(nameof(dds));
            if (keylist == null)
                throw new ArgumentNullException(nameof(keylist));

            texture = dds;
            TextureImage = (dds.GameData as IImage).GetBitmap().GetBitmapSource();

            foreach (var a in keylist.Where(x => x.TextureIndex == index))
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
}