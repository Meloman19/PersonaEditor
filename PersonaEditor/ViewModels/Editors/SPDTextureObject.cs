using PersonaEditorLib.SpriteContainer;
using System;
using System.Windows;

namespace PersonaEditor.ViewModels.Editors
{
    public sealed class SPDTextureObject : TextureObjectBase
    {
        private readonly SPDKey _key;

        public SPDTextureObject(SPDKey key)
        {
            _key = key ?? throw new ArgumentNullException(nameof(key));
            Name = _key.CommentString;
        }

        public override int X
        {
            get => _key.X0;
            set
            {
                if (value != _key.X0)
                {
                    _key.X0 = value;
                    Notify(nameof(X));
                    Notify(nameof(Rect));
                }
            }
        }

        public override int Y
        {
            get => _key.Y0;
            set
            {
                if (value != _key.Y0)
                {
                    _key.Y0 = value;
                    Notify(nameof(Y));
                    Notify(nameof(Rect));
                }
            }
        }

        public override int Width
        {
            get => _key.Xdel;
            set
            {
                if (value != _key.Xdel)
                {
                    _key.Xdel = value;
                    Notify(nameof(Width));
                    Notify(nameof(Rect));
                }
            }
        }

        public override int Height
        {
            get => _key.Ydel;
            set
            {
                if (value != _key.Ydel)
                {
                    _key.Ydel = value;
                    Notify(nameof(Height));
                    Notify(nameof(Rect));
                }
            }
        }

        public Rect Rect => new Rect(new Point(_key.X0, _key.Y0), new Size(_key.Xdel, _key.Ydel));
    }
}