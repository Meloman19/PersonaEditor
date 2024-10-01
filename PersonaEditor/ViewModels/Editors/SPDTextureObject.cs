using System;
using PersonaEditorLib.SpriteContainer;

namespace PersonaEditor.ViewModels.Editors
{
    public sealed class SPDTextureObject : TextureObjectBase
    {
        private readonly SPDKey _key;

        public SPDTextureObject(SPDKey key)
        {
            _key = key ?? throw new ArgumentNullException(nameof(key));
            Name = _key.CommentString;

            XProp.PropertyValue = _key.SpriteX;
            XProp.PropertyValueChanged = false;
            XProp.SaveDelegate = val => _key.SpriteX = val ?? 0;

            YProp.PropertyValue = _key.SpriteY;
            YProp.PropertyValueChanged = false;
            YProp.SaveDelegate = val => _key.SpriteY = val ?? 0;

            WidthProp.PropertyValue = _key.SpriteWidth;
            WidthProp.PropertyValueChanged = false;
            WidthProp.SaveDelegate = val => _key.SpriteWidth = val ?? 0;

            HeightProp.PropertyValue = _key.SpriteHeight;
            HeightProp.PropertyValueChanged = false;
            HeightProp.SaveDelegate = val => _key.SpriteHeight = val ?? 0;

            XOffsetProp.PropertyValue = _key.ScreenXOffset;
            XOffsetProp.PropertyValueChanged = false;
            XOffsetProp.SaveDelegate = val => _key.ScreenXOffset = val ?? 0;

            YOffsetProp.PropertyValue = _key.ScreenYOffset;
            YOffsetProp.PropertyValueChanged = false;
            YOffsetProp.SaveDelegate = val => _key.ScreenYOffset = val ?? 0;
        }
    }
}