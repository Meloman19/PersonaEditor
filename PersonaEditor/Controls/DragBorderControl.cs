using PersonaEditor.ViewModels.Editors;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace PersonaEditor.Controls
{
    public sealed class DragBorderVisualize : Control
    {
        static DragBorderVisualize()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DragBorderVisualize),
                new FrameworkPropertyMetadata(typeof(DragBorderVisualize)));
        }

        public Rect Location { get; private set; }

        public void SetNewLocation(Rect rect)
        {
            Location = rect;
            this.SetLocation(Location);
        }
    }

    public sealed class DragBorderControl : Control
    {
        private TextureObjectBase _selectedObject;
        private Rect _location;

        private Thumb ThumbL;
        private Thumb ThumbLT;
        private Thumb ThumbT;
        private Thumb ThumbTR;
        private Thumb ThumbR;
        private Thumb ThumbRB;
        private Thumb ThumbB;
        private Thumb ThumbLB;
        private Thumb ThumbC;

        private DragBorderVisualize _visualize;

        static DragBorderControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DragBorderControl),
                new FrameworkPropertyMetadata(typeof(DragBorderControl)));
        }

        public DragBorderControl()
        {
            Visibility = Visibility.Collapsed;
        }

        public DragBorderControl(DragBorderVisualize visualize)
        {
            Visibility = Visibility.Collapsed;
            visualize.Visibility = Visibility.Collapsed;
            _visualize = visualize;
        }

        public override void OnApplyTemplate()
        {
            ThumbL = GetTemplateChild(nameof(ThumbL)) as Thumb;
            ThumbL.Tag = nameof(ThumbL);
            ThumbL.DragCompleted += Thumb_DragCompleted;
            ThumbL.DragDelta += Thumb_DragDelta;

            ThumbLT = GetTemplateChild(nameof(ThumbLT)) as Thumb;
            ThumbLT.Tag = nameof(ThumbLT);
            ThumbLT.DragCompleted += Thumb_DragCompleted;
            ThumbLT.DragDelta += Thumb_DragDelta;

            ThumbT = GetTemplateChild(nameof(ThumbT)) as Thumb;
            ThumbT.Tag = nameof(ThumbT);
            ThumbT.DragCompleted += Thumb_DragCompleted;
            ThumbT.DragDelta += Thumb_DragDelta;

            ThumbTR = GetTemplateChild(nameof(ThumbTR)) as Thumb;
            ThumbTR.Tag = nameof(ThumbTR);
            ThumbTR.DragCompleted += Thumb_DragCompleted;
            ThumbTR.DragDelta += Thumb_DragDelta;

            ThumbR = GetTemplateChild(nameof(ThumbR)) as Thumb;
            ThumbR.Tag = nameof(ThumbR);
            ThumbR.DragCompleted += Thumb_DragCompleted;
            ThumbR.DragDelta += Thumb_DragDelta;

            ThumbRB = GetTemplateChild(nameof(ThumbRB)) as Thumb;
            ThumbRB.Tag = nameof(ThumbRB);
            ThumbRB.DragCompleted += Thumb_DragCompleted;
            ThumbRB.DragDelta += Thumb_DragDelta;

            ThumbB = GetTemplateChild(nameof(ThumbB)) as Thumb;
            ThumbB.Tag = nameof(ThumbB);
            ThumbB.DragCompleted += Thumb_DragCompleted;
            ThumbB.DragDelta += Thumb_DragDelta;

            ThumbLB = GetTemplateChild(nameof(ThumbLB)) as Thumb;
            ThumbLB.Tag = nameof(ThumbLB);
            ThumbLB.DragCompleted += Thumb_DragCompleted;
            ThumbLB.DragDelta += Thumb_DragDelta;

            ThumbC = GetTemplateChild(nameof(ThumbC)) as Thumb;
            ThumbC.Tag = nameof(ThumbC);
            ThumbC.DragCompleted += Thumb_DragCompleted;
            ThumbC.DragDelta += Thumb_DragDelta;
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var deltaX = Math.Round(e.HorizontalChange);
            var deltaY = Math.Round(e.VerticalChange);

            switch ((sender as Thumb).Tag as string)
            {
                case nameof(ThumbL):
                    Transform(deltaX, 0, -deltaX, 0);
                    break;
                case nameof(ThumbLT):
                    Transform(deltaX, deltaY, -deltaX, -deltaY);
                    break;
                case nameof(ThumbT):
                    Transform(0, deltaY, 0, -deltaY);
                    break;
                case nameof(ThumbTR):
                    Transform(0, deltaY, deltaX, -deltaY);
                    break;
                case nameof(ThumbR):
                    Transform(0, 0, deltaX, 0);
                    break;
                case nameof(ThumbRB):
                    Transform(0, 0, deltaX, deltaY);
                    break;
                case nameof(ThumbB):
                    Transform(0, 0, 0, deltaY);
                    break;
                case nameof(ThumbLB):
                    Transform(deltaX, 0, -deltaX, deltaY);
                    break;
                case nameof(ThumbC):
                    Transform(deltaX, deltaY, 0, 0);
                    break;
            }
        }

        private void Transform(double offsetX, double offsetY, double changeWidth, double changeHeight)
        {
            var newLocation = new Rect(
                _location.X + offsetX,
                _location.Y + offsetY,
                _location.Width + Math.Max(-_location.Width + 1, changeWidth),
                _location.Height + Math.Max(-_location.Height + 1, changeHeight));
            _visualize.SetNewLocation(newLocation);
        }

        private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            _location = _visualize.Location;
            this.SetLocation(_location);
            _selectedObject.X = Convert.ToInt32(_location.X);
            _selectedObject.Y = Convert.ToInt32(_location.Y);
            _selectedObject.Width = Convert.ToInt32(_location.Width);
            _selectedObject.Height = Convert.ToInt32(_location.Height);
        }

        private void UpdateLocation()
        {
            if (_selectedObject == null)
            {
                Visibility = Visibility.Collapsed;
                _visualize.Visibility = Visibility.Collapsed;
                _location = new Rect();
            }
            else
            {
                Visibility = Visibility.Visible;
                _visualize.Visibility = Visibility.Visible;
                _location = new Rect(_selectedObject.X, _selectedObject.Y, _selectedObject.Width, _selectedObject.Height);
            }

            _visualize.SetLocation(_location);
            this.SetLocation(_location);
        }

        public void SetSelectedObject(object selectedObject)
        {
            var newSelectedObject = selectedObject as TextureObjectBase;
            _selectedObject = newSelectedObject;
            UpdateLocation();
        }
    }
}