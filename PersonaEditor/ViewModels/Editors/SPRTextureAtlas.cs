using AuxiliaryLibraries.WPF.Wrapper;
using PersonaEditorLib;
using PersonaEditorLib.Sprite;
using PersonaEditorLib.SpriteContainer;
using System;
using System.Linq;
using System.Windows.Media.Imaging;

namespace PersonaEditor.ViewModels.Editors
{
    public sealed class SPRTextureAtlas : TextureAtlasBase<SPRTextureObject>
    {
        private readonly GameFile texture;

        public SPRTextureAtlas(GameFile tmx, SPRKey[] keylist)
        {
            if (tmx == null)
                throw new ArgumentNullException(nameof(tmx));
            if (keylist == null)
                throw new ArgumentNullException(nameof(keylist));

            texture = tmx;
            TextureImage = (tmx.GameData as IImage).GetBitmap().GetBitmapSource();

            foreach (var a in keylist)
                Objects.Add(new SPRTextureObject(a));

            SelectedObject = Objects.FirstOrDefault();
        }

        public override BitmapSource TextureImage { get; }

        public override string Name => texture.GameData is TMX tmx ? tmx.Comment : texture.Name;
    }
}