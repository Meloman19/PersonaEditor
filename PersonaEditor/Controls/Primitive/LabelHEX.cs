using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PersonaEditor.Controls.Primitive
{
    class LabelHEX : TextBlock
    {
        public static DependencyProperty ByteArrayProperty = DependencyProperty.Register("ByteArray", typeof(object), typeof(LabelHEX), new PropertyMetadata(null, propertyChanged));

        private static void propertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as LabelHEX).DataUpdate();
        }

        public object ByteArray
        {
            get { return GetValue(ByteArrayProperty); }
            set { SetValue(ByteArrayProperty, value); }
        }

        internal void DataUpdate()
        {
            if (ByteArray is byte[] data)
            {
                string temp = "";

                for (int i = 0; i < data.Length; i++)
                {
                    temp += string.Format("{0:X2}", data[i]);
                    if (i + 1 == data.Length)
                    {
                        while ((i + 1) % 16 != 0)
                        {
                            temp += " ..";
                            i++;
                        }
                        continue;
                    }

                    if ((i + 1) % 16 == 0)
                        temp += Environment.NewLine;
                    else
                        temp += " ";
                }
                Text = temp;
            }
            else
                Text = "DATA is null";
        }

        public LabelHEX() : base()
        {
            Text = "DATA is null";
            FontFamily = new FontFamily(System.Drawing.FontFamily.GenericMonospace.Name);
            
        }
    }
}