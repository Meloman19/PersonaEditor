using System;
using System.Collections.Generic;
using System.IO;
using PersonaEditorLib.Extension;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonaEditorLib.FileStructure.BVP
{
    public class BVP
    {
        List<int> ListUnknown = new List<int>();
        List<byte[]> List = new List<byte[]>();

        public BVP(string file, bool IsLittleEndian)
        {
            BinaryReader reader = Utilities.IO.OpenReadFile(file, IsLittleEndian);

            List<int[]> Entry = new List<int[]>();
            int[] temp = reader.ReadInt32Array(3);

            while (!(temp[0] == 0 & temp[1] == 0 & temp[2] == 0))
            {
                Entry.Add(temp);
                temp = reader.ReadInt32Array(3);
                ListUnknown.Add(temp[0]);
            }

            for (int i = 0; i < Entry.Count; i++)
            {
                reader.BaseStream.Position = Entry[i][1];
                List.Add(reader.ReadBytes(Entry[i][2]));
            }

        }

        public int Count
        {
            get { return List.Count; }
        }

        public byte[] this[int index]
        {
            get
            {
                if (List.Count > index)
                {
                    return List[index].ToArray();
                }
                return null;
            }
            set
            {
                if (List.Count > index)
                {
                    List[index] = value;

                }
            }
        }

        public byte[] Get(bool IsLittleEndian)
        {
            byte[] returned = null;

            using (BinaryWriter writer = Utilities.IO.OpenWriteFile(new MemoryStream(), IsLittleEndian))
            {
                writer.Write(new byte[ListUnknown.Count * 12]);
                writer.Write(new byte[Utilities.Utilities.Alignment(writer.BaseStream.Position, 16)]);

                List<int[]> Entry = new List<int[]>();

                for (int i = 0; i < List.Count; i++)
                {
                    Entry.Add(new int[] { ListUnknown[i], (int)writer.BaseStream.Position, List[i].Length });

                    writer.Write(List[i].Length);
                    writer.Write(List[i]);
                    writer.Write(new byte[Utilities.Utilities.Alignment(writer.BaseStream.Position, 16)]);
                }

                writer.BaseStream.Position = 0;

                foreach (var a in Entry)
                    writer.WriteInt32Array(a);

                writer.BaseStream.Position = 0;
                returned = new byte[writer.BaseStream.Length];
                writer.BaseStream.Read(returned, 0, returned.Length);
            }

            return returned;
        }
    }
}
