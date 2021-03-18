using PersonaEditor.ViewModels.Editors;
using System.Windows;
using System.Windows.Controls;

namespace PersonaEditor.Views.Editors
{
    public partial class FTDEditor : UserControl
    {
        public FTDEditor()
        {
            InitializeComponent();
        }

        private void MenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var vm = (sender as MenuItem)?.CommandParameter as FTDSingleVM;
            if (vm == null)
            {
                return;
            }

            Clipboard.SetText(vm.DataDecode.Replace('\0',' '));
        }
    }
}