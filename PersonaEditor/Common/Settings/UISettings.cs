using System;
using System.Windows;

namespace PersonaEditor.Common.Settings
{
    public sealed class UISettings : BindingObject, ICloneable
    {
        private GridLength _ptpNameBlockSize = new GridLength(100);
        private double _mainWindowPosX = 0;
        private double _mainWindowPosY = 0;
        private double _mainWindowWidth = 900;
        private double _mainWidnowHeight = 600;
        private GridLength _mainWindowTreeWidth = new GridLength(200);

        public GridLength PTPNameBlockSize
        {
            get => _ptpNameBlockSize;
            set => SetProperty(ref _ptpNameBlockSize, value);
        }

        public double MainWindowPosX
        {
            get => _mainWindowPosX;
            set => SetProperty(ref _mainWindowPosX, value);
        }

        public double MainWindowPosY
        {
            get => _mainWindowPosY;
            set => SetProperty(ref _mainWindowPosY, value);
        }

        public double MainWindowWidth
        {
            get => _mainWindowWidth;
            set => SetProperty(ref _mainWindowWidth, value);
        }

        public double MainWidnowHeight
        {
            get => _mainWidnowHeight;
            set => SetProperty(ref _mainWidnowHeight, value);
        }

        public GridLength MainWindowTreeWidth
        {
            get => _mainWindowTreeWidth;
            set => SetProperty(ref _mainWindowTreeWidth, value);
        }

        public UISettings Clone()
        {
            return (UISettings)MemberwiseClone();
        }

        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }
    }
}