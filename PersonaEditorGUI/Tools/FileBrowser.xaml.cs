using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PersonaEditorGUI.Tools
{
    /// <summary>
    /// Логика взаимодействия для FileBrowser.xaml
    /// </summary>
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
