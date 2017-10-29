using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonaEditorLib.FileStructure.TBL
{
    public class TBL
    {
        List<byte[]> List = new List<byte[]>();

        public TBL(string path, bool IsLittleEndian)
        {
            BinaryReader reader = Utilities.IO.OpenReadFile(path, IsLittleEndian);

            do
            {
                int Size = reader.ReadInt32();
                List.Add(reader.ReadBytes(Size));
                long temp = Utilities.Utilities.Alignment(reader.BaseStream.Position, 16);
                reader.BaseStream.Position += temp;
            } while (reader.BaseStream.Position < reader.BaseStream.Length);
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
                foreach (var element in List)
                {
                    writer.Write(element.Length);
                    writer.Write(element);
                    writer.Write(new byte[Utilities.Utilities.Alignment(writer.BaseStream.Position, 16)]);
                }

                writer.BaseStream.Position = 0;
                returned = new byte[writer.BaseStream.Length];
                writer.BaseStream.Read(returned, 0, returned.Length);
            }

            return returned;
        }
    }
}
