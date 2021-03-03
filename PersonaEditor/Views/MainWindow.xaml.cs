using System.Windows;
using PersonaEditor.ViewModels;

namespace PersonaEditor.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            BackgroundWorker.Control = this;

        }
    }
}