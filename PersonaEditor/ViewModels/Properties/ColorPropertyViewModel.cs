using System;
using System.Windows.Media;

namespace PersonaEditor.ViewModels.Properties
{
    public sealed class ColorPropertyViewModel : PropertyViewModelBase
    {
        private Color? _propertyValue;

        public Color? PropertyValue
        {
            get => _propertyValue;
            set
            {
                if (SetProperty(ref _propertyValue, value))
                {
                    PropertyValueChanged = true;
                    Notify(nameof(PropertyReadOnlyTextValue));
                }
            }
        }

        public Action<Color?> SaveDelegate { get; set; }

        public override string PropertyReadOnlyTextValue => PropertyValue?.ToString() ?? string.Empty;

        public override void SaveChanges()
        {
            if (PropertyValueChanged)
                SaveDelegate?.Invoke(PropertyValue);
        }
    }
}