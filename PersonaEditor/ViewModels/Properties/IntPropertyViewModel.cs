using System;

namespace PersonaEditor.ViewModels.Properties
{
    public sealed class IntPropertyViewModel : PropertyViewModelBase
    {
        private int? _propertyValue;
        private int _propertyMinValue = int.MinValue;
        private int _propertyMaxValue = int.MaxValue;

        public int? PropertyValue
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

        public int PropertyMinValue
        {
            get => _propertyMinValue;
            set => SetProperty(ref _propertyMinValue, value);
        }

        public int PropertyMaxValue
        {
            get => _propertyMaxValue;
            set => SetProperty(ref _propertyMaxValue, value);
        }

        public Action<int?> SaveDelegate { get; set; }

        public override string PropertyReadOnlyTextValue => PropertyValue?.ToString() ?? string.Empty;

        public override void SaveChanges()
        {
            if (PropertyValueChanged)
                SaveDelegate?.Invoke(PropertyValue);
        }
    }
}