using System;
using System.Linq;
using PersonaEditorLib;
using PersonaEditorLib.SpriteContainer;

namespace PersonaEditor.ViewModels.Editors
{
    public sealed class SPRTextureAtlasEditor : TextureAtlasEditorBase<SPRTextureAtlas, SPRTextureObject>
    {
        private readonly GameFile _sprGameFile;

        public SPRTextureAtlasEditor(GameFile sprGameFile)
        {
            ArgumentNullException.ThrowIfNull(sprGameFile);
            _sprGameFile = sprGameFile;

            var spr = _sprGameFile.GameData as SPR;
            for (int i = 0; i < spr.SubFiles.Count; i++)
            {
                var textureIndex = i;
                var textureKeys = spr.KeyList.List.Where(x => x.mTextureIndex == textureIndex).ToArray();
                var textureObject = new SPRTextureAtlas(spr.SubFiles[i], textureKeys);
                TextureAtlasList.Add(textureObject);
            }

            SelectedTextureAtlas = TextureAtlasList.FirstOrDefault();
        }
    }
}