using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using PersonaEditor.Common;

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
            if (TextureAtlasList.Any(x => x.HasAnyChanges()))
            {
                var result = MessageBox.Show("Save changes?", "Saving", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    foreach (var item in TextureAtlasList)
                        item.SaveChanges();
                }
                else if (result == MessageBoxResult.No)
                    return true;
                else
                    return false;
            }

            return true;
        }
    }
}