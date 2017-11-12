using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PersonaEditorLib.Extension;

namespace PersonaEditorLib.FileStructure.SPR
{
    class SPRTextures : ISPR
    {
        public List<TMX.TMX> list = new List<TMX.TMX>();

        public SPRTextures(BinaryReader reader, List<int> offset)
        {
            foreach (var off in offset)
                list.Add(new TMX.TMX(reader.BaseStream, off, true));
        }

        public int Size
        {
            get
            {
                int returned = 0;

                returned += list[0].Size;
                for(int i = 1; i<list.Count;i++)
                {
                    int temp = Utilities.Utilities.Alignment(returned, 16);
                    returned += temp == 0 ? 16 : temp;
                    returned += list[i].Size;
                }

                return returned;
            }
        }

        public void Get(BinaryWriter writer)
        {
            writer.Write(list[0].Get(true));
            for (int i = 1; i < list.Count; i++)
            {
                int temp = Utilities.Utilities.Alignment(writer.BaseStream.Length, 16);
                writer.Write(new byte[temp == 0 ? 16 : temp]) ;
                writer.Write(list[i].Get(true));
            }
        }
    }
}