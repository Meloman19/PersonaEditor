﻿using System.Diagnostics;
using System.Windows;

namespace PersonaEditor.Views
{
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            try
            {
                var processInfo = new ProcessStartInfo()
                {
                    FileName = e.Uri.AbsoluteUri,
                    UseShellExecute = true
                };
                Process.Start(processInfo);
            }
            catch
            {
            }
            e.Handled = true;
        }
    }
}