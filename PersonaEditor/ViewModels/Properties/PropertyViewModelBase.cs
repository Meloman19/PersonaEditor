using PersonaEditor.Common;

namespace PersonaEditor.ViewModels.Properties
{
    public abstract class PropertyViewModelBase : BindingObject
    {
        private string _propertyName;
        private bool _propertyValueChanged = false;

        public string PropertyName
        {
            get => _propertyName;
            set => SetProperty(ref _propertyName, value);
        }

        public bool PropertyValueChanged
        {
            get => _propertyValueChanged;
            set => SetProperty(ref _propertyValueChanged, value);
        }

        public abstract string PropertyReadOnlyTextValue { get; }

        public abstract void SaveChanges();
    }
}