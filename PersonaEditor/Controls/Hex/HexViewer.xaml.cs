using AuxiliaryLibraries.WPF;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace PersonaEditor.Controls.Hex
{
    public partial class HexViewer : UserControl
    {
        private HexHandler _hexHandler = new HexHandler();

        public static readonly DependencyProperty StreamProperty
            = DependencyProperty.Register(nameof(Stream), typeof(Stream), typeof(HexViewer), new PropertyMetadata(null, StreamPropertyChanged));

        public HexViewer()
        {
            InitializeComponent();
            Root.DataContext = null;
            SizeChanged += HexEditorUserControl_SizeChanged;
            MouseWheel += HexEditorUserControl_MouseWheel;
            Scroll.ValueChanged += Scroll_ValueChanged;
            Items.ItemsSource = _hexHandler.Lines;
        }

        public Stream Stream
        {
            get { return (Stream)GetValue(StreamProperty); }
            set { SetValue(StreamProperty, value); }
        }

        private static void StreamPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as HexViewer;
            var newStream = e.NewValue as Stream;

            int lineCount = 0;
            if (newStream != null && newStream.CanRead)
                lineCount = (int)newStream.Length / 0x10;

            control._hexHandler.SetStream(newStream);
            control.Scroll.Maximum = lineCount;
            control.Scroll.Value = 0;
        }

        private void HexEditorUserControl_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            int delta = e.Delta;
            if (delta > 0)
                SetScrollValue(false);
            else if (delta < 0)
                SetScrollValue(true);
        }

        public void SetScrollValue(bool add)
        {
            if (add)
            {
                if (Scroll.Value + 4 <= Scroll.Maximum)
                    Scroll.Value += 4;
                else
                    Scroll.Value = Scroll.Maximum;
            }
            else
            {
                if (Scroll.Value - 4 >= Scroll.Minimum)
                    Scroll.Value -= 4;
                else
                    Scroll.Value = Scroll.Minimum;
            }
        }

        private void Scroll_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            long val = (long)e.NewValue;
            _hexHandler.SetOffsetIndex(val);
        }

        private void HexEditorUserControl_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (e.HeightChanged)
                _hexHandler.SetHeight(e.NewSize.Height);
        }
        private sealed class HexViewLineVM : BindingObject
        {
            private long _offset = 0;
            public long Offset
            {
                get => _offset;
                set => SetProperty(ref _offset, value);
            }

            private byte[] _data = null;
            public byte[] Data
            {
                get => _data;
                set => SetProperty(ref _data, value);
            }
        }

        private sealed class HexHandler
        {
            private Stream _stream;
            private long offsetIndex = 0;

            public ObservableCollection<HexViewLineVM> Lines { get; } = new ObservableCollection<HexViewLineVM>();

            public void SetHeight(double height)
            {
                var targetCount = Convert.ToInt32((height / 16) + 1);
                targetCount = Math.Min(targetCount, 256);

                var addCount = Math.Max(targetCount - Lines.Count, 0);
                for (int i = 0; i < addCount; i++)
                {
                    var line = new HexViewLineVM();
                    SetLine(line, Lines.Count);
                    Lines.Add(line);
                }
            }

            public void SetStream(Stream stream)
            {
                _stream = stream;
                offsetIndex = 0;
                UpdateLines();
            }

            public void SetOffsetIndex(long offset)
            {
                if (offsetIndex != offset)
                {
                    offsetIndex = offset;
                    UpdateLines();
                }
            }

            private void UpdateLines()
            {
                for (int i = 0; i < Lines.Count; i++)
                    SetLine(Lines[i], i);
            }

            private void SetLine(HexViewLineVM line, int index)
            {
                var stream = _stream;
                if (stream == null || !stream.CanRead)
                {
                    long newoffset = index * 16;
                    line.Offset = newoffset;
                    line.Data = null;
                }
                else
                {
                    long newoffset = (offsetIndex + index) * 16;
                    line.Offset = newoffset;
                    if (stream.Length > newoffset)
                    {
                        stream.Position = newoffset;
                        long available = stream.Length - stream.Position;
                        if (available >= 16)
                        {
                            byte[] temp = new byte[16];
                            stream.Read(temp, 0, 16);
                            line.Data = temp;
                        }
                        else
                        {
                            byte[] temp = new byte[available];
                            stream.Read(temp, 0, (int)available);
                            line.Data = temp;
                        }
                    }
                    else
                        line.Data = null;
                }
            }
        }
    }
}