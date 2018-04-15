using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace PersonaEditorLib.FileStructure.SPR
{
    class SPDTextureKey
    {
        public string Name { get; set; }
    }

    class SPD
    {

        public SPD(byte[] data)
        {
            using (MemoryStream MS = new MemoryStream(data))
                Read(new StreamFile(MS, MS.Length, 0));
        }

        private void Read(StreamFile streamFile)
        {
            streamFile.Stream.Position = streamFile.Position;

            using (BinaryReader reader = new BinaryReader(streamFile.Stream, Encoding.ASCII, true))
            {

            }
        }
    }
}