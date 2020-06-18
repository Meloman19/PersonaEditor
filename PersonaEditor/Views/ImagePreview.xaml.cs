using PersonaEditor.View.Settings;
using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace PersonaEditor.Views
{
    public partial class ImagePreview : UserControl
    {
        public ImagePreview()
        {
            InitializeComponent();

            Loaded += ImagePreview_Loaded;
            Unloaded += ImagePreview_Unloaded;
        }

        private void ImagePreview_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var prop = DependencyPropertyDescriptor.FromProperty(Image.SourceProperty, typeof(Image));
            prop.AddValueChanged(MainImage, SourceChangedHandler);

            ScrollViewer.SetToDefault();
        }

        private void ImagePreview_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var prop = DependencyPropertyDescriptor.FromProperty(Image.SourceProperty, typeof(Image));
            prop.RemoveValueChanged(MainImage, SourceChangedHandler);
        }

        private void SourceChangedHandler(object sender, EventArgs e)
        {
            ScrollViewer.SetToDefault();
        }

        private void CheckBox_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (MainBorder == null)
            {
                return;
            }

            MainBorder.BorderThickness = new System.Windows.Thickness(1);
        }

        private void CheckBox_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (MainBorder == null)
            {
                return;
            }

            MainBorder.BorderThickness = new System.Windows.Thickness(0);
        }
    }
}