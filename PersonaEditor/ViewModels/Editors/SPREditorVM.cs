using System.Collections.ObjectModel;
using AuxiliaryLibraries.WPF;
using PersonaEditorLib.SpriteContainer;
using PersonaEditor.Classes;

namespace PersonaEditor.ViewModels.Editors
{
    class SPREditorVM : BindingObject, IEditorViewModel
    {
        public ObservableCollection<object> TextureList { get; set; } = new ObservableCollection<object>();

        public SPREditorVM(SPR spr)
        {
            for (int i = 0; i < spr.SubFiles.Count; i++)
                TextureList.Add(new SPRTextureVM(spr.SubFiles[i], spr.KeyList.List, i));
        }

        public bool Close()
        {
            return true;
        }
    }
}