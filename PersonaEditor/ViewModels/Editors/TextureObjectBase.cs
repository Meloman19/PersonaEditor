using PersonaEditor.Common;
using PersonaEditor.ViewModels.Properties;
using System;
using System.Windows;

namespace PersonaEditor.ViewModels.Editors
{
    public abstract class TextureObjectBase : BindingObject
    {
        private bool _isSelected = false;

        protected TextureObjectBase()
        {
            WidthProp.PropertyMinValue = 0;
            HeightProp.PropertyMinValue = 0;

            XProp.PropertyChanged += RectProp_PropertyChanged;
            YProp.PropertyChanged += RectProp_PropertyChanged;
            WidthProp.PropertyChanged += RectProp_PropertyChanged;
            HeightProp.PropertyChanged += RectProp_PropertyChanged;
        }

        public string Name { get; protected set; }

        public IntPropertyViewModel XProp { get; } = new IntPropertyViewModel();

        public IntPropertyViewModel YProp { get; } = new IntPropertyViewModel();

        public IntPropertyViewModel WidthProp { get; } = new IntPropertyViewModel();

        public IntPropertyViewModel HeightProp { get; } = new IntPropertyViewModel();

        public Rect TextureObjectRect
        {
            get => new Rect(XProp.PropertyValue ?? 0, YProp.PropertyValue ?? 0, WidthProp.PropertyValue ?? 0, HeightProp.PropertyValue ?? 0);
            set
            {
                XProp.PropertyValue = Convert.ToInt32(value.X);
                YProp.PropertyValue = Convert.ToInt32(value.Y);
                WidthProp.PropertyValue = Convert.ToInt32(value.Width);
                HeightProp.PropertyValue = Convert.ToInt32(value.Height);
            }
        }

        private void RectProp_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Notify(nameof(TextureObjectRect));
        }

        public IntPropertyViewModel XOffsetProp { get; } = new IntPropertyViewModel();

        public IntPropertyViewModel YOffsetProp { get; } = new IntPropertyViewModel();

        public ColorPropertyViewModel ColorProp { get; } = new ColorPropertyViewModel();

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public bool HasAnyChanges()
        {
            return XProp.PropertyValueChanged
                || YProp.PropertyValueChanged
                || WidthProp.PropertyValueChanged
                || HeightProp.PropertyValueChanged
                || XOffsetProp.PropertyValueChanged
                || YOffsetProp.PropertyValueChanged
                || ColorProp.PropertyValueChanged;
        }

        public void SaveChanges()
        {
            XProp.SaveChanges();
            YProp.SaveChanges();
            WidthProp.SaveChanges();
            HeightProp.SaveChanges();
            XOffsetProp.SaveChanges();
            YOffsetProp.SaveChanges();
            ColorProp.SaveChanges();
        }
    }
}