using AuxiliaryLibraries.GameFormat.SpriteContainer;
using AuxiliaryLibraries.WPF;
using PersonaEditor.Classes;
using System.Collections.ObjectModel;

namespace PersonaEditor.ViewModels.Editors
{
    class SPDEditorVM : BindingObject, IViewModel
    {
        public ObservableCollection<object> TextureList { get; set; } = new ObservableCollection<object>();

        public SPDEditorVM(SPD spd)
        {
            for (int i = 0; i < spd.SubFiles.Count; i++)
                TextureList.Add(new SPDTextureVM(spd.SubFiles[i], spd.KeyList, i));
        }

        public bool Close()
        {
            return true;
        }
    }
}