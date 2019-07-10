using PersonaEditorLib.SpriteContainer;
using AuxiliaryLibraries.WPF;
using PersonaEditor.Classes;
using System.Collections.ObjectModel;

namespace PersonaEditor.ViewModels.Editors
{
    class SPDEditorVM : BindingObject, IEditor
    {
        public ObservableCollection<SPDTextureVM> TextureList { get; } = new ObservableCollection<SPDTextureVM>();

        public SPDEditorVM(SPD spd)
        {
            if (spd == null)
                throw new System.ArgumentNullException(nameof(spd));

            for (int i = 0; i < spd.SubFiles.Count; i++)
                TextureList.Add(new SPDTextureVM(spd.SubFiles[i], spd.KeyList, (int)spd.SubFiles[i].Tag));
        }

        public bool Close()
        {
            return true;
        }
    }
}