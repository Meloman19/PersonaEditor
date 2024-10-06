using System.Windows;
using System.Windows.Controls;
using PersonaEditor.Controls;

namespace PersonaEditor.Views.Editors
{
    public partial class TextureAtlasEditor : UserControl
    {
        public TextureAtlasEditor()
        {
            InitializeComponent();
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