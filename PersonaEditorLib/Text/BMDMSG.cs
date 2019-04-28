using AuxiliaryLibraries.Extensions;
using AuxiliaryLibraries.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PersonaEditorLib.Text
{
    public class BMDMSG
    {
        public int Index { get; set; }
        public int Type { get; set; }
        public int NameIndex { get; set; }
        public byte[][] MsgStrings { get; set; }
        public string Name { get; set; }
        private int SEL_UNKNOWN = 0;

        public BMDMSG()
        {

        }

        public BMDMSG(BinaryReader reader, int index, int type)
        {
            Index = index;
            Type = type;
            NameIndex = 0xFFFF;
            MsgStrings = null;
            Name = Encoding.ASCII.GetString(reader.ReadBytes(24)).TrimEnd('\0');
            if (string.IsNullOrEmpty(Name))
                Name = "<EMPTY>";

            if (Type == 0)
            {
                int count = reader.ReadUInt16();
                NameIndex = reader.ReadUInt16();

                if (count > 0)
                {
                    int[] stringPointer = reader.ReadInt32Array(count);
                    int msgStringsSize = reader.ReadInt32();
                    int msgStringsPos = (int)reader.BaseStream.Position - 0x20;
                    stringPointer = stringPointer.Select(x => x - msgStringsPos).ToArray();

                    MsgStrings = reader.ReadBytes(msgStringsSize).Split(stringPointer).ToArray();

                    //This is hardcoded fix! Japanese version of Persona 5
                    //contain invalid file: data.cpk\field\npc\corp009.bf
                    //Where there are incorrect pointers.
                    //In english version all correct!
                    if (Name == "MSG_C09_ODA_05_00" && MsgStrings.Length == 3)
                        MsgStrings = new byte[2][] { MsgStrings[0].Concat(MsgStrings[1]).ToArray(), MsgStrings[2] };
                    else if ((Name == "MSG_C09_BYEGREET_00_00" | Name == "MSG_C09_BYEGREET_01_00") && MsgStrings.Length == 5)
                        MsgStrings = new byte[4][] { MsgStrings[0].Concat(MsgStrings[1]).ToArray(), MsgStrings[2], MsgStrings[3], MsgStrings[4] };
                }
                else
                {
                    MsgStrings = new byte[0][];
                }
            }
            else if (Type == 1)
            {
                if (reader.ReadInt16() != 0)
                    throw new Exception("BMD Read Error: Select Message - 0x18 not 0");
                int count = reader.ReadInt16();
                SEL_UNKNOWN = reader.ReadInt32();

                if (count > 0)
                {
                    int[] stringPointer = reader.ReadInt32Array(count);
                    int msgStringsSize = reader.ReadInt32();
                    int msgStringsPos = (int)reader.BaseStream.Position - 0x20;
                    stringPointer = stringPointer.Select(x => x - msgStringsPos).ToArray();

                    MsgStrings = reader.ReadBytes(msgStringsSize).Split(stringPointer).ToArray();
                }
                else
                {
                    MsgStrings = new byte[0][];
                }
            }
            else
                throw new Exception("BMD Read Error: Unknown MSG's type");
        }

        public int GetSize()
        {
            int returned = 28;
            if (Type == 1)
                returned += 4;
            if (MsgStrings.Length > 0)
                returned += 4 + 4 * MsgStrings.Length;
            returned += MsgStrings.Sum(x => x.Length);
            returned += IOTools.Alignment(returned, 4);
            return returned;
        }

        public void Write(BinaryWriter writer, int offset, List<int> pointers)
        {
            int size = 28;
            writer.WriteString(Name, Encoding.ASCII, 24);

            if (Type == 0)
            {
                writer.Write((ushort)MsgStrings.Length);

                if (NameIndex == -1)
                    writer.Write((ushort)0xFFFF);
                else
                    writer.Write((ushort)NameIndex);
            }
            else if (Type == 1)
            {
                writer.Write((ushort)0);
                writer.Write((ushort)MsgStrings.Length);
                writer.Write(SEL_UNKNOWN);

                size += 4;
            }
            else
                throw new Exception("BMD Write Error: Unknown type");

            if (MsgStrings.Length > 0)
            {
                size += 4 + 4 * MsgStrings.Length;

                int sum = 0;
                foreach (var a in MsgStrings)
                {
                    pointers.Add((int)writer.BaseStream.Position);
                    writer.Write(offset + size + sum);
                    sum += a.Length;
                }

                writer.Write(sum);

                foreach (var a in MsgStrings)
                    writer.Write(a);

                writer.Write(new byte[IOTools.Alignment(size + sum, 4)]);
            }
        }
    }
}