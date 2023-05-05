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
        public int X0 { get; set; }
        public int Y0 { get; set; }
        public int Xdel { get; set; }
        public int Ydel { get; set; }
        private int X1 { get; set; }
        private int Y1 { get; set; }
        private int X1Del { get; set; }
        private int Y1Del { get; set; }

        private int[] Unk0x40 { get; set; } // x 12
        public byte[] Comment { get; private set; }


        private double XScale;
        private double YScale;

        public string CommentString => Static.ShiftJIS.GetString(Comment).TrimEnd('\0');

        public SPDKey(BinaryReader reader)
        {
            ListIndex = reader.ReadInt32();
            TextureIndex = reader.ReadInt32();
            Unk0x08 = reader.ReadInt32Array(6);

            X0 = reader.ReadInt32();
            Y0 = reader.ReadInt32();
            Xdel = reader.ReadInt32();
            Ydel = reader.ReadInt32();

            X1 = reader.ReadInt32();
            Y1 = reader.ReadInt32();
            X1Del = reader.ReadInt32();
            Y1Del = reader.ReadInt32();

            XScale = X1Del == 0 ? 1 : ((double)Xdel / (double)X1Del);
            YScale = Y1Del == 0 ? 1 : ((double)Ydel / (double)Y1Del);

            Unk0x40 = reader.ReadInt32Array(12);
            Comment = reader.ReadBytes(0x30);
        }

        public void Get(BinaryWriter writer)
        {
            writer.Write(ListIndex);
            writer.Write(TextureIndex);
            writer.WriteInt32Array(Unk0x08);

            writer.Write(X0);
            writer.Write(Y0);
            writer.Write(Xdel);
            writer.Write(Ydel);

            writer.Write(X1);
            writer.Write(Y1);
            X1Del = Convert.ToInt32(Xdel / XScale);
            Y1Del = Convert.ToInt32(Ydel / YScale);
            writer.Write(X1Del);
            writer.Write(Y1Del);

            writer.WriteInt32Array(Unk0x40);
            writer.Write(Comment);
        }
    }
}