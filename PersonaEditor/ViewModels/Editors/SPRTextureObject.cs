using PersonaEditorLib.SpriteContainer;
using System;
using System.Windows;

namespace PersonaEditor.ViewModels.Editors
{
    public sealed class SPRTextureObject : TextureObjectBase
    {
        private readonly SPRKey _key;

        public SPRTextureObject(SPRKey key)
        {
            _key = key ?? throw new ArgumentNullException(nameof(key));
            Name = _key.mComment;
        }

        public override int X
        {
            get => _key.X1;
            set
            {
                if (value != _key.X1)
                {
                    _key.X1 = value;
                    Notify(nameof(X));
                    Notify(nameof(Rect));
                }
            }
        }

        public override int Y
        {
            get => _key.Y1;
            set
            {
                if (value != _key.Y1)
                {
                    _key.Y1 = value;
                    Notify(nameof(Y));
                    Notify(nameof(Rect));
                }
            }
        }

        public override int Width
        {
            get => _key.X2 - _key.X1;
            set
            {
                if (value != _key.X2 - _key.X1)
                {
                    _key.X2 = _key.X1 + value;
                    Notify(nameof(Width));
                    Notify(nameof(Rect));
                }
            }
        }

        public override int Height
        {
            get => _key.Y2 - _key.Y1;
            set
            {
                if (value != _key.Y2 - _key.Y1)
                {
                    _key.Y2 = _key.Y1 + value;
                    Notify(nameof(Height));
                    Notify(nameof(Rect));
                }
            }
        }

        public Rect Rect => new Rect(new Point(_key.X1, _key.Y1), new Point(_key.X2, _key.Y2));
    }
}