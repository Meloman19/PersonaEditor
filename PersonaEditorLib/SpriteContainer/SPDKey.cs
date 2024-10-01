using AuxiliaryLibraries.Extensions;
using System;
using System.IO;

namespace PersonaEditorLib.SpriteContainer
{
    public class SPDKey
    {
        public int ListIndex { get; private set; }
        public int TextureIndex { get; private set; }
        private int[] Unk0x08 { get; set; } // x 6
        public int SpriteX { get; set; }
        public int SpriteY { get; set; }
        public int SpriteWidth { get; set; }
        public int SpriteHeight { get; set; }
        public int ScreenXOffset { get; set; }
        public int ScreenYOffset { get; set; }
        private int ScreenWidth { get; set; }
        private int ScreenHeight { get; set; }

        private int[] Unk0x40 { get; set; } // x 12
        public byte[] Comment { get; private set; }


        private double? XScale;
        private double? YScale;

        public string CommentString => Static.ShiftJIS.GetString(Comment).TrimEnd('\0');

        public SPDKey(BinaryReader reader)
        {
            ListIndex = reader.ReadInt32();
            TextureIndex = reader.ReadInt32();
            Unk0x08 = reader.ReadInt32Array(6);

            SpriteX = reader.ReadInt32();
            SpriteY = reader.ReadInt32();
            SpriteWidth = reader.ReadInt32();
            SpriteHeight = reader.ReadInt32();

            ScreenXOffset = reader.ReadInt32();
            ScreenYOffset = reader.ReadInt32();
            ScreenWidth = reader.ReadInt32();
            ScreenHeight = reader.ReadInt32();

            if (SpriteWidth != 0 && ScreenWidth != 0)
                XScale = (double)SpriteWidth / (double)ScreenWidth;
            if (SpriteHeight != 0 && ScreenHeight != 0)
                YScale = (double)SpriteHeight / (double)ScreenHeight;

            Unk0x40 = reader.ReadInt32Array(12);
            Comment = reader.ReadBytes(0x30);
        }

        public void Get(BinaryWriter writer)
        {
            writer.Write(ListIndex);
            writer.Write(TextureIndex);
            writer.WriteInt32Array(Unk0x08);

            writer.Write(SpriteX);
            writer.Write(SpriteY);
            writer.Write(SpriteWidth);
            writer.Write(SpriteHeight);

            writer.Write(ScreenXOffset);
            writer.Write(ScreenYOffset);
            if (XScale.HasValue)
                ScreenWidth = Convert.ToInt32(SpriteWidth / XScale.Value);
            if (YScale.HasValue)
                ScreenHeight = Convert.ToInt32(SpriteHeight / YScale.Value);
            writer.Write(ScreenWidth);
            writer.Write(ScreenHeight);

            writer.WriteInt32Array(Unk0x40);
            writer.Write(Comment);
        }
    }
}