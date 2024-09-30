using System;
using System.Linq;
using PersonaEditorLib;
using PersonaEditorLib.SpriteContainer;

namespace PersonaEditor.ViewModels.Editors
{
    public sealed class SPDTextureAtlasEditor : TextureAtlasEditorBase<SPDTextureAtlas, SPDTextureObject>
    {
        private readonly GameFile _spdGameFile;

        public SPDTextureAtlasEditor(GameFile spdGameFile)
        {
            ArgumentNullException.ThrowIfNull(spdGameFile);
            _spdGameFile = spdGameFile;

            var spd = _spdGameFile.GameData as SPD;
            foreach (var subFile in spd.SubFiles)
            {
                var textureIndex = (int)subFile.Tag;
                var textureKeys = spd.KeyList.Where(x => x.TextureIndex == textureIndex).ToArray();
                var textureObject = new SPDTextureAtlas(subFile, textureKeys);
                TextureAtlasList.Add(textureObject);
            }

            SelectedTextureAtlas = TextureAtlasList.FirstOrDefault();
        }
    }
}