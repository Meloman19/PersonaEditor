using PersonaEditor.ViewModels.Tools;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PersonaEditor.Views.Tools
{
    public partial class FileBrowser : Window
    {
        public FileBrowser()
        {
            InitializeComponent();
        }

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            (DataContext as FileBrowserVM).mouseDoubleClick((sender as DataGridRow).DataContext as FileBrowserGridLine);
        }
    }
}