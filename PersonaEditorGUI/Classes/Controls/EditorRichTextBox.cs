using PersonaEditorLib.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace PersonaEditorGUI.Classes.Controls
{
    class EditorRichTextBox : BindableRichTextBox
    {
        public EditorRichTextBox()
        {
            DataObject.AddCopyingHandler(this, Copy);
            DataObject.AddPastingHandler(this, Paste);
        }


        private void Paste(object sender, DataObjectPastingEventArgs e)
        {
            e.CancelCommand();

            string text = (string)e.DataObject.GetData(DataFormats.UnicodeText);

            if (text == null)
                return;

            var input = text.SplitBySystem();
            var te = (RichTextBox)sender;

            foreach (var a in input)
            {
                if (a.StartsWith("{") && a.EndsWith("}"))
                {
                    TextBlock textBlock = new TextBlock();
                    textBlock.Text = a;
                    new InlineUIContainer(textBlock, te.CaretPosition);
                }
                else
                {
                    new Run(a, te.CaretPosition);
                }
            }
        }

        private void Copy(object sender, DataObjectCopyingEventArgs e)
        {
            RichTextBox richTextBox = (RichTextBox)sender;
            TextPointer start = richTextBox.Selection.Start;
            TextPointer end = richTextBox.Selection.End;

            int poscount = start.GetOffsetToPosition(end);

            string data = "";

            for (int i = 0; i < poscount; i++)
            {
                var type = start.GetPointerContext(LogicalDirection.Forward);
                if (type == TextPointerContext.EmbeddedElement)
                {                    
                    data += (start.GetAdjacentElement(LogicalDirection.Forward) as TextBlock).Text;
                }
                else if(type == TextPointerContext.Text)
                {
                    data += start.GetTextInRun(LogicalDirection.Forward)[0];
                }
                
                
                start = start.GetNextContextPosition(LogicalDirection.Forward);
            }

            e.DataObject.SetData(DataFormats.UnicodeText, data);
        }
    }
}
