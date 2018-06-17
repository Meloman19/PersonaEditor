using PersonaEditorLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PersonaEditorGUI.Controls
{
    public class ImagePreviewVM : BindingObject
    {
        BitmapSource TransparentBackground = PersonaEditorLib.Utilities.WPF.CreateTransparentBackground(Colors.White, Colors.Gray, 50);
        public BitmapSource Background2 => TransparentBackground;

        public ReadOnlyObservableCollection<PropertyClass> PropertiesView { get; private set; }

        private ImageSource imageSource = null;
        public ImageSource SourceIMG
        {
            get { return imageSource; }
            set
            {
                if (imageSource != value)
                {
                    imageSource = value;
                    Notify("SourceIMG");
                }
            }
        }

        private Color background;
        public Color Background
        {
            get { return background; }
            set
            {
                if (background != value)
                {
                    background = value;
                    Settings.AppSetting.Default.PreviewSelectedColor = value;
                    Notify("Background");
                }
            }
        }

        public ICommand SelectBack { get; }
        private void SelectBackground()
        {
            ColorPicker.ColorPickerTool colorPickerTool = new ColorPicker.ColorPickerTool(Background);
            if (colorPickerTool.ShowDialog() == true)
                Background = colorPickerTool.Color;
        }

        public ImagePreviewVM()
        {
            background = Settings.AppSetting.Default.PreviewSelectedColor;
            SelectBack = new RelayCommand(SelectBackground);
        }
        
        public void SetPropertiesTable(ReadOnlyObservableCollection<PropertyClass> PropertiesView)
        {
            this.PropertiesView = PropertiesView;
            Notify("PropertiesView");
        }
    }
}