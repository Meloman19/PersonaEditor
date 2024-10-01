using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using PersonaEditor.Common;

namespace PersonaEditor.ViewModels.Editors
{
    public abstract class TextureAtlasBase<T> : BindingObject
        where T : TextureObjectBase
    {
        private T _selectedObject;

        public ObservableCollection<T> Objects { get; } = new ObservableCollection<T>();

        public T SelectedObject
        {
            get => _selectedObject;
            set
            {
                var oldVal = _selectedObject;
                if (SetProperty(ref _selectedObject, value))
                {
                    if (oldVal != null)
                        oldVal.IsSelected = false;
                    if (_selectedObject != null)
                        _selectedObject.IsSelected = true;
                }
            }
        }

        public abstract BitmapSource TextureImage { get; }

        public Rect Rect => TextureImage == null
            ? new Rect()
            : new Rect(0, 0, TextureImage.PixelWidth, TextureImage.PixelHeight);

        public abstract string Name { get; }

        public bool HasAnyChanges() => Objects.Any(x => x.HasAnyChanges());

        public void SaveChanges()
        {
            foreach (var obj in Objects)
                obj.SaveChanges();
        }
    }
}