using PersonaEditor.Common;
using System.Windows;

namespace PersonaEditor.ViewModels.Editors
{
    public abstract class TextureObjectBase : BindingObject
    {
        private bool _isSelected = false;

        public abstract int X { get; set; }

        public abstract int Y { get; set; }

        public abstract int Width { get; set; }

        public abstract int Height { get; set; }
        
        public abstract int Red { get; set; }

        public abstract int Green { get; set; }

        public abstract int Blue { get; set; }

        public abstract int Alpha { get; set; }
        
        public string Name { get; protected set; }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}