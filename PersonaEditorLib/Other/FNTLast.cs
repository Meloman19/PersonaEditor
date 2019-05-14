using AuxiliaryLibraries.IO;
using AuxiliaryLibraries.Tools;
using System;
using System.Collections.Generic;
using System.IO;

namespace PersonaEditorLib.Other
{
    public class FNTLast
    {
        List<byte[]> List = new List<byte[]>();

        public FNTLast(BinaryReader reader, int count)
        {
            for (int i = 0; i < count & reader.BaseStream.Position < reader.BaseStream.Length; i++)
                List.Add(reader.ReadBytes(2));
        }

        public int Size
        {
            get { return List.Count * 2; }
        }

        public int Get(BinaryWriter writer)
        {
            writer.Write(new byte[IOTools.Alignment(writer.BaseStream.Length, 16)]);
            long returned = writer.BaseStream.Position;
            foreach (var a in List)
                writer.Write(a);

            return Convert.ToInt32(returned);
        }
    }
}