using AuxiliaryLibraries.Extensions;
using AuxiliaryLibraries.IO;
using AuxiliaryLibraries.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace PersonaEditorLib.Text
{
    public class BMD : IGameData
    {
        private BMD()
        {

        }

        public BMD(byte[] data)
        {
            using (MemoryStream MS = new MemoryStream(data))
            {
                Read(new StreamPart(MS, MS.Length, 0));
            }
        }

        public BMD(PTP ptp, Encoding newEncoding)
        {
            Read(ptp, newEncoding);
        }

        private void Read(StreamPart streamPart)
        {
            streamPart.Stream.Position = streamPart.Position + 0x8;
            byte[] buffer = new byte[4];
            streamPart.Stream.Read(buffer, 0, 4);
            IsLittleEndian = Encoding.ASCII.GetString(buffer) == "MSG1";

            streamPart.Stream.Position = streamPart.Position;
            using (BinaryReader reader = IOTools.OpenReadFile(streamPart.Stream, IsLittleEndian))
            {
                int MSG_Pointer_Start = 0x20;

                #region Header

                if (!reader.ReadBytes(4).SequenceEqual(new byte[] { 0x7, 0x0, 0x0, 0x0 }))
                    throw new Exception("BMD Read Error: (0x0) not 7");
                int BMD_Size = reader.ReadInt32();
                reader.ReadInt32(); // MagicNumber
                if (reader.ReadInt32() != 0)
                    throw new Exception("BMD Read Error: (0x0C) not 0");

                reader.ReadInt32(); // Position of EndBlock
                int MSG_PointerBlock_Size = reader.ReadInt32();
                int MSG_Count = reader.ReadInt32();
                if (reader.ReadInt16() != 0)
                    throw new Exception("BMD Read Error: (0x1C) not 0");
                if (reader.ReadInt16() != 2)
                    throw new Exception("BMD Read Error: (0x1E) not 2");

                #endregion

                #region MSG/Name Pointer

                List<int[]> MSGPosition = new List<int[]>();

                for (int i = 0; i < MSG_Count; i++)
                {
                    int[] temp = new int[2];
                    temp[0] = reader.ReadInt32();
                    temp[1] = reader.ReadInt32();
                    MSGPosition.Add(temp);
                }

                int Name_Block_Position = reader.ReadInt32();
                int Name_Count = reader.ReadInt32();

                #endregion

                reader.BaseStream.Position = Name_Block_Position + MSG_Pointer_Start;
                List<int> NamePosition = new List<int>();
                for (int i = 0; i < Name_Count; i++)
                    NamePosition.Add(reader.ReadInt32());

                for (int i = 0; i < NamePosition.Count; i++)
                {
                    reader.BaseStream.Position = NamePosition[i] + MSG_Pointer_Start;
                    List<byte> Bytes = new List<byte>(30);

                    byte Byte = reader.ReadByte();
                    while (Byte != 0)
                    {
                        Bytes.Add(Byte);
                        Byte = reader.ReadByte();
                    }

                    Name.Add(new BMDName(i, Bytes.ToArray()));
                }

                for (int i = 0; i < MSGPosition.Count; i++)
                {
                    reader.BaseStream.Position = MSG_Pointer_Start + MSGPosition[i][1];
                    Msg.Add(new BMDMSG(reader, i, MSGPosition[i][0]));
                }
            }
        }

        private void Read(PTP ptp, Encoding newEncoding)
        {
            foreach (var a in ptp.Names)
                Name.Add(new BMDName(a.Index, a.NewName.GetTextBases(newEncoding).GetByteArray()));

            foreach (var a in ptp.Msg)
            {
                Msg.Add(new BMDMSG
                {
                    Index = a.Index,
                    Name = a.Name,
                    Type = a.Type,
                    NameIndex = a.CharacterIndex,
                    MsgStrings = a.Strings.Select(x => x.GetNew(newEncoding)).ToArray()
                });
            }

        }

        public List<BMDMSG> Msg { get; } = new List<BMDMSG>();
        public List<BMDName> Name { get; } = new List<BMDName>();

        public bool IsLittleEndian { get; set; } = true;

        #region IGameFile

        public FormatEnum Type => FormatEnum.BMD;

        public List<GameFile> SubFiles { get; } = new List<GameFile>();

        public int GetSize()
        {
            return GetData().Length;
        }

        public byte[] GetData()
        {
            using (MemoryStream MS = new MemoryStream())
            using (BinaryWriter writer = IOTools.OpenWriteFile(MS, IsLittleEndian))
            {
                byte[] buffer;

                List<int> LastBlock = new List<int>();

                #region Header

                buffer = new byte[4] { 7, 0, 0, 0 };
                writer.Write(buffer);
                writer.Write((int)0x0);
                buffer = Encoding.ASCII.GetBytes("MSG1");
                writer.Write(BitConverter.ToInt32(buffer, 0));
                writer.Write((int)0x0);
                writer.Write((int)0x0);
                writer.Write((int)0x0);
                writer.Write(Msg.Count);
                writer.Write((ushort)0);
                writer.Write((ushort)0x2);

                #endregion

                #region MSG Pointer

                int MSG_offset = Msg.Count * 8 + 16;

                foreach (var MSG in Msg)
                {
                    writer.Write(MSG.Type);
                    LastBlock.Add((int)MS.Position);
                    writer.Write(MSG_offset);
                    MSG_offset += MSG.GetSize();
                }
                LastBlock.Add((int)MS.Position);
                writer.Write(MSG_offset);
                writer.Write(Name.Count);
                writer.Write((int)0x0);
                writer.Write((int)0x0);

                #endregion

                #region MSG

                MSG_offset = Msg.Count * 8 + 16;

                foreach (var MSG in Msg)
                {
                    MSG.Write(writer, MSG_offset, LastBlock);
                    MSG_offset += MSG.GetSize();
                }

                #endregion

                #region Name Pointer

                MSG_offset += Name.Count * 4;

                foreach (var name in Name)
                {
                    LastBlock.Add((int)MS.Position);
                    writer.Write(MSG_offset);
                    if (name.NameBytes.Length == 0)
                        MSG_offset += 2;
                    else
                        MSG_offset += name.NameBytes.Length + 1;
                }

                #endregion

                #region Name

                foreach (var name in Name)
                {
                    if (name.NameBytes.Length == 0)
                        writer.Write((byte)0x20);
                    else
                        writer.Write(name.NameBytes);
                    writer.Write((byte)0);
                }
                writer.Write(new byte[IOTools.Alignment(MS.Position, 4)]);

                #endregion

                #region Pointers

                int LastBlockPos = (int)MS.Position;
                byte[] ptrDiffSection = CreatePointersDiffSection(LastBlock);
                writer.Write(ptrDiffSection);

                MS.Position = 0x10;
                writer.Write(LastBlockPos);
                writer.Write(ptrDiffSection.Length);

                MS.Position = 0x4;
                writer.Write((int)MS.Length);

                #endregion

                return MS.ToArray();
            }
        }

        #endregion

        /// <summary>
        /// Create the section with the information to jump from
        /// pointer to pointer. That is, the difference between pointer
        /// positions.
        /// </summary>
        /// <param name="pointers">
        /// List of addresses where each pointer is written.
        /// </param>
        /// <returns>Bytes of the section.</returns>
        static byte[] CreatePointersDiffSection(IList<int> pointers)
        {
            List<byte> encodedDiffs = new List<byte>();
            for (int i = 0; i < pointers.Count; i++)
            {
                // Consecutive pointers
                int consecutive = 0;
                for (int j = i; j > 0 && j < pointers.Count; j++)
                {
                    int diff = pointers[j] - pointers[j - 1];

                    // Pointers are 32-bits so they are consecutive if the
                    // different of their position is their size.
                    if (diff == sizeof(UInt32))
                        consecutive++;
                    else
                        break;
                }

                if (consecutive >= 2)
                {
                    // If there are more than 2 consecutive pointers
                    // encoded the number of them.
                    // The maximum is 2^(8 bits - 3 bits flag) + 2 constant
                    consecutive = (consecutive > 33) ? 33 : consecutive;

                    int encoded = ((consecutive - 2) << 3) | 0b111;
                    encodedDiffs.Add((byte)encoded);

                    // Skip the encoding of those consecutive pointers
                    // (the loop will increase one already)
                    i += (consecutive - 1);
                }
                else
                {
                    // We encode the distance with the previous pointer.
                    // The first pointer is relative to the start of the
                    // section at 0x20.
                    int prevPtr = (i > 0) ? pointers[i - 1] : 0x20;
                    int diff = pointers[i] - prevPtr;

                    // We encode the multiple of 4
                    diff /= 4;

                    // Check how may bytes we need to encoded the diff.
                    if (diff < 128)
                    {
                        // Distance fits in 1 byte (1 bit of flag)
                        int encoded = (diff << 1) | 0b0;
                        encodedDiffs.Add((byte)encoded);
                    }
                    else if (diff < 16384)
                    {
                        // Distance fits in 2 bytes (2 bits of flag)
                        int encoded = (diff << 2) | 0b01;
                        encodedDiffs.Add((byte)(encoded & 0xFF));
                        encodedDiffs.Add((byte)(encoded >> 8));
                    }
                    else if (diff < 2097152)
                    {
                        // Distance fits in 3 bytes (3 bits of flag)
                        int encoded = (diff << 3) | 0b011;
                        encodedDiffs.Add((byte)(encoded & 0xFF));
                        encodedDiffs.Add((byte)((encoded >> 8) & 0xFF));
                        encodedDiffs.Add((byte)(encoded >> 16));
                    }
                    else
                    {
                        throw new FormatException("Pointer difference too big");
                    }
                }
            }

            return encodedDiffs.ToArray();
        }
    }
}