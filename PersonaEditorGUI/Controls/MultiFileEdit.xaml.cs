using PersonaEditorGUI.Files;
using PersonaEditorLib.Interfaces;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PersonaEditorGUI.Controls
{
    public partial class MultiFileEdit : UserControl
    {
        MultiFileEditVM MultiFileEditVM = new MultiFileEditVM();
        SingleFileEditVM SingleFileEditVM = new SingleFileEditVM();

        public MultiFileEdit()
        {
            DataContext = MultiFileEditVM;
            InitializeComponent();

            SingleFile.DataContext = SingleFileEditVM;
        }

        private void LeftCC_SelectedItemData(object sender)
        {
            if (sender is IPersonaFile file)
                PropertyCC.ItemsSource = file.GetProperties;
            else
                PropertyCC.ItemsSource = null;

            if (sender is IPreview preview)
                PreviewCC.Content = preview.Control;
            else
                PreviewCC.Content = null;
        }
        
        private void LeftCC_SelectedItemDataOpen(object sender)
        {
            SingleFileEditVM.Open((sender as UserTreeViewItem).personaFile);
        }

        double temp = 0;
        private void GridSplitter_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (FirstCol.MaxWidth == Double.PositiveInfinity)
            {
                temp = FirstCol.Width.Value;
                FirstCol.MaxWidth = 0;
            }
            else
            {
                FirstCol.MaxWidth = Double.PositiveInfinity;
                FirstCol.Width = new GridLength(temp);
            }
        }
    }
}
