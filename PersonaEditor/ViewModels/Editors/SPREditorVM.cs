using System.Collections.ObjectModel;
using AuxiliaryLibraries.WPF;
using PersonaEditorLib.SpriteContainer;
using PersonaEditor.Classes;

namespace PersonaEditor.ViewModels.Editors
{
    class SPREditorVM : BindingObject, IEditor
    {
        public ObservableCollection<SPRTextureVM> TextureList { get; } = new ObservableCollection<SPRTextureVM>();

        public SPREditorVM(SPR spr)
        {
            if (spr == null)
                throw new System.ArgumentNullException(nameof(spr));

            for (int i = 0; i < spr.SubFiles.Count; i++)
                TextureList.Add(new SPRTextureVM(spr.SubFiles[i], spr.KeyList.List, i));
        }

        public bool Close()
        {
            return true;
        }
    }
}