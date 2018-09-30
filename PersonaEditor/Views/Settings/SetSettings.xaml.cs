using System.Windows;

namespace PersonaEditor.View.Settings
{
    public partial class SetSettings : Window
    {
        public SetSettings()
        {
            InitializeComponent();
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}