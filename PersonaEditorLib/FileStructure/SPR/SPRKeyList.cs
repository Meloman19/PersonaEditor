using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonaEditorLib.FileStructure.SPR
{
    public class SPRKey : BindingObject
    {
        public int _unk0x00;
        public string mComment { get; private set; }
        public byte[] mCommentByte;
        public int mTextureIndex;
        public int _unk0x18;
        public int _unk0x1C;
        public int _unk0x20;
        public int _unk0x24;
        public int _unk0x28;
        public int _unk0x2C;
        public int _unk0x30;
        public int _unk0x34;
        public int _unk0x38;
        public int _unk0x3C;
        public int _unk0x40;
        public int _unk0x44;
        public int _unk0x48;
        public int _unk0x4C;
        public int _unk0x50;
        private int _X1;
        private int _Y1;
        private int _X2;
        private int _Y2;
        public int ColorA;
        public int ColorR;
        public int ColorG;
        public int ColorB;
        public int _unk0x74;
        public int _unk0x78;
        public int _unk0x7C;

        public int X1
        {
            get { return _X1; }
            set
            {
                if (_X1 != value)
                {
                    _X1 = value;
                    Notify("X1");
                }
            }
        }
        public int Y1
        {
            get { return _Y1; }
            set
            {
                if (_Y1 != value)
                {
                    _Y1 = value;
                    Notify("Y1");
                }
            }
        }
        public int X2
        {
            get { return _X2; }
            set
            {
                if (_X2 != value)
                {
                    _X2 = value;
                    Notify("X2");
                }
            }
        }
        public int Y2
        {
            get { return _Y2; }
            set
            {
                if (_Y2 != value)
                {
                    _Y2 = value;
                    Notify("Y2");
                }
            }
        }

        public SPRKey(byte[] key)
        {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(key)))
            {
                _unk0x00 = reader.ReadInt32();
                mCommentByte = reader.ReadBytes(16);
                mComment = Encoding.GetEncoding("shift-jis").GetString(mCommentByte.Where(x => x != 0x00).ToArray());
                mTextureIndex = reader.ReadInt32();
                _unk0x18 = reader.ReadInt32();
                _unk0x1C = reader.ReadInt32();
                _unk0x20 = reader.ReadInt32();
                _unk0x24 = reader.ReadInt32();
                _unk0x28 = reader.ReadInt32();
                _unk0x2C = reader.ReadInt32();
                _unk0x30 = reader.ReadInt32();
                _unk0x34 = reader.ReadInt32();
                _unk0x38 = reader.ReadInt32();
                _unk0x3C = reader.ReadInt32();
                _unk0x40 = reader.ReadInt32();
                _unk0x44 = reader.ReadInt32();
                _unk0x48 = reader.ReadInt32();
                _unk0x4C = reader.ReadInt32();
                _unk0x50 = reader.ReadInt32();
                X1 = reader.ReadInt32();
                Y1 = reader.ReadInt32();
                X2 = reader.ReadInt32();
                Y2 = reader.ReadInt32();
                ColorA = reader.ReadInt32();
                ColorR = reader.ReadInt32();
                ColorG = reader.ReadInt32();
                ColorB = reader.ReadInt32();
                _unk0x74 = reader.ReadInt32();
                _unk0x78 = reader.ReadInt32();
                _unk0x7C = reader.ReadInt32();
            }
        }

        public int Size
        {
            get { return 0x80; }
        }

        public void Get(BinaryWriter writer)
        {
            writer.Write(_unk0x00);
            writer.Write(mCommentByte);
            writer.Write(mTextureIndex);
            writer.Write(_unk0x18);
            writer.Write(_unk0x1C);
            writer.Write(_unk0x20);
            writer.Write(_unk0x24);
            writer.Write(_unk0x28);
            writer.Write(_unk0x2C);
            writer.Write(_unk0x30);
            writer.Write(_unk0x34);
            writer.Write(_unk0x38);
            writer.Write(_unk0x3C);
            writer.Write(_unk0x40);
            writer.Write(_unk0x44);
            writer.Write(_unk0x48);
            writer.Write(_unk0x4C);
            writer.Write(_unk0x50);
            writer.Write(X1);
            writer.Write(Y1);
            writer.Write(X2);
            writer.Write(Y2);
            writer.Write(ColorA);
            writer.Write(ColorR);
            writer.Write(ColorG);
            writer.Write(ColorB);
            writer.Write(_unk0x74);
            writer.Write(_unk0x78);
            writer.Write(_unk0x7C);
        }
    }

    public class SPRKeyList
    {
        public List<SPRKey> List = new List<SPRKey>();

        public SPRKeyList(BinaryReader reader, int count)
        {
            for (int i = 0; i < count; i++)
                List.Add(new SPRKey(reader.ReadBytes(0x80)));
        }

        public int Size
        {
            get
            {
                int returned = 0;
                foreach (var a in List) returned += a.Size;
                return returned;
            }
        }

        public void Get(BinaryWriter writer)
        {
            foreach (var a in List)
            {
                a.Get(writer);
            }
        }
    }
}