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

namespace PersonaEditor.Controls.ToolBox
{
    /// <summary>
    /// Логика взаимодействия для Resize.xaml
    /// </summary>
    public partial class Resize : Window
    {
        private int size = 0;
        public int Size
        {
            get { return size; }
            set
            {
                size = value;
                TextB.Text = size.ToString();
            }
        }

        public Resize()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private string tempText = "0";
        private void TextB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(TextB.Text, out int newsize))
            {
                size = newsize;
                tempText = TextB.Text;
            }
            else
                TextB.Text = tempText;
        }
    }
}