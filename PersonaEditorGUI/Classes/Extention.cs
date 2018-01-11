using PersonaEditorGUI.Classes.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace PersonaEditorGUI.Classes
{
    public static class ListExtentsion
    {
        public static Block GetDocument(this IList<PersonaEditorLib.FileStructure.Text.TextBaseElement> ByteCollection, Encoding encoding, bool LineSplit = true)
        {
            // List<Block> returned = new List<Block>();

            Paragraph paragraph = new Paragraph();

            foreach (var MSG in ByteCollection)
            {
                if (MSG.IsText)
                    paragraph.Inlines.Add(MSG.GetText(encoding, LineSplit));
                else
                {
                    TextBlock textBlock = new TextBlock();
                    textBlock.Text = MSG.GetSystem();
                    paragraph.Inlines.Add(new InlineUIContainer(textBlock));
                }
            }

            return paragraph;
        }
    }
}