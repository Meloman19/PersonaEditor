using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace PersonaEditor.Controls.Hex
{
    public partial class HexViewerLine : UserControl
    {
        public static readonly DependencyProperty OffsetProperty
            = DependencyProperty.Register(nameof(Offset), typeof(long), typeof(HexViewerLine), new PropertyMetadata((long)0, OffsetPropertyChanged));

        public static readonly DependencyProperty DataProperty
            = DependencyProperty.Register(nameof(Data), typeof(IList<byte>), typeof(HexViewerLine), new PropertyMetadata(null, DataPropertyChanged));

        public HexViewerLine()
        {
            InitializeComponent();
            Root.DataContext = null;
        }

        public long Offset
        {
            get { return (long)GetValue(OffsetProperty); }
            set { SetValue(OffsetProperty, value); }
        }

        public IList<byte> Data
        {
            get { return (IList<byte>)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        private static void OffsetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as HexViewerLine;
            var newOffset = (long)e.NewValue;
            control.HexOffset.Text = string.Format("0x{0:X8}", newOffset);
        }

        private static void DataPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as HexViewerLine;
            var newData = e.NewValue as IList<byte>;
            control.SetData(newData);
        }

        private void SetData(IList<byte> data)
        {
            Hex01.Text = GetHEX(data, 0);
            Hex02.Text = GetHEX(data, 4);
            Hex03.Text = GetHEX(data, 8);
            Hex04.Text = GetHEX(data, 12);

            AsText.Text = Encode(data);
        }

        private static string GetHEX(IList<byte> data, int offset)
        {
            if (data == null)
                return ".. .. .. ..";

            var sb = new StringBuilder();

            for (int i = 0; i < 4; i++)
            {
                var iOffset = i + offset;
                byte? b = null;
                if (iOffset < data.Count)
                    b = data[iOffset];

                if (b.HasValue)
                    sb.AppendFormat("{0:X2}", b.Value);
                else
                    sb.Append("..");

                if (i < 3)
                    sb.Append(' ');
            }

            return sb.ToString();
        }

        private static string Encode(IList<byte> data)
        {
            if (data == null)
                return string.Empty;

            var sb = new StringBuilder();
            foreach (byte b in data)
            {
                if (b < 0x20 || b > 0x7F)
                    sb.Append('.');
                else
                    sb.Append((char)b);
            }

            return sb.ToString();
        }
    }
}