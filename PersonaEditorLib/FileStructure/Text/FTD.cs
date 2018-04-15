using PersonaEditorLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PersonaEditorLib.FileStructure.Text
{
    public class FTD : IPersonaFile
    {
        List<ObjectFile> SubFiles = new List<ObjectFile>();

        byte[] Header;

        List<byte[]> Text = new List<byte[]>();

        public FTD(byte[] data)
        {
            using (MemoryStream MS = new MemoryStream(data))
            using (BinaryReaderBE reader = new BinaryReaderBE(MS))
            {
                Header = reader.ReadBytes(0x20);
                MS.Position += 4;
                int size = reader.ReadInt32();
                int count = reader.ReadInt32();
                int entry_size = size / count;

                for (int i = 0; i < count; i++)
                    Text.Add(reader.ReadBytes(entry_size));
            }
        }

        #region IPersonaFile

        public FileType Type => FileType.FTD;

        public List<ObjectFile> GetSubFiles()
        {
            return SubFiles;
        }

        public Dictionary<string, object> GetProperties
        {
            get
            {
                Dictionary<string, object> returned = new Dictionary<string, object>();

                returned.Add("Type", Type);

                return returned;
            }
        }

        #endregion IPersonaFile

        #region IFile

        public int Size
        {
            get
            {
                return 0;
            }
        }

        public byte[] Get()
        {
            return new byte[0];
        }

        #endregion IFile      
    }
}