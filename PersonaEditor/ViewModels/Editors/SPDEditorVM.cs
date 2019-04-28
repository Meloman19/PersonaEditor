using PersonaEditorLib.SpriteContainer;
using AuxiliaryLibraries.WPF;
using PersonaEditor.Classes;
using System.Collections.ObjectModel;

namespace PersonaEditor.ViewModels.Editors
{
    class SPDEditorVM : BindingObject, IEditorViewModel
    {
        public ObservableCollection<object> TextureList { get; set; } = new ObservableCollection<object>();

        public SPDEditorVM(SPD spd)
        {
            for (int i = 0; i < spd.SubFiles.Count; i++)
                TextureList.Add(new SPDTextureVM(spd.SubFiles[i], spd.KeyList, (int)spd.SubFiles[i].Tag));
        }

        public bool Close()
        {
            return true;
        }
    }
}