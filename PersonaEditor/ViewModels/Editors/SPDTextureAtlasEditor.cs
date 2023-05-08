using PersonaEditorLib.SpriteContainer;
using System.Linq;

namespace PersonaEditor.ViewModels.Editors
{
    class SPDTextureAtlasEditor : TextureAtlasEditorBase<SPDTextureAtlas, SPDTextureObject>
    {
        public SPDTextureAtlasEditor(SPD spd)
        {
            if (spd == null)
                throw new System.ArgumentNullException(nameof(spd));

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