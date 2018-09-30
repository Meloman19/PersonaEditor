using System.Collections.ObjectModel;
using AuxiliaryLibraries.WPF;
using AuxiliaryLibraries.GameFormat.SpriteContainer;
using PersonaEditor.Classes;

namespace PersonaEditor.ViewModels.Editors
{
    class SPREditorVM : BindingObject, IViewModel
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