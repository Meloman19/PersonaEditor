using PersonaEditor.Controls;
using PersonaEditor.Views.Tools;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PersonaEditor.Views.Editors
{
    public partial class TextureAtlasEditor : UserControl
    {
        public TextureAtlasEditor()
        {
            InitializeComponent();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item)
                if (item.Tag is Color color)
                {
                    ColorPickerTool tool = new ColorPickerTool()
                    {
                        SelectedColor = color
                    };
                    if (tool.ShowDialog() == true)
                        item.Tag = tool.SelectedColor;
                }
        }

        private void TextureAtlasEditControl_CursorPositionChanged(object sender, RoutedEventArgs e)
        {
            var position = (sender as TextureAtlasEditControl).CursorPosition;
            if (!position.HasValue)
                return;

            XCoo.Text = position.Value.X.ToString();
            YCoo.Text = position.Value.Y.ToString();
        }
    }
}