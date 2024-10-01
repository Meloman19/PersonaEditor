using System;
using System.Windows.Media;
using AuxiliaryLibraries.Media;
using AuxiliaryLibraries.WPF.Wrapper;
using PersonaEditorLib.SpriteContainer;

namespace PersonaEditor.ViewModels.Editors
{
    public sealed class SPRTextureObject : TextureObjectBase
    {
        private readonly SPRKey _key;

        public SPRTextureObject(SPRKey key)
        {
            _key = key ?? throw new ArgumentNullException(nameof(key));
            Name = _key.mComment;

            XProp.PropertyValue = _key.X1;
            XProp.PropertyValueChanged = false;
            XProp.SaveDelegate = val =>
            {
                var width = _key.X2 - _key.X1;
                _key.X1 = val ?? 0;
                _key.X2 = _key.X1 + width;
            };

            YProp.PropertyValue = _key.Y1;
            YProp.PropertyValueChanged = false;
            YProp.SaveDelegate = val =>
            {
                var height = _key.Y2 - _key.Y1;
                _key.Y1 = val ?? 0;
                _key.Y2 = _key.Y1 + height;
            };

            WidthProp.PropertyValue = _key.X2 - _key.X1;
            WidthProp.PropertyValueChanged = false;
            WidthProp.SaveDelegate = val => _key.X2 = _key.X1 + val ?? 0;

            HeightProp.PropertyValue = _key.Y2 - _key.Y1;
            HeightProp.PropertyValueChanged = false;
            HeightProp.SaveDelegate = val => _key.Y2 = _key.Y1 + val ?? 0;

            ColorProp.PropertyValue = DecodingHelper.PixelFromFullRgba32PS2(_key.RGBACoords[0], reverseOrder: true).ToColor();
            ColorProp.PropertyValueChanged = false;
            ColorProp.SaveDelegate = val =>
            {
                var pixel = (val ?? Colors.White).ToPixel();
                var data = EncodingHelper.ToFullRgba32PS2(pixel, reverseOrder: true);
                for (var i = 0; i < 4; i++)
                    _key.RGBACoords[i] = data;
            };
        }
    }
}