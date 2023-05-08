using PersonaEditorLib.SpriteContainer;
using System.Linq;

namespace PersonaEditor.ViewModels.Editors
{
    class SPRTextureAtlasEditor : TextureAtlasEditorBase<SPRTextureAtlas, SPRTextureObject>
    {
        public SPRTextureAtlasEditor(SPR spr)
        {
            if (spr == null)
                throw new System.ArgumentNullException(nameof(spr));

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