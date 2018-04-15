using Microsoft.Win32;
using PersonaEditorLib.Extension;
using PersonaEditorLib.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PersonaEditorLib.Utilities
{
    public static class PersonaFile
    {
        public static Dictionary<FileType, string> PersonaFileFilter = new Dictionary<FileType, string>()
        {
            { FileType.TMX, "|PNG file|*.PNG" },
            { FileType.BMD, "|Persona Text Project|*.PTP" },
            { FileType.FNT, "|PNG file|*.PNG" }
        };

        static Dictionary<FileType, ContextMenuItems[]> PersonaFileContextMenu = new Dictionary<FileType, ContextMenuItems[]>()
        {
            { FileType.BF, new ContextMenuItems[] {ContextMenuItems.Replace, ContextMenuItems.Separator, ContextMenuItems.SaveAs, ContextMenuItems.SaveAll} }

        };


        public static List<Tuple<FileType, string>> FileInfo = new List<Tuple<FileType, string>>()
            {
            new Tuple<FileType, string>(FileType.DAT, "Raw Data (*.*)|*.*"),
            new Tuple<FileType, string>(FileType.BIN, "BIN file (*.BIN)|*.BIN|PAK file (*.PAK)|*.PAK"),
            new Tuple<FileType, string>(FileType.SPR, "SPR file (*.SPR)|*.SPR"),
            new Tuple<FileType, string>(FileType.TMX, "TMX file (*.TMX)|*.TMX|PNG file (*.PNG)|*.PNG"),
            new Tuple<FileType, string>(FileType.BF, "BF file (*.BF)|*.BF"),
            new Tuple<FileType, string>(FileType.BMD, "|Persona Text Project (*.PTP)|*.PTP;.BMD"),
            new Tuple<FileType, string>(FileType.PTP, "|Persona Text Project (*.PTP)|*.PTP"),
            new Tuple<FileType, string>(FileType.FNT, "Persona Font (*.FNT)|*.FNT"),
            new Tuple<FileType, string>(FileType.BVP, "BVP file (*.BVP)|*.BVP")
            };

        public static void SaveImageFile(ObjectFile objectFile, string path)
        {
            if (objectFile.Object is IImage image)
                Imaging.SavePNG(image.GetImage(), path);
        }

        public static void OpenImageFile(ObjectFile objectFile, string path)
        {
            if (objectFile.Object is IImage image)
                if (File.Exists(path))
                    image.SetImage(Imaging.OpenPNG(path));
        }

        public static void SavePTPFile(ObjectFile objectFile, string path, PersonaEncoding.PersonaEncoding oldEncoding = null)
        {
            if (objectFile.Object is FileStructure.Text.BMD bmd)
            {
                FileStructure.Text.PTP PTP = new FileStructure.Text.PTP(bmd);
                if (oldEncoding != null)
                    PTP.CopyOld2New(oldEncoding);
                File.WriteAllBytes(path, PTP.Get());
            }
        }

        public static void OpenPTPFile(ObjectFile objectFile, string path, PersonaEncoding.PersonaEncoding newEncoding)
        {
            if (objectFile.Object is FileStructure.Text.BMD bmd)
                if (File.Exists(path))
                {
                    FileStructure.Text.PTP PTP = new FileStructure.Text.PTP(File.ReadAllBytes(path));
                    bmd.Open(PTP, newEncoding);
                }
        }

        public static void OpenPersonaFileDialog(ObjectFile objectFile, PersonaEncoding.PersonaEncoding encoding = null)
        {

        }

        public static ObjectFile OpenFile(string name, byte[] data, FileType type)
        {
            try
            {
                object Obj;

                if (type == FileType.BIN)
                    Obj = new FileStructure.Container.BIN(data);
                else if (type == FileType.SPR)
                    Obj = new FileStructure.SPR.SPR(data);
                else if (type == FileType.TMX)
                    Obj = new FileStructure.Graphic.TMX(data);
                else if (type == FileType.BF)
                    Obj = new FileStructure.Container.BF(data, name);
                else if (type == FileType.PM1)
                    Obj = new FileStructure.Container.PM1(data);
                else if (type == FileType.BMD)
                    Obj = new FileStructure.Text.BMD(data);
                else if (type == FileType.PTP)
                    Obj = new FileStructure.Text.PTP(data);
                else if (type == FileType.FNT)
                    Obj = new FileStructure.FNT.FNT(data);
                else if (type == FileType.BVP)
                    Obj = new FileStructure.Container.BVP(name, data);
                else if (type == FileType.TBL)
                    Obj = new FileStructure.Container.TBL(data, name);
                else if (type == FileType.FTD)
                    Obj = new FileStructure.Text.FTD(data);
                else if (type == FileType.DDS)
                    Obj = new FileStructure.Graphic.DDS(data);
                else if (type == FileType.SPD)
                    Obj = new FileStructure.SPR.SPD(data);
                else
                    Obj = new FileStructure.DAT(data);

                return new ObjectFile(name, Obj);
            }
            catch
            {
                return new ObjectFile(name, null);
            }
        }

        public static FileType GetFileType(string name)
        {
            string ext = Path.GetExtension(name).ToLower().TrimEnd(' ');
            if (ext == ".bin" | ext== ".pak" | ext == ".pac" | ext == ".p00" | ext == ".arc" | ext == ".dds2")
                return FileType.BIN;
            else if (ext.ToLower() == ".spr")
                return FileType.SPR;
            else if (ext.ToLower() == ".tmx")
                return FileType.TMX;
            else if (ext.ToLower() == ".bf")
                return FileType.BF;
            else if (ext.ToLower() == ".pm1")
                return FileType.PM1;
            else if (ext.ToLower() == ".bmd" | ext.ToLower() == ".msg")
                return FileType.BMD;
            else if (ext.ToLower() == ".ptp")
                return FileType.PTP;
            else if (ext.ToLower() == ".fnt")
                return FileType.FNT;
            else if (ext.ToLower() == ".bvp")
                return FileType.BVP;
            else if (ext.ToLower() == ".tbl")
                return FileType.TBL;
            else if (ext.ToLower() == ".dds")
                return FileType.DDS;
            else if (ext.ToLower() == ".spd")
                return FileType.SPD;
            //else if (ext.ToLower() == ".ctd")
            //    return FileType.FTD;
            else
                return FileType.DAT;
        }

        public static FileType GetFileType(byte[] data)
        {
            if (data.Length >= 0xc)
            {
                byte[] buffer = data.SubArray(8, 4);
                if (buffer.SequenceEqual(new byte[] { 0x31, 0x47, 0x53, 0x4D }) | buffer.SequenceEqual(new byte[] { 0x4D, 0x53, 0x47, 0x31 }))
                    return FileType.BMD;
                else if (buffer.SequenceEqual(new byte[] { 0x54, 0x4D, 0x58, 0x30 }))
                    return FileType.TMX;
                else if (buffer.SequenceEqual(new byte[] { 0x53, 0x50, 0x52, 0x30 }))
                    return FileType.SPR;
                else if (buffer.SequenceEqual(new byte[] { 0x46, 0x4C, 0x57, 0x30 }))
                    return FileType.BF;
                else if (buffer.SequenceEqual(new byte[] { 0x50, 0x4D, 0x44, 0x31 }))
                    return FileType.PM1;
            }
            return FileType.Unknown;
        }

        public static List<ContextMenuItems> GetContextMenuItems(FileType type)
        {
            List<ContextMenuItems> returned = new List<ContextMenuItems>();

            switch (type)
            {
                case FileType.BF:
                case FileType.BIN:
                case FileType.BVP:
                case FileType.PM1:
                case FileType.DAT:
                case FileType.FNT:
                case FileType.StringList:
                case FileType.TBL:
                case FileType.TMX:
                    returned.Add(ContextMenuItems.Replace);
                    break;
                case FileType.BMD:
                case FileType.PTP:
                case FileType.SPR:
                    returned.Add(ContextMenuItems.Edit);
                    returned.Add(ContextMenuItems.Replace);
                    break;
            }

            returned.Add(ContextMenuItems.Separator);

            switch (type)
            {
                case FileType.BF:
                case FileType.BIN:
                case FileType.BVP:
                case FileType.PM1:
                case FileType.SPR:
                case FileType.TBL:
                    returned.Add(ContextMenuItems.SaveAs);
                    returned.Add(ContextMenuItems.SaveAll);
                    break;
                case FileType.BMD:
                case FileType.DAT:
                case FileType.FNT:
                case FileType.PTP:
                case FileType.StringList:
                case FileType.TMX:
                    returned.Add(ContextMenuItems.SaveAs);
                    break;
            }

            return returned;
        }
    }
}