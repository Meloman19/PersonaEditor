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

            XProp.PropertyValue = _key.X0;
            XProp.PropertyValueChanged = false;
            XProp.SaveDelegate = val => _key.X0 = val ?? 0;

            YProp.PropertyValue = _key.Y0;
            YProp.PropertyValueChanged = false;
            YProp.SaveDelegate = val => _key.Y0 = val ?? 0;

            WidthProp.PropertyValue = _key.Xdel;
            WidthProp.PropertyValueChanged = false;
            WidthProp.SaveDelegate = val => _key.Xdel = val ?? 0;

            HeightProp.PropertyValue = _key.Ydel;
            HeightProp.PropertyValueChanged = false;
            HeightProp.SaveDelegate = val => _key.Ydel = val ?? 0;
        }
    }
}