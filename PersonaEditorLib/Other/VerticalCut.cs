using System;
using System.Collections.Generic;
using System.Text;

namespace PersonaEditorLib.Other
{
    public struct VerticalCut
    {
        public byte Left { get; private set; }
        public byte Right { get; private set; }

        public VerticalCut(byte[] bytes)
        {
            if (bytes != null)
                if (bytes.Length >= 2)
                {
                    Left = bytes[0];
                    Right = bytes[1];
                    return;
                }

            Left = 0;
            Right = 0;
        }

        public VerticalCut(byte Left, byte Right)
        {
            this.Left = Left;
            this.Right = Right;
        }

        public byte[] Get()
        {
            return new byte[2] { Left, Right };
        }
    }
}