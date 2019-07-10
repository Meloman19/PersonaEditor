using AuxiliaryLibraries.Extensions;
using PersonaEditorLib.Other;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PersonaEditorLib
{
    public static class GameFormatHelper
    {
        public static Dictionary<string, FormatEnum> FileTypeDic = new Dictionary<string, FormatEnum>()
        {
            //Containers
            { ".bin", FormatEnum.BIN },
            { ".pak",  FormatEnum.BIN },
            { ".pac",  FormatEnum.BIN },
            { ".p00",  FormatEnum.BIN },
            { ".p01",  FormatEnum.BIN },
            { ".arc",  FormatEnum.BIN },
            { ".dds2", FormatEnum.BIN },

            { ".bf",  FormatEnum.BF  },
            { ".pm1", FormatEnum.PM1 },
            { ".bvp", FormatEnum.BVP },
            { ".tbl", FormatEnum.TBL },

            { ".ctd", FormatEnum.FTD },
            { ".ftd", FormatEnum.FTD },
            { ".ttd", FormatEnum.FTD },

            //Graphic containers
            { ".spr", FormatEnum.SPR },
            { ".spd", FormatEnum.SPD },

            //Graphic
            { ".fnt", FormatEnum.FNT },
            { ".tmx", FormatEnum.TMX },
            { ".dds", FormatEnum.DDS },

            //Text
            { ".bmd", FormatEnum.BMD },
            { ".msg", FormatEnum.BMD },
            { ".ptp", FormatEnum.PTP }
        };

        /// <summary>
        /// Tries to open a file with the specified data type.
        /// </summary>
        /// <param name="name">Name of file</param>
        /// <param name="data">Data of file</param>
        /// <param name="type">Type of file</param>
        /// <returns>Return ObjectContainer for this file or null if an error occurred.</returns>
        public static GameFile OpenFile(string name, byte[] data, FormatEnum type)
        {
            try
            {
                IGameData Obj;

                if (type == FormatEnum.BIN)
                    Obj = new FileContainer.BIN(data);
                else if (type == FormatEnum.SPR)
                    Obj = new SpriteContainer.SPR(data);
                else if (type == FormatEnum.TMX)
                    Obj = new Sprite.TMX(data);
                else if (type == FormatEnum.BF)
                    Obj = new FileContainer.BF(data, name);
                else if (type == FormatEnum.PM1)
                    Obj = new FileContainer.PM1(data);
                else if (type == FormatEnum.BMD)
                    Obj = new Text.BMD(data);
                else if (type == FormatEnum.PTP)
                    Obj = new Text.PTP(data);
                else if (type == FormatEnum.FNT)
                    Obj = new FNT(data);
                else if (type == FormatEnum.FNT0)
                    Obj = new FNT0(data);
                else if (type == FormatEnum.BVP)
                    Obj = new FileContainer.BVP(name, data);
                else if (type == FormatEnum.TBL)
                    try
                    {
                        Obj = new FileContainer.TBL(data, name);
                    }
                    catch
                    {
                        Obj = new FileContainer.BIN(data);
                    }
                else if (type == FormatEnum.FTD)
                    Obj = new FTD(data);
                else if (type == FormatEnum.DDS)
                    try
                    {
                        Obj = new Sprite.DDS(data);
                    }
                    catch
                    {
                        Obj = new Sprite.DDSAtlus(data);
                    }
                else if (type == FormatEnum.SPD)
                    Obj = new SpriteContainer.SPD(data);
                else
                    Obj = new DAT(data);

                return new GameFile(name, Obj);
            }
            catch
            {
                return null;
            }
        }

        public static GameFile OpenFile(string name, byte[] data)
        {
            var format = GetFormat(data);
            if (format == FormatEnum.Unknown)
                format = GetFormat(name);

            return OpenFile(name, data, format);
        }

        public static FormatEnum GetFormat(string name)
        {
            string ext = Path.GetExtension(name).ToLower().TrimEnd(' ');
            if (FileTypeDic.ContainsKey(ext))
                return FileTypeDic[ext];
            else
                return FormatEnum.DAT;
        }

        public static FormatEnum GetFormat(byte[] data)
        {
            if (data.Length >= 0xc)
            {
                byte[] buffer = data.SubArray(0, 4);
                if (buffer.SequenceEqual(new byte[] { 0x46, 0x4E, 0x54, 0x30 }))
                    return FormatEnum.FNT0;

                buffer = data.SubArray(8, 4);
                if (buffer.SequenceEqual(new byte[] { 0x31, 0x47, 0x53, 0x4D }) | buffer.SequenceEqual(new byte[] { 0x4D, 0x53, 0x47, 0x31 }))
                    return FormatEnum.BMD;
                else if (buffer.SequenceEqual(new byte[] { 0x54, 0x4D, 0x58, 0x30 }))
                    return FormatEnum.TMX;
                else if (buffer.SequenceEqual(new byte[] { 0x53, 0x50, 0x52, 0x30 }))
                    return FormatEnum.SPR;
                else if (buffer.SequenceEqual(new byte[] { 0x46, 0x4C, 0x57, 0x30 }))
                    return FormatEnum.BF;
                else if (buffer.SequenceEqual(new byte[] { 0x50, 0x4D, 0x44, 0x31 }))
                    return FormatEnum.PM1;
            }
            return FormatEnum.Unknown;
        }
    }
}