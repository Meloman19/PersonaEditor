using AuxiliaryLibraries.WPF.Wrapper;
using PersonaEditorLib;
using PersonaEditorLib.SpriteContainer;
using System;
using System.Linq;
using System.Windows.Media.Imaging;

namespace PersonaEditor.ViewModels.Editors
{
    public sealed class SPDTextureAtlas : TextureAtlasBase<SPDTextureObject>
    {
        private readonly GameFile _texture;

        public SPDTextureAtlas(GameFile dds, SPDKey[] keylist)
        {
            if (dds == null)
                throw new ArgumentNullException(nameof(dds));
            if (keylist == null)
                throw new ArgumentNullException(nameof(keylist));

            _texture = dds;
            TextureImage = (dds.GameData as IImage).GetBitmap().GetBitmapSource();

            foreach (var key in keylist)
                Objects.Add(new SPDTextureObject(key));

            SelectedObject = Objects.FirstOrDefault();
        }

        public override BitmapSource TextureImage { get; }

        public override string Name => _texture.Name;
    }
}