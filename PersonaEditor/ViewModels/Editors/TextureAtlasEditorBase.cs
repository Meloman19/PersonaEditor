using PersonaEditor.Common;
using System.Collections.ObjectModel;

namespace PersonaEditor.ViewModels.Editors
{
    public abstract class TextureAtlasEditorBase<T, R> : BindingObject, IEditor
        where T : TextureAtlasBase<R>
        where R : TextureObjectBase
    {
        private T _selectedTextureAtlas;

        public ObservableCollection<T> TextureAtlasList { get; } = new ObservableCollection<T>();

        public T SelectedTextureAtlas
        {
            get => _selectedTextureAtlas;
            set => SetProperty(ref _selectedTextureAtlas, value);
        }

        public bool Close()
        {
            return true;
        }
    }
}